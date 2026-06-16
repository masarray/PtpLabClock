// SPDX-License-Identifier: Apache-2.0
using System.Threading.Channels;
using PtpLabClock.Core.Abstractions;
using PtpLabClock.Core.Diagnostics;
using PtpLabClock.Core.Monitor;
using PtpLabClock.Core.Scheduling;
using PtpLabClock.Protocol;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Ethernet;
using PtpLabClock.Protocol.Messages;
using PtpLabClock.Protocol.Serialization;

namespace PtpLabClock.Core.Engine;

public sealed class PtpMasterEngine : IAsyncDisposable
{
    private readonly IPtpTransport _transport;
    private readonly PtpMessageSerializer _serializer = new();
    private readonly SequenceIdManager _sequence = new();
    private readonly PtpRuntimeCounters _counters = new();
    private readonly PtpPassiveMonitor _passiveMonitor = new();
    private readonly object _counterGate = new();

    private CancellationTokenSource? _cts;
    private Channel<PtpTxItem>? _txQueue;
    private Task? _txTask;
    private Task? _announceTask;
    private Task? _syncTask;

    private PtpEngineOptions? _options;
    private PtpBuildOptions? _buildOptions;
    private MacAddress _sourceMac = MacAddress.Parse("02-00-00-00-00-01");

    private volatile bool _dropAnnounce;
    private volatile bool _dropFollowUp;
    private volatile bool _dropPdelay;

    public PtpMasterEngine(IPtpTransport transport)
    {
        _transport = transport;
        _transport.PacketReceived += OnPacketReceived;
        _passiveMonitor.SnapshotUpdated += (_, snapshot) => MonitorSnapshotUpdated?.Invoke(this, snapshot);
    }

    public PtpEngineState State { get; private set; } = PtpEngineState.Stopped;

    public event EventHandler<PtpEngineEventArgs>? EventLogged;
    public event EventHandler<PtpCountersEventArgs>? CountersUpdated;
    public event EventHandler<PtpEngineState>? StateChanged;
    public event EventHandler<PtpFrameObservedEventArgs>? FrameObserved;
    public event EventHandler<PtpMonitorSnapshot>? MonitorSnapshotUpdated;

