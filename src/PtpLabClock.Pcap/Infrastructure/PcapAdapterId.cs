// SPDX-License-Identifier: Apache-2.0
using System.Text;

namespace PtpLabClock.Pcap.Infrastructure;

internal static class PcapAdapterId
{
    private const string Prefix = "pcap://";

    public static string FromDeviceName(string deviceName)
    {
        var bytes = Encoding.UTF8.GetBytes(deviceName);
        var token = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return Prefix + token;
    }

    public static string ToDeviceName(string adapterId)
    {
        if (string.IsNullOrWhiteSpace(adapterId))
            throw new ArgumentException("Adapter id is empty.", nameof(adapterId));

        if (!adapterId.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            return adapterId;

        var token = adapterId[Prefix.Length..]
            .Replace('-', '+')
            .Replace('_', '/');

        var padding = token.Length % 4;
        if (padding != 0)
            token = token.PadRight(token.Length + (4 - padding), '=');

        return Encoding.UTF8.GetString(Convert.FromBase64String(token));
    }
}
