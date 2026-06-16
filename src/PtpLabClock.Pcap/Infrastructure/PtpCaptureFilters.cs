// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Pcap.Infrastructure;

internal static class PtpCaptureFilters
{
    /// <summary>
    /// Captures Layer-2 PTP EtherType for untagged, single VLAN and common double-tagged QinQ frames.
    /// The repeated vlan keywords are intentional: libpcap's vlan primitive adjusts the decoding offset.
    /// </summary>
    public const string Layer2PtpWithVlan = "ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)";
}
