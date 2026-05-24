// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Core.Engine;
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Monitor;

public sealed class PtpPassiveMonitor
{
    private readonly object _gate = new();
    private readonly Dictionary<PtpMessageType, long> _messageCounters = new();
    private readonly Dictionary<byte, long> _domainCounters = new();
    private readonly Dictionary<string, PtpSourceClockState> _sources = new(StringComparer.OrdinalIgnoreCase);
    private PtpObservedMessage? _lastMessage;
    private string _lastInvalidReason = string.Empty;
    private long _totalFrames;
    private long _invalidFrames;

    public event EventHandler<PtpObservedMessage>? MessageObserved;
    public event EventHandler<PtpMonitorSnapshot>? SnapshotUpdated;

    public PtpMonitorSnapshot ObserveFrame(byte[] frame, string direction = "RX", byte? expectedDomain = null)
    {
        var now = DateTime.Now;
        var inspection = PtpFrameInspector.Inspect(frame, expectedDomain);
        PtpObservedMessage? observed = null;
        PtpMonitorSnapshot snapshot;

        lock (_gate)
        {
            _totalFrames++;

            if (!inspection.IsValid)
            {
                _invalidFrames++;
                _lastInvalidReason = inspection.Error;
                snapshot = CreateSnapshotLocked(now);
            }
            else
            {
                observed = new PtpObservedMessage
                {
                    Timestamp = now,
                    Direction = direction,
                    MessageType = inspection.MessageType,
                    Domain = inspection.Domain,
                    SequenceId = inspection.SequenceId,
                    SourceClockIdentity = inspection.SourceClockIdentity,
                    Transport = inspection.Transport,
                    FrameLength = inspection.FrameLength,
                    MessageLength = inspection.MessageLength
                };

                _lastMessage = observed;
                Increment(_messageCounters, inspection.MessageType);
                Increment(_domainCounters, inspection.Domain);
                UpdateSourceLocked(observed);
                snapshot = CreateSnapshotLocked(now);
            }
        }

        if (observed is not null)
            MessageObserved?.Invoke(this, observed);

        SnapshotUpdated?.Invoke(this, snapshot);
        return snapshot;
    }

    public PtpMonitorSnapshot GetSnapshot()
    {
        lock (_gate)
            return CreateSnapshotLocked(DateTime.Now);
    }

    public void Reset()
    {
        lock (_gate)
        {
            _messageCounters.Clear();
            _domainCounters.Clear();
            _sources.Clear();
            _lastMessage = null;
            _lastInvalidReason = string.Empty;
            _totalFrames = 0;
            _invalidFrames = 0;
        }

        SnapshotUpdated?.Invoke(this, GetSnapshot());
    }

    private void UpdateSourceLocked(PtpObservedMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.SourceClockIdentity))
            return;

        var key = $"{message.Domain}:{message.SourceClockIdentity}";
        if (!_sources.TryGetValue(key, out var source))
        {
            source = new PtpSourceClockState
            {
                ClockIdentity = message.SourceClockIdentity,
                Domain = message.Domain,
                FirstSeen = message.Timestamp
            };
            _sources[key] = source;
        }

        source.LastSeen = message.Timestamp;
        source.LastMessageType = message.MessageType;
        source.LastSequenceId = message.SequenceId;

        switch (message.MessageType)
        {
            case PtpMessageType.Announce:
                source.AnnounceCount++;
                break;
            case PtpMessageType.Sync:
                source.SyncCount++;
                break;
            case PtpMessageType.FollowUp:
                source.FollowUpCount++;
                break;
            case PtpMessageType.PdelayReq:
                source.PdelayReqCount++;
                break;
            case PtpMessageType.PdelayResp:
                source.PdelayRespCount++;
                break;
            case PtpMessageType.PdelayRespFollowUp:
                source.PdelayRespFollowUpCount++;
                break;
            default:
                source.OtherCount++;
                break;
        }
    }

    private PtpMonitorSnapshot CreateSnapshotLocked(DateTime now)
    {
        return new PtpMonitorSnapshot
        {
            Timestamp = now,
            TotalFrames = _totalFrames,
            InvalidFrames = _invalidFrames,
            MessageCounters = new Dictionary<PtpMessageType, long>(_messageCounters),
            DomainCounters = new Dictionary<byte, long>(_domainCounters),
            Sources = _sources.Values.Select(s => s.Clone()).OrderByDescending(s => s.LastSeen).ToArray(),
            LastMessage = _lastMessage,
            LastInvalidReason = _lastInvalidReason
        };
    }

    private static void Increment<TKey>(Dictionary<TKey, long> map, TKey key) where TKey : notnull
    {
        map.TryGetValue(key, out var current);
        map[key] = current + 1;
    }
}
