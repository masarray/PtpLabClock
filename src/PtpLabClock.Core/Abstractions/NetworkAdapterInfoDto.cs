// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Abstractions;

public sealed class NetworkAdapterInfoDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsDemo { get; init; }

    /// <summary>Original libpcap/Npcap device name, useful for diagnostics.</summary>
    public string PcapName { get; init; } = string.Empty;

    /// <summary>Adapter MAC address formatted as AA-BB-CC-DD-EE-FF when available.</summary>
    public string PhysicalAddress { get; init; } = string.Empty;

    public bool IsLoopback { get; init; }
    public bool IsWireless { get; init; }
    public bool IsVirtual { get; init; }
    public bool IsUp { get; init; } = true;
    public string OperationalStatus { get; init; } = string.Empty;
    public string Diagnostics { get; init; } = string.Empty;

    public string ModeLabel => IsDemo ? "DEMO" : IsWireless ? "RAW/WI-FI" : IsVirtual ? "RAW/VIRTUAL" : "RAW";

    public string SuitabilityLabel
    {
        get
        {
            if (IsDemo) return "Safe demo";
            if (!IsUp) return "Adapter down";
            if (IsWireless) return "Wi-Fi may block injection";
            if (IsVirtual) return "Virtual adapter";
            if (string.IsNullOrWhiteSpace(PhysicalAddress)) return "No MAC detected";
            return "Wired candidate";
        }
    }

    public override string ToString() => string.IsNullOrWhiteSpace(Description) ? Name : Description;
}
