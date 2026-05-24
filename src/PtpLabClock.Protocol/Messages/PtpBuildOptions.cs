// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Protocol.Messages;

public sealed class PtpBuildOptions
{
    public byte DomainNumber { get; init; } = 0;
    public ClockIdentity ClockIdentity { get; init; } = ClockIdentity.Parse("02-00-00-FF-FE-00-00-01");
    public ushort PortNumber { get; init; } = 1;
    public byte Priority1 { get; init; } = 128;
    public byte Priority2 { get; init; } = 128;
    public byte ClockClass { get; init; } = 248;
    public PtpClockAccuracy ClockAccuracy { get; init; } = PtpClockAccuracy.Unknown;
    public ushort OffsetScaledLogVariance { get; init; } = 0xFFFF;
    public PtpTimeSource TimeSource { get; init; } = PtpTimeSource.InternalOscillator;
    public bool TwoStep { get; init; } = true;
    public sbyte AnnounceLogInterval { get; init; } = 0;
    public sbyte SyncLogInterval { get; init; } = 0;
}
