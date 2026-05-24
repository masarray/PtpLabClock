// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Protocol.Ethernet;

public static class PtpMulticastAddresses
{
    public static readonly MacAddress General = MacAddress.Parse("01-1B-19-00-00-00");
    public static readonly MacAddress PeerDelay = MacAddress.Parse("01-80-C2-00-00-0E");
}
