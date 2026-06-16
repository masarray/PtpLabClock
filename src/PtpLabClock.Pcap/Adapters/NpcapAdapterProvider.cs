// SPDX-License-Identifier: Apache-2.0
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using PtpLabClock.Core.Abstractions;
using PtpLabClock.Pcap.Infrastructure;
using SharpPcap;

namespace PtpLabClock.Pcap.Adapters;

/// <summary>
/// Enumerates live packet-capture adapters exposed by Npcap/libpcap.
/// </summary>
public sealed class NpcapAdapterProvider : IAdapterProvider
{
    public string LastDiagnostic { get; private set; } = string.Empty;

    public IReadOnlyList<NetworkAdapterInfoDto> GetAdapters()
    {
        LastDiagnostic = string.Empty;

        try
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count == 0)
            {
                LastDiagnostic = "Npcap/libpcap returned zero capture devices. Install Npcap and allow non-admin capture or run as Administrator.";
                return Array.Empty<NetworkAdapterInfoDto>();
            }

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var result = new List<NetworkAdapterInfoDto>();
            var skipped = new List<string>();

            foreach (var device in devices.OfType<ILiveDevice>())
            {
                try
                {
                    var name = device.Name ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        skipped.Add("unnamed capture device");
                        continue;
                    }

                    var matchedInterface = TryFindMatchingInterface(name, networkInterfaces);
                    var isLoopback = LooksLikeLoopback(device, matchedInterface);
                    if (isLoopback)
                    {
                        skipped.Add($"loopback: {GetFriendlyName(device)}");
                        continue;
                    }

                    var friendlyName = matchedInterface?.Name ?? GetFriendlyName(device);
                    var mac = FormatMac(matchedInterface?.GetPhysicalAddress());
                    var isWireless = matchedInterface?.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
                    var isVirtual = LooksVirtual(device, matchedInterface);
                    var isUp = matchedInterface is null || matchedInterface.OperationalStatus == OperationalStatus.Up;
                    var diagnostic = BuildDiagnostic(device, matchedInterface, isWireless, isVirtual, isUp, mac);

                    result.Add(new NetworkAdapterInfoDto
                    {
                        Id = PcapAdapterId.FromDeviceName(name),
                        PcapName = name,
                        Name = friendlyName,
                        Description = BuildDescription(device, matchedInterface, diagnostic),
                        PhysicalAddress = mac,
                        IsDemo = false,
                        IsLoopback = false,
                        IsWireless = isWireless,
                        IsVirtual = isVirtual,
                        IsUp = isUp,
                        OperationalStatus = matchedInterface?.OperationalStatus.ToString() ?? "Unknown",
                        Diagnostics = diagnostic
                    });
                }
                catch (Exception ex)
                {
                    skipped.Add($"device scan error: {ex.Message}");
                }
            }

            LastDiagnostic = BuildSummary(result.Count, skipped);

            return result
                .OrderBy(x => x.IsUp ? 0 : 1)
                .ThenBy(x => x.IsWireless ? 1 : 0)
                .ThenBy(x => x.IsVirtual ? 1 : 0)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (DllNotFoundException ex)
        {
            LastDiagnostic = "Npcap/Packet.dll was not found. Install Npcap, then restart the app. " + ex.Message;
            return Array.Empty<NetworkAdapterInfoDto>();
        }
        catch (Exception ex)
        {
            LastDiagnostic = "Npcap adapter scan failed. Demo Mode remains available. " + ex.Message;
            return Array.Empty<NetworkAdapterInfoDto>();
        }
    }

    private static bool LooksLikeLoopback(ICaptureDevice device, NetworkInterface? networkInterface)
    {
        if (networkInterface?.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            return true;

        var text = $"{device.Name} {device.Description} {networkInterface?.Name} {networkInterface?.Description}";
        return text.Contains("loopback", StringComparison.OrdinalIgnoreCase)
            || text.Contains("NPF_Loopback", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksVirtual(ICaptureDevice device, NetworkInterface? networkInterface)
    {
        if (networkInterface?.NetworkInterfaceType is NetworkInterfaceType.Tunnel or NetworkInterfaceType.Ppp)
            return true;

        var text = $"{device.Name} {device.Description} {networkInterface?.Name} {networkInterface?.Description}";
        string[] markers =
        [
            "virtual", "hyper-v", "vmware", "virtualbox", "vpn", "tap", "tunnel", "docker", "npcap loopback",
            "bluetooth", "zerotier", "tailscale", "wireguard", "wintun"
        ];

        return markers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetFriendlyName(ICaptureDevice device)
    {
        if (!string.IsNullOrWhiteSpace(device.Description))
            return device.Description!;

        return device.Name ?? "Npcap adapter";
    }

    private static string BuildDescription(ICaptureDevice device, NetworkInterface? networkInterface, string diagnostic)
    {
        var friendly = networkInterface?.Name ?? GetFriendlyName(device);
        var detail = networkInterface?.Description ?? device.Description ?? device.Name ?? "Npcap adapter";
        return $"{friendly} - {detail} [{diagnostic}]";
    }

    private static string BuildDiagnostic(ICaptureDevice device, NetworkInterface? networkInterface, bool isWireless, bool isVirtual, bool isUp, string mac)
    {
        var tags = new List<string>();

        if (!isUp) tags.Add("DOWN");
        if (isWireless) tags.Add("Wi-Fi");
        if (isVirtual) tags.Add("Virtual/VPN");
        if (!string.IsNullOrWhiteSpace(mac)) tags.Add(mac); else tags.Add("No MAC");
        if (networkInterface is not null) tags.Add(networkInterface.NetworkInterfaceType.ToString());
        else tags.Add("Pcap-only");

        return string.Join(" • ", tags);
    }

    private static string BuildSummary(int usableCount, IReadOnlyList<string> skipped)
    {
        var baseText = usableCount == 0
            ? "No usable RAW adapter candidate was detected. Demo Mode is still available."
            : $"{usableCount} RAW adapter candidate(s) detected. Prefer a wired Ethernet adapter for process-bus lab traffic.";

        if (skipped.Count == 0)
            return baseText;

        return baseText + " Skipped: " + string.Join("; ", skipped.Take(4));
    }

    private static string FormatMac(PhysicalAddress? address)
    {
        if (address is null)
            return string.Empty;

        var bytes = address.GetAddressBytes();
        if (bytes.Length != 6 || bytes.All(b => b == 0))
            return string.Empty;

        return string.Join("-", bytes.Select(b => b.ToString("X2")));
    }

    private static NetworkInterface? TryFindMatchingInterface(string deviceName, IEnumerable<NetworkInterface> interfaces)
    {
        var guid = TryExtractGuid(deviceName);
        if (guid is null)
            return null;

        return interfaces.FirstOrDefault(ni => string.Equals(NormalizeGuid(ni.Id), guid, StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryExtractGuid(string value)
    {
        var match = Regex.Match(value, "\\{(?<guid>[0-9A-Fa-f-]{36})\\}");
        return match.Success ? NormalizeGuid(match.Groups["guid"].Value) : null;
    }

    private static string NormalizeGuid(string value) => value.Trim('{', '}').ToUpperInvariant();
}
