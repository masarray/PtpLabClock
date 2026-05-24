// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Protocol.Enums;

public enum PtpClockAccuracy : byte
{
    AccurateWithin1us = 0x20,
    AccurateWithin10us = 0x21,
    AccurateWithin100us = 0x22,
    AccurateWithin1ms = 0x23,
    AccurateWithin10ms = 0x24,
    AccurateWithin100ms = 0x25,
    AccurateWithin1s = 0x26,
    AccurateWithin10s = 0x27,
    Unknown = 0xFE
}