    public async Task StartAsync(PtpEngineOptions options, CancellationToken cancellationToken = default)
    {
        if (State == PtpEngineState.Running)
            return;

        SetState(PtpEngineState.Starting);
        _options = options;
        _sourceMac = MacAddress.Parse(options.SourceMac);
        _buildOptions = ToBuildOptions(options);
        _cts = new CancellationTokenSource();
        _txQueue = Channel.CreateBounded<PtpTxItem>(new BoundedChannelOptions(512)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        try
        {
            await _transport.OpenAsync(options.AdapterId, cancellationToken).ConfigureAwait(false);
            _txTask = Task.Run(() => RunTxLoopAsync(_cts.Token), CancellationToken.None);
            await _transport.StartCaptureAsync(cancellationToken).ConfigureAwait(false);

            if (options.EnableAnnounce)
                _announceTask = RunPeriodicAsync("Announce", options.AnnounceIntervalMs, SendAnnounceAsync, _cts.Token);

            if (options.EnableSync)
                _syncTask = RunPeriodicAsync("Sync", options.SyncIntervalMs, SendSyncFollowUpAsync, _cts.Token);

            SetState(PtpEngineState.Running);
            Log("INFO", "ENGINE", $"Started on {options.AdapterName}. Domain={options.DomainNumber}, Profile={options.ProfilePreset}.");
        }
        catch
        {
            SetState(PtpEngineState.Faulted);
            _cts.Cancel();
            _txQueue.Writer.TryComplete();
            try { await _transport.StopCaptureAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (State == PtpEngineState.Stopped)
            return;

        _cts?.Cancel();
        _txQueue?.Writer.TryComplete();

        var activeTasks = new[] { _announceTask, _syncTask, _txTask }
            .Where(t => t is not null)
            .Cast<Task>()
            .ToArray();

        try
        {
            if (activeTasks.Length > 0)
                await Task.WhenAll(activeTasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected during stop
        }
        catch (Exception ex)
        {
            RecordError($"Stop cleanup warning: {ex.Message}");
        }

        try
        {
            await _transport.StopCaptureAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            RecordError($"Stop capture warning: {ex.Message}");
        }

        _announceTask = null;
        _syncTask = null;
        _txTask = null;
        _txQueue = null;
        _cts?.Dispose();
        _cts = null;

        SetState(PtpEngineState.Stopped);
        Log("INFO", "ENGINE", "Stopped.");
    }

    public void ResetScenarios()
    {
        _dropAnnounce = false;
        _dropFollowUp = false;
        _dropPdelay = false;
        if (_options is { } options)
        {
            options.DomainNumber = 0;
            options.ClockClass = 248;
            _buildOptions = ToBuildOptions(options);
        }
        Log("INFO", "SCENARIO", "All temporary fault scenarios reset.");
    }

    public void ApplyScenario(string scenario)
    {
        if (_options is null) return;

        switch (scenario)
        {
            case "GM_LOST":
                _dropAnnounce = true;
                Log("WARN", "SCENARIO", "GM Lost scenario active: Announce transmission suppressed.");
                break;
            case "MISSING_FOLLOW_UP":
                _dropFollowUp = true;
                Log("WARN", "SCENARIO", "Missing Follow_Up scenario active.");
                break;
            case "CLOCK_DEGRADED":
                _options.ClockClass = 255;
                _options.ClockAccuracy = PtpClockAccuracy.Unknown;
                _buildOptions = ToBuildOptions(_options);
                Log("WARN", "SCENARIO", "Clock quality degraded: ClockClass=255, Accuracy=Unknown.");
                break;
            case "SEQUENCE_JUMP":
                _sequence.Jump(PtpMessageType.Sync, 200);
                _sequence.Jump(PtpMessageType.Announce, 200);
                Log("WARN", "SCENARIO", "Sequence ID jump injected.");
                break;
            case "GM_SWITCH":
                _options.ClockIdentity = RandomClockIdentity();
                _buildOptions = ToBuildOptions(_options);
                Log("WARN", "SCENARIO", $"GM switch simulated. New ClockIdentity={_options.ClockIdentity}");
                break;
            case "STOP_PDELAY":
                _dropPdelay = true;
                Log("WARN", "SCENARIO", "Pdelay response suppressed.");
                break;
        }
    }

    private async Task RunPeriodicAsync(string name, int intervalMs, Func<CancellationToken, Task> action, CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(intervalMs, 100)));
        while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
        {
            try
            {
                await action(token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordError(ex.Message);
                Log("ERROR", name.ToUpperInvariant(), ex.Message);
            }
        }
    }

    private async Task RunTxLoopAsync(CancellationToken token)
    {
        if (_txQueue is null)
            return;

        await foreach (var item in _txQueue.Reader.ReadAllAsync(token).ConfigureAwait(false))
        {
            try
            {
                var inspection = PtpFrameInspector.Inspect(item.Frame, _options?.DomainNumber);
                if (!inspection.IsValid)
                {
                    RecordError(inspection.Error);
                    Log("ERROR", "TXCHK", inspection.Summary);
                    continue;
                }

                await _transport.SendAsync(item.Frame, token).ConfigureAwait(false);
                FrameObserved?.Invoke(this, new PtpFrameObservedEventArgs("TX", item.Kind.ToString(), item.Frame, inspection));
                MarkSent(item, inspection);

                if (!string.IsNullOrWhiteSpace(item.LogMessage))
                    Log(item.LogSeverity, item.LogSource, $"{item.LogMessage} [{inspection.Transport}, len={inspection.MessageLength}, offset={inspection.PtpOffset}]");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordError(ex.Message);
                Log("ERROR", "TX", ex.Message);
                SetState(PtpEngineState.Faulted);
                _cts?.Cancel();
                break;
            }
        }
    }

    private async Task SendAnnounceAsync(CancellationToken token)
    {
        if (_dropAnnounce || _buildOptions is null) return;

        var seq = _sequence.Next(PtpMessageType.Announce);
        var ptp = _serializer.BuildAnnounce(_buildOptions, seq);
        var frame = EthernetFrameBuilder.Build(PtpMulticastAddresses.General, _sourceMac, EtherTypes.Ptp, ptp);
        await EnqueueTxAsync(new PtpTxItem(frame, PtpTxKind.Announce, seq, "TX", "ANNOUNCE", $"Announce seq={seq} domain={_buildOptions.DomainNumber}."), token).ConfigureAwait(false);
    }

    private async Task SendSyncFollowUpAsync(CancellationToken token)
    {
        if (_buildOptions is null) return;

        var seq = _sequence.Next(PtpMessageType.Sync);
        var syncTimestamp = PtpTimestamp.Now();
        var sync = _serializer.BuildSync(_buildOptions, seq, syncTimestamp);
        var syncFrame = EthernetFrameBuilder.Build(PtpMulticastAddresses.General, _sourceMac, EtherTypes.Ptp, sync);
        await EnqueueTxAsync(new PtpTxItem(syncFrame, PtpTxKind.Sync, seq, "TX", "SYNC", $"Sync seq={seq}."), token).ConfigureAwait(false);

        if (!_dropFollowUp && _options?.EnableFollowUp == true)
        {
            await Task.Delay(8, token).ConfigureAwait(false);
            var follow = _serializer.BuildFollowUp(_buildOptions, seq, syncTimestamp);
            var followFrame = EthernetFrameBuilder.Build(PtpMulticastAddresses.General, _sourceMac, EtherTypes.Ptp, follow);
            await EnqueueTxAsync(new PtpTxItem(followFrame, PtpTxKind.FollowUp, seq, "TX", "FOLLOWUP", $"Follow_Up seq={seq} paired with Sync timestamp."), token).ConfigureAwait(false);
        }
    }

    private async void OnPacketReceived(object? sender, PtpPacketReceivedEventArgs e)
    {
        if (_options is not null)
        {
            var rxInspection = PtpFrameInspector.Inspect(e.Frame, _options.DomainNumber);
            if (rxInspection.IsValid)
            {
                _passiveMonitor.ObserveFrame(e.Frame, "RX", _options.DomainNumber);
                FrameObserved?.Invoke(this, new PtpFrameObservedEventArgs("RX", rxInspection.MessageType.ToString(), e.Frame, rxInspection));
            }
        }

        if (_buildOptions is null || _options is null || !_options.EnablePdelayResponder || _dropPdelay)
            return;

        if (!PtpPacketParser.TryReadPdelayReq(e.Frame, _options.DomainNumber, out var sequenceId, out var requester, out var rejectReason))
        {
            if (rejectReason.Contains("domain mismatch", StringComparison.OrdinalIgnoreCase))
                Log("WARN", "PDELAY", rejectReason);
            return;
        }

        try
        {
            UpdateCounters(counters =>
            {
                counters.PdelayReqRx++;
                counters.LastPdelayReqSeq = sequenceId;
                counters.LastPeerClockIdentity = ToClockIdentityText(requester);
            });

            var responseTimestamp = PtpTimestamp.Now();
            var resp = _serializer.BuildPdelayResp(_buildOptions, sequenceId, requester, responseTimestamp);
            var respFrame = EthernetFrameBuilder.Build(PtpMulticastAddresses.PeerDelay, _sourceMac, EtherTypes.Ptp, resp);
            await EnqueueTxAsync(new PtpTxItem(respFrame, PtpTxKind.PdelayResp, sequenceId, "TX", "PDELAY", $"Pdelay_Resp seq={sequenceId} to {ToClockIdentityText(requester)}."), _cts?.Token ?? CancellationToken.None).ConfigureAwait(false);

            await Task.Delay(8, _cts?.Token ?? CancellationToken.None).ConfigureAwait(false);
            var follow = _serializer.BuildPdelayRespFollowUp(_buildOptions, sequenceId, requester, responseTimestamp);
            var followFrame = EthernetFrameBuilder.Build(PtpMulticastAddresses.PeerDelay, _sourceMac, EtherTypes.Ptp, follow);
            await EnqueueTxAsync(new PtpTxItem(followFrame, PtpTxKind.PdelayRespFollowUp, sequenceId, "TX", "PDELAY", $"Pdelay_Resp_Follow_Up seq={sequenceId} paired with response timestamp."), _cts?.Token ?? CancellationToken.None).ConfigureAwait(false);

            Log("RX", "PDELAY", $"Pdelay_Req seq={sequenceId} received from {ToClockIdentityText(requester)}; response queued.");
        }
        catch (OperationCanceledException)
        {
            // expected during stop
        }
        catch (Exception ex)
        {
            RecordError(ex.Message);
            Log("ERROR", "PDELAY", ex.Message);
        }
    }

    private async ValueTask EnqueueTxAsync(PtpTxItem item, CancellationToken token)
    {
        var queue = _txQueue ?? throw new InvalidOperationException("TX queue is not ready.");
        await queue.Writer.WriteAsync(item, token).ConfigureAwait(false);
    }

    private void MarkSent(PtpTxItem item, PtpFrameValidationResult inspection)
    {
        UpdateCounters(counters =>
        {
            counters.LastTxSummary = inspection.Summary;
            switch (item.Kind)
            {
                case PtpTxKind.Announce:
                    counters.AnnounceTx++;
                    counters.LastAnnounceSeq = item.SequenceId;
                    break;
                case PtpTxKind.Sync:
                    counters.SyncTx++;
                    counters.LastSyncSeq = item.SequenceId;
                    break;
                case PtpTxKind.FollowUp:
                    counters.FollowUpTx++;
                    break;
                case PtpTxKind.PdelayResp:
                    counters.PdelayRespTx++;
                    break;
                case PtpTxKind.PdelayRespFollowUp:
                    counters.PdelayRespFollowUpTx++;
                    break;
            }
        });
    }

    private void RecordError(string message)
    {
        UpdateCounters(counters =>
        {
            counters.PacketErrors++;
            counters.LastTxError = message;
        });
    }

    private void UpdateCounters(Action<PtpRuntimeCounters> update)
    {
        PtpRuntimeCounters snapshot;
        lock (_counterGate)
        {
            update(_counters);
            snapshot = _counters.Clone();
        }

        CountersUpdated?.Invoke(this, new PtpCountersEventArgs(snapshot));
    }

    private static PtpBuildOptions ToBuildOptions(PtpEngineOptions options)
    {
        return new PtpBuildOptions
        {
            DomainNumber = options.DomainNumber,
            ClockIdentity = ClockIdentity.Parse(options.ClockIdentity),
            Priority1 = options.Priority1,
            Priority2 = options.Priority2,
            ClockClass = options.ClockClass,
            ClockAccuracy = options.ClockAccuracy,
            OffsetScaledLogVariance = options.OffsetScaledLogVariance,
            TimeSource = PtpTimeSource.InternalOscillator,
            TwoStep = options.TwoStep,
            AnnounceLogInterval = MsToLogInterval(options.AnnounceIntervalMs),
            SyncLogInterval = MsToLogInterval(options.SyncIntervalMs)
        };
    }

    private static sbyte MsToLogInterval(int ms)
    {
        var seconds = Math.Max(ms / 1000.0, 0.125);
        return (sbyte)Math.Round(Math.Log(seconds, 2));
    }

    private static string RandomClockIdentity()
    {
        var bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        bytes[0] = 0x02;
        bytes[3] = 0xFF;
        bytes[4] = 0xFE;
        return string.Join("-", bytes.Select(b => b.ToString("X2")));
    }

    private static string ToClockIdentityText(ReadOnlySpan<byte> portIdentity)
    {
        if (portIdentity.Length < 8)
            return "unknown";

        return string.Join("-", portIdentity[..8].ToArray().Select(b => b.ToString("X2")));
    }

    private void SetState(PtpEngineState state)
    {
        State = state;
        StateChanged?.Invoke(this, State);
    }

    private void Log(string severity, string source, string message)
    {
        EventLogged?.Invoke(this, new PtpEngineEventArgs(new PtpEventLogItem
        {
            Timestamp = DateTime.Now,
            Severity = severity,
            Source = source,
            Message = message
        }));
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _transport.PacketReceived -= OnPacketReceived;
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    private enum PtpTxKind
    {
        Announce,
        Sync,
        FollowUp,
        PdelayResp,
        PdelayRespFollowUp
    }

    private sealed record PtpTxItem(byte[] Frame, PtpTxKind Kind, ushort SequenceId, string LogSeverity, string LogSource, string? LogMessage);
}
