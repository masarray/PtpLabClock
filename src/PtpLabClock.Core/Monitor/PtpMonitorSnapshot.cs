// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Monitor;

public sealed class PtpMonitorSnapshot
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public long TotalFrames { get; init; }
    public long InvalidFrames { get; init; }
    public IReadOnlyDictionary<PtpMessageType, long> MessageCounters { get; init; } = new Dictionary<PtpMessageType, long>();
    public IReadOnlyDictionary<byte, long> DomainCounters { get; init; } = new Dictionary<byte, long>();
    public IReadOnlyList<PtpSourceClockState> Sources { get; init; } = Array.Empty<PtpSourceClockState>();
    public PtpObservedMessage? LastMessage { get; init; }
    public string LastInvalidReason { get; init; } = string.Empty;

    public long ValidFrames => Math.Max(0, TotalFrames - InvalidFrames);
    public int LiveSourceCount => Sources.Count(s => s.IsLive(Timestamp, TimeSpan.FromSeconds(5)));
    public int DetectedDomainCount => DomainCounters.Count;
    public bool MultipleAnnounceSources => Sources.Count(s => s.AnnounceCount > 0 && s.IsLive(Timestamp, TimeSpan.FromSeconds(5))) > 1;

    public string Summary => $"frames={TotalFrames} valid={ValidFrames} invalid={InvalidFrames} domains={DetectedDomainCount} liveSources={LiveSourceCount} last={LastMessage?.Summary ?? "none"}";
}
