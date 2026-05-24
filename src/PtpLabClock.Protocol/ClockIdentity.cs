// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;

namespace PtpLabClock.Protocol;

public sealed class ClockIdentity
{
    public byte[] Bytes { get; }

    public ClockIdentity(byte[] bytes)
    {
        if (bytes.Length != 8)
            throw new ArgumentException("Clock identity must be 8 bytes.", nameof(bytes));
        Bytes = bytes.ToArray();
    }

    public static ClockIdentity Parse(string text)
    {
        var parts = text.Replace('-', ':').Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 8)
            return new ClockIdentity(parts.Select(p => byte.Parse(p, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToArray());

        if (parts.Length == 6)
        {
            var mac = parts.Select(p => byte.Parse(p, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToArray();
            return FromMac(mac);
        }

        throw new FormatException("Clock identity must be 8 bytes, or 6-byte MAC that can be converted to EUI-64 style identity.");
    }

    public static ClockIdentity FromMac(byte[] mac)
    {
        if (mac.Length != 6) throw new ArgumentException("MAC must be 6 bytes.", nameof(mac));
        return new ClockIdentity(new[] { mac[0], mac[1], mac[2], (byte)0xFF, (byte)0xFE, mac[3], mac[4], mac[5] });
    }

    public override string ToString() => string.Join("-", Bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
}
