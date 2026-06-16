// SPDX-License-Identifier: Apache-2.0
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PtpLabClock.App.Commands;
using PtpLabClock.Core.Abstractions;
using PtpLabClock.Core.Diagnostics;
using PtpLabClock.Core.Engine;
using PtpLabClock.Core.Health;
using PtpLabClock.Core.Monitor;
using PtpLabClock.Core.Transports;
using PtpLabClock.Pcap.Adapters;
using PtpLabClock.Pcap.Transport;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Reporting;

namespace PtpLabClock.App.ViewModels;

public sealed class MainViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly PtpTimingHealthValidator _healthValidator = new();
    private PtpMasterEngine? _engine;
    private NetworkAdapterInfoDto? _selectedAdapter;
    private PtpProfilePreset _selectedProfile = PtpProfilePreset.Iec61850_9_3_Lab;
    private string _sourceMac = "02-00-00-00-00-01";
    private string _clockIdentity = "02-00-00-FF-FE-00-00-01";
    private int _domainNumber;
    private int _clockClass = 248;
    private string _stateText = "STOPPED";
    private string _adapterStatusText = "Demo Mode is always available. RAW mode uses Npcap/SharpPcap and requires administrator privileges on Windows.";
    private PtpRuntimeCounters _counters = new();
    private string _healthOverallText = "WAITING";
    private string _healthSummaryText = "No monitor data yet";
    private int _healthPassCount;
    private int _healthWarnCount;
    private int _healthFailCount;
    private DateTime _sessionStartedAt = DateTime.Now;
    private PtpMonitorSnapshot? _latestMonitorSnapshot;
    private PtpHealthSnapshot? _latestHealthSnapshot;

    public MainViewModel()
    {
        RefreshCommand = new RelayCommand(_ => RefreshAdapters());
        StartCommand = new AsyncRelayCommand(_ => StartAsync(), _ => SelectedAdapter is not null && _engine is null);
        StopCommand = new AsyncRelayCommand(_ => StopAsync(), _ => _engine is not null);
        ScenarioCommand = new RelayCommand(p => _engine?.ApplyScenario(p?.ToString() ?? string.Empty), _ => _engine is not null);
        ResetScenarioCommand = new RelayCommand(_ => _engine?.ResetScenarios(), _ => _engine is not null);
        ExportReportCommand = new RelayCommand(_ => ExportReport());
        ExportPackageCommand = new RelayCommand(_ => ExportPackage());
        ResetHealthCards();
        RefreshAdapters();
    }

    public ObservableCollection<NetworkAdapterInfoDto> Adapters { get; } = new();
    public ObservableCollection<PtpEventLogItem> Events { get; } = new();
    public ObservableCollection<HealthCheckCardViewModel> HealthChecks { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ScenarioCommand { get; }
    public ICommand ResetScenarioCommand { get; }
    public ICommand ExportReportCommand { get; }
    public ICommand ExportPackageCommand { get; }

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
        : "RAW Ethernet via Npcap. Sends and captures Layer-2 PTP frames on the selected adapter.";

    public long AnnounceTx => _counters.AnnounceTx;
    public long SyncTx => _counters.SyncTx;
    public long FollowUpTx => _counters.FollowUpTx;
    public long PdelayReqRx => _counters.PdelayReqRx;
    public long PdelayRespTx => _counters.PdelayRespTx;
    public long PdelayFollowUpTx => _counters.PdelayRespFollowUpTx;
    public long Errors => _counters.PacketErrors;

    public string HealthOverallText { get => _healthOverallText; private set => Set(ref _healthOverallText, value); }
    public string HealthSummaryText { get => _healthSummaryText; private set => Set(ref _healthSummaryText, value); }
    public int HealthPassCount { get => _healthPassCount; private set => Set(ref _healthPassCount, value); }
    public int HealthWarnCount { get => _healthWarnCount; private set => Set(ref _healthWarnCount, value); }
    public int HealthFailCount { get => _healthFailCount; private set => Set(ref _healthFailCount, value); }

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
            foreach (var adapter in GetRawAdapters())
                Adapters.Add(adapter);

            SelectedAdapter = demo;
            AdapterStatusText = $"{Adapters.Count - 1} RAW adapter candidate(s) listed. Demo Mode is selected by default; RAW transport uses Npcap/SharpPcap in this build.";
            AddEvent("INFO", "ADAPTER", $"{Adapters.Count - 1} RAW adapter candidate(s) listed. Select a real Ethernet adapter for RAW packet injection, or keep Demo Mode for UI validation.");
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


    private static IReadOnlyList<NetworkAdapterInfoDto> GetRawAdapters()
    {
        try
        {
            return new NpcapAdapterProvider().GetAdapters();
        }
        catch
        {
            return Array.Empty<NetworkAdapterInfoDto>();
        }
    }

    private static IPtpTransport CreateTransport(NetworkAdapterInfoDto adapter)
    {
        return adapter.IsDemo ? new MockPtpTransport() : new NpcapPtpTransport();
    }

    private async Task StartAsync()
    {
        if (SelectedAdapter is null) return;

        IPtpTransport transport = CreateTransport(SelectedAdapter);

        _engine = new PtpMasterEngine(transport);
        _engine.EventLogged += OnEngineEvent;
        _engine.CountersUpdated += OnCountersUpdated;
        _engine.MonitorSnapshotUpdated += OnMonitorSnapshotUpdated;
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
            _sessionStartedAt = DateTime.Now;
            _latestMonitorSnapshot = null;
            _latestHealthSnapshot = null;
            ResetCounters();
            ResetHealthCards();
            await _engine.StartAsync(options);
            AddEvent("INFO", "MODE", SelectedAdapter.IsDemo
                ? "Demo transport started. Counters will move, but no network packets are transmitted."
                : "RAW transport started. Validate packets in Wireshark with eth.type == 0x88f7.");
            RaiseCommandStates();
        }
        catch (Exception ex)
        {
            AddEvent("ERROR", "START", ex.Message);
            AddEvent("INFO", "HINT", "RAW start failed. Run as Administrator, confirm Npcap is installed, select a wired Ethernet adapter, and verify Wireshark can capture on the same NIC.");
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
            _engine.MonitorSnapshotUpdated -= OnMonitorSnapshotUpdated;
            await _engine.DisposeAsync();
            _engine = null;
        }
    }

    private void OnEngineEvent(object? sender, PtpEngineEventArgs e) => Dispatch(() => AddEvent(e.Item));

    private void OnMonitorSnapshotUpdated(object? sender, PtpMonitorSnapshot snapshot)
    {
        var domain = (byte)Math.Clamp(DomainNumber, 0, 255);
        var health = _healthValidator.Evaluate(snapshot, PtpHealthValidatorOptions.ForLabDomain(domain));
        Dispatch(() =>
        {
            _latestMonitorSnapshot = snapshot;
            _latestHealthSnapshot = health;
            UpdateHealthCards(health);
        });
    }

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

    private void ResetHealthCards()
    {
        HealthOverallText = "WAITING";
        HealthSummaryText = "Start Demo/Monitor traffic to evaluate PTP health.";
        HealthPassCount = 0;
        HealthWarnCount = 0;
        HealthFailCount = 0;

        HealthChecks.Clear();
        foreach (var name in new[]
        {
            "PTP Visibility",
            "Domain Match",
            "GM Stability",
            "Follow_Up Pairing",
            "Pdelay Activity",
            "Sequence Continuity",
            "Analyzer Readiness"
        })
        {
            HealthChecks.Add(new HealthCheckCardViewModel
            {
                Name = name,
                Level = "INFO",
                Summary = "Waiting",
                Detail = "No monitor data yet."
            });
        }
    }

    private void UpdateHealthCards(PtpHealthSnapshot health)
    {
        HealthOverallText = health.OverallLevel.ToString().ToUpperInvariant();
        HealthPassCount = health.PassCount;
        HealthWarnCount = health.WarningCount;
        HealthFailCount = health.FailCount;
        HealthSummaryText = $"PASS {health.PassCount} · WARN {health.WarningCount} · FAIL {health.FailCount}";

        HealthChecks.Clear();
        foreach (var check in health.Checks)
        {
            HealthChecks.Add(new HealthCheckCardViewModel
            {
                Name = check.Name,
                Level = check.Level.ToString().ToUpperInvariant(),
                Summary = check.Summary,
                Detail = check.Detail
            });
        }
    }

    private void ExportReport()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export PTP Health PDF Report",
                Filter = "PDF report (*.pdf)|*.pdf",
                FileName = $"ptp-health-report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf",
                AddExtension = true,
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            var data = BuildReportData();
            PtpSessionReportGenerator.GeneratePdf(data, dialog.FileName);
            AddEvent("INFO", "REPORT", $"PDF exported: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            AddEvent("ERROR", "REPORT", ex.Message);
        }
    }


    private void ExportPackage()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export Session Evidence Package",
                Filter = "Session evidence package (*.zip)|*.zip",
                FileName = $"ptp-session-package-{DateTime.Now:yyyyMMdd-HHmmss}.zip",
                AddExtension = true,
                DefaultExt = ".zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            var data = BuildReportData();
            PtpSessionPackageExporter.ExportZip(data, dialog.FileName);
            AddEvent("INFO", "PACKAGE", $"Session package exported: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            AddEvent("ERROR", "PACKAGE", ex.Message);
        }
    }

    private PtpSessionReportData BuildReportData()
    {
        var domain = (byte)Math.Clamp(DomainNumber, 0, 255);
        return new PtpSessionReportData
        {
            ProjectName = "Process Bus Timing Lab",
            OperatorName = Environment.UserName,
            GeneratedAt = DateTime.Now,
            SessionStartedAt = _sessionStartedAt,
            SessionEndedAt = DateTime.Now,
            AdapterName = SelectedAdapter?.Description ?? "Not selected",
            ProfileName = SelectedProfile.ToString(),
            DomainNumber = domain,
            Mode = AdapterModeText,
            Counters = _counters.Clone(),
            MonitorSnapshot = _latestMonitorSnapshot,
            HealthSnapshot = _latestHealthSnapshot,
            Events = Events.ToArray()
        };
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
        (ExportReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ExportPackageCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public async ValueTask DisposeAsync() => await DisposeEngineAsync();
}

public sealed class HealthCheckCardViewModel
{
    public string Name { get; init; } = string.Empty;
    public string Level { get; init; } = "INFO";
    public string Summary { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
}
