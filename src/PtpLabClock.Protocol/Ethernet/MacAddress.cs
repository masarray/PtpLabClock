// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;

namespace PtpLabClock.Protocol.Ethernet;

public readonly struct MacAddress
{
    public byte[] Bytes { get; }

    public MacAddress(byte[] bytes)
    {
        if (bytes.Length != 6)
            throw new ArgumentException("MAC address must be 6 bytes.", nameof(bytes));
        Bytes = bytes.ToArray();
    }

    public static MacAddress Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("MAC address cannot be empty.", nameof(text));

        var parts = text.Replace('-', ':').Split(':');
        if (parts.Length != 6)
            throw new FormatException($"Invalid MAC address: {text}");

        return new MacAddress(parts.Select(p => byte.Parse(p, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToArray());
    }

    public override string ToString() => string.Join("-", Bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
}
