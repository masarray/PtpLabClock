// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Protocol.Serialization;

namespace PtpLabClock.Protocol.Messages;

public readonly struct PtpTimestamp
{
    public ulong Seconds { get; }
    public uint Nanoseconds { get; }

    public PtpTimestamp(ulong seconds, uint nanoseconds)
    {
        Seconds = seconds;
        Nanoseconds = nanoseconds;
    }

    public static PtpTimestamp Now()
    {
        var now = DateTimeOffset.UtcNow;
        var seconds = (ulong)now.ToUnixTimeSeconds();
        var nanos = (uint)((now.Ticks % TimeSpan.TicksPerSecond) * 100);
        return new PtpTimestamp(seconds, nanos);
    }

    public void WriteTo(BigEndianWriter writer)
    {
        // IEEE 1588 timestamp: 48-bit seconds + 32-bit nanoseconds.
        writer.WriteUInt16((ushort)((Seconds >> 32) & 0xFFFF));
        writer.WriteUInt32((uint)(Seconds & 0xFFFFFFFF));
        writer.WriteUInt32(Nanoseconds);
    }
}
