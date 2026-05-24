// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Monitor;

public sealed class PtpSourceClockState
{
    public string ClockIdentity { get; init; } = string.Empty;
    public byte Domain { get; set; }
    public DateTime FirstSeen { get; set; } = DateTime.Now;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public PtpMessageType LastMessageType { get; set; }
    public ushort LastSequenceId { get; set; }
    public long AnnounceCount { get; set; }
    public long SyncCount { get; set; }
    public long FollowUpCount { get; set; }
    public long PdelayReqCount { get; set; }
    public long PdelayRespCount { get; set; }
    public long PdelayRespFollowUpCount { get; set; }
    public long OtherCount { get; set; }
    public long SequenceAnomalyCount { get; set; }

    public bool IsLive(DateTime now, TimeSpan timeout) => now - LastSeen <= timeout;

    public long TotalCount => AnnounceCount + SyncCount + FollowUpCount + PdelayReqCount + PdelayRespCount + PdelayRespFollowUpCount + OtherCount;

    public PtpSourceClockState Clone() => (PtpSourceClockState)MemberwiseClone();
}
