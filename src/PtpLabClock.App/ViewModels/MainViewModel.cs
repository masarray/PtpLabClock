// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PtpLabClock.App.Commands;
using PtpLabClock.Core.Abstractions;
using PtpLabClock.Core.Diagnostics;
using PtpLabClock.Core.Engine;
using PtpLabClock.Core.Transports;
using PtpLabClock.Pcap.Adapters;
using PtpLabClock.Pcap.Transport;
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.App.ViewModels;

public sealed class MainViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly NpcapAdapterProvider _adapterProvider = new();
    private PtpMasterEngine? _engine;
    private NetworkAdapterInfoDto? _selectedAdapter;
    private PtpProfilePreset _selectedProfile = PtpProfilePreset.Iec61850_9_3_Lab;
    private string _sourceMac = "02-00-00-00-00-01";
    private string _clockIdentity = "02-00-00-FF-FE-00-00-01";
    private int _domainNumber;
    private int _clockClass = 248;
    private string _stateText = "STOPPED";
    private string _adapterStatusText = "Demo Mode is always available. RAW mode requires Npcap + Administrator.";
    private PtpRuntimeCounters _counters = new();

    public MainViewModel()
    {
        RefreshCommand = new RelayCommand(_ => RefreshAdapters());
        StartCommand = new AsyncRelayCommand(_ => StartAsync(), _ => SelectedAdapter is not null && _engine is null);
        StopCommand = new AsyncRelayCommand(_ => StopAsync(), _ => _engine is not null);
        ScenarioCommand = new RelayCommand(p => _engine?.ApplyScenario(p?.ToString() ?? string.Empty), _ => _engine is not null);
        ResetScenarioCommand = new RelayCommand(_ => _engine?.ResetScenarios(), _ => _engine is not null);
        RefreshAdapters();
    }

    public ObservableCollection<NetworkAdapterInfoDto> Adapters { get; } = new();
    public ObservableCollection<PtpEventLogItem> Events { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ScenarioCommand { get; }
    public ICommand ResetScenarioCommand { get; }

    public NetworkAdapterInfoDto? SelectedAdapter
    {
        get => _selectedAdapter;
        set
        {
            if (Set(ref _selectedAdapter, value))
            {
                Notify(nameof(AdapterModeText));
                Notify(nameof(AdapterModeDetail));
                RaiseCommandStates();
            }
        }
    }

    public PtpProfilePreset SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (Set(ref _selectedProfile, value))
            {
                Notify(nameof(IsIecProfile));
                Notify(nameof(IsAnalyzerProfile));
                Notify(nameof(IsGenericProfile));
            }
        }
    }

    public bool IsIecProfile
    {
        get => SelectedProfile == PtpProfilePreset.Iec61850_9_3_Lab;
        set { if (value) SelectedProfile = PtpProfilePreset.Iec61850_9_3_Lab; }
    }

    public bool IsAnalyzerProfile
    {
        get => SelectedProfile == PtpProfilePreset.AnalyzerTest;
        set { if (value) SelectedProfile = PtpProfilePreset.AnalyzerTest; }
    }

    public bool IsGenericProfile
    {
        get => SelectedProfile == PtpProfilePreset.GenericPtpV2;
        set { if (value) SelectedProfile = PtpProfilePreset.GenericPtpV2; }
    }

    public string SourceMac { get => _sourceMac; set => Set(ref _sourceMac, value); }
    public string ClockIdentity { get => _clockIdentity; set => Set(ref _clockIdentity, value); }
    public int DomainNumber { get => _domainNumber; set => Set(ref _domainNumber, value); }
    public int ClockClass { get => _clockClass; set => Set(ref _clockClass, value); }
    public string StateText { get => _stateText; set => Set(ref _stateText, value); }
    public string AdapterStatusText { get => _adapterStatusText; set => Set(ref _adapterStatusText, value); }

    public string AdapterModeText => SelectedAdapter?.IsDemo == true ? "DEMO MODE" : "RAW PACKET MODE";
    public string AdapterModeDetail => SelectedAdapter?.IsDemo == true
        ? "UI + engine simulation. No packets are sent to the network."
        : "Npcap raw Ethernet. Run elevated for transmit/capture.";

    public long AnnounceTx => _counters.AnnounceTx;
    public long SyncTx => _counters.SyncTx;
    public long FollowUpTx => _counters.FollowUpTx;
    public long PdelayReqRx => _counters.PdelayReqRx;
    public long PdelayRespTx => _counters.PdelayRespTx;
    public long PdelayFollowUpTx => _counters.PdelayRespFollowUpTx;
    public long Errors => _counters.PacketErrors;

    private void RefreshAdapters()
    {
        Adapters.Clear();
        var demo = new NetworkAdapterInfoDto
        {
            Id = "mock://ui-demo",
            Name = "Demo Engine",
            Description = "Demo Mode - UI/engine simulator, no Npcap required",
            IsDemo = true
        };
        Adapters.Add(demo);

        try
        {
            foreach (var adapter in _adapterProvider.GetAdapters())
                Adapters.Add(adapter);

            SelectedAdapter = Adapters.FirstOrDefault(a => !a.IsDemo) ?? demo;
            AdapterStatusText = $"{Adapters.Count - 1} raw adapter(s) detected. Demo Mode remains available for UI validation.";
            AddEvent("INFO", "ADAPTER", $"{Adapters.Count - 1} raw adapter(s) detected. Use Demo Mode if Npcap/Admin is not ready.");
        }
        catch (Exception ex)
        {
            SelectedAdapter = demo;
            AdapterStatusText = "Npcap adapter scan failed. Demo Mode selected automatically.";
            AddEvent("WARN", "NPCAP", ex.Message);
            AddEvent("INFO", "UI", "Demo Mode is active so the app can still run without raw packet access.");
        }

        RaiseCommandStates();
    }

    private async Task StartAsync()
    {
        if (SelectedAdapter is null) return;

        IPtpTransport transport = SelectedAdapter.IsDemo
            ? new MockPtpTransport()
            : new NpcapPtpTransport();

        _engine = new PtpMasterEngine(transport);
        _engine.EventLogged += OnEngineEvent;
        _engine.CountersUpdated += OnCountersUpdated;
        _engine.StateChanged += (_, state) => Dispatch(() => StateText = state.ToString().ToUpperInvariant());

        var options = new PtpEngineOptions
        {
            AdapterId = SelectedAdapter.Id,
            AdapterName = SelectedAdapter.Description,
            SourceMac = SourceMac,
            ClockIdentity = ClockIdentity,
            ProfilePreset = SelectedProfile,
            DomainNumber = (byte)Math.Clamp(DomainNumber, 0, 255),
            ClockClass = (byte)Math.Clamp(ClockClass, 0, 255),
            ClockAccuracy = PtpClockAccuracy.Unknown,
            Priority1 = 128,
            Priority2 = 128,
            EnablePdelayResponder = true,
            EnableFollowUp = true,
            TwoStep = true
        };

        try
        {
            Events.Clear();
            ResetCounters();
            await _engine.StartAsync(options);
            AddEvent("INFO", "MODE", SelectedAdapter.IsDemo
                ? "Demo transport started. Counters will move, but no network packets are transmitted."
                : "Raw Npcap transport started. Validate packets in Wireshark with eth.type == 0x88f7.");
            RaiseCommandStates();
        }
        catch (Exception ex)
        {
            AddEvent("ERROR", "START", ex.Message);
            AddEvent("INFO", "HINT", "For RAW mode: install Npcap, use x64, and run Visual Studio/app as Administrator. Demo Mode should run without those requirements.");
            await DisposeEngineAsync();
            StateText = "STOPPED";
            RaiseCommandStates();
        }
    }

    private async Task StopAsync()
    {
        await DisposeEngineAsync();
        StateText = "STOPPED";
        RaiseCommandStates();
    }

    private async Task DisposeEngineAsync()
    {
        if (_engine is not null)
        {
            _engine.EventLogged -= OnEngineEvent;
            _engine.CountersUpdated -= OnCountersUpdated;
            await _engine.DisposeAsync();
            _engine = null;
        }
    }

    private void OnEngineEvent(object? sender, PtpEngineEventArgs e) => Dispatch(() => AddEvent(e.Item));


    private void ResetCounters()
    {
        _counters = new PtpRuntimeCounters();
        Notify(nameof(AnnounceTx));
        Notify(nameof(SyncTx));
        Notify(nameof(FollowUpTx));
        Notify(nameof(PdelayReqRx));
        Notify(nameof(PdelayRespTx));
        Notify(nameof(PdelayFollowUpTx));
        Notify(nameof(Errors));
    }

    private void OnCountersUpdated(object? sender, PtpCountersEventArgs e)
    {
        Dispatch(() =>
        {
            _counters = e.Counters;
            Notify(nameof(AnnounceTx));
            Notify(nameof(SyncTx));
            Notify(nameof(FollowUpTx));
            Notify(nameof(PdelayReqRx));
            Notify(nameof(PdelayRespTx));
            Notify(nameof(PdelayFollowUpTx));
            Notify(nameof(Errors));
        });
    }

    private void AddEvent(string severity, string source, string message) => AddEvent(new PtpEventLogItem { Timestamp = DateTime.Now, Severity = severity, Source = source, Message = message });

    private void AddEvent(PtpEventLogItem item)
    {
        Events.Insert(0, item);
        while (Events.Count > 300) Events.RemoveAt(Events.Count - 1);
    }

    private static void Dispatch(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action);
    }

    private void RaiseCommandStates()
    {
        (StartCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (StopCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ScenarioCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetScenarioCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public async ValueTask DisposeAsync() => await DisposeEngineAsync();
}
