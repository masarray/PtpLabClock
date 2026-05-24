// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Engine;

public sealed class PtpEngineOptions
{
    public string AdapterId { get; set; } = string.Empty;
    public string AdapterName { get; set; } = string.Empty;
    public string SourceMac { get; set; } = "02-00-00-00-00-01";
    public string ClockIdentity { get; set; } = "02-00-00-FF-FE-00-00-01";
    public PtpProfilePreset ProfilePreset { get; set; } = PtpProfilePreset.Iec61850_9_3_Lab;

    public byte DomainNumber { get; set; } = 0;
    public byte ClockClass { get; set; } = 248;
    public PtpClockAccuracy ClockAccuracy { get; set; } = PtpClockAccuracy.Unknown;
    public ushort OffsetScaledLogVariance { get; set; } = 0xFFFF;
    public byte Priority1 { get; set; } = 128;
    public byte Priority2 { get; set; } = 128;

    public bool EnableAnnounce { get; set; } = true;
    public bool EnableSync { get; set; } = true;
    public bool EnableFollowUp { get; set; } = true;
    public bool EnablePdelayResponder { get; set; } = true;
    public bool TwoStep { get; set; } = true;

    public int AnnounceIntervalMs { get; set; } = 1000;
    public int SyncIntervalMs { get; set; } = 1000;
}
