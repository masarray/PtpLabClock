// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Diagnostics;

public sealed class PtpRuntimeCounters
{
    public long AnnounceTx { get; set; }
    public long SyncTx { get; set; }
    public long FollowUpTx { get; set; }
    public long PdelayReqRx { get; set; }
    public long PdelayRespTx { get; set; }
    public long PdelayRespFollowUpTx { get; set; }
    public long PacketErrors { get; set; }

    public ushort LastAnnounceSeq { get; set; }
    public ushort LastSyncSeq { get; set; }
    public ushort LastPdelayReqSeq { get; set; }
    public string LastPeerClockIdentity { get; set; } = string.Empty;
    public string LastTxSummary { get; set; } = string.Empty;
    public string LastTxError { get; set; } = string.Empty;

    public PtpRuntimeCounters Clone() => (PtpRuntimeCounters)MemberwiseClone();
}
