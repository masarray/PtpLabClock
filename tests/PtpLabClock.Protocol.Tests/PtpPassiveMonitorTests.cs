// SPDX-License-Identifier: Apache-2.0
using Xunit;
using PtpLabClock.Core.Monitor;
using PtpLabClock.Protocol;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Ethernet;
using PtpLabClock.Protocol.Messages;
using PtpLabClock.Protocol.Serialization;

namespace PtpLabClock.Protocol.Tests;

public sealed class PtpPassiveMonitorTests
{
    [Fact]
    public void ObserveFrame_GroupsSourcesByDomainAndClockIdentity()
    {
        var serializer = new PtpMessageSerializer();
        var options = new PtpBuildOptions
        {
            DomainNumber = 0,
            ClockIdentity = ClockIdentity.Parse("02-00-00-FF-FE-00-00-01"),
            TwoStep = true
        };
        var srcMac = MacAddress.Parse("02-00-00-00-00-01");
        var monitor = new PtpPassiveMonitor();

        var announce = EthernetFrameBuilder.Build(PtpMulticastAddresses.General, srcMac, EtherTypes.Ptp, serializer.BuildAnnounce(options, 1));
        var sync = EthernetFrameBuilder.Build(PtpMulticastAddresses.General, srcMac, EtherTypes.Ptp, serializer.BuildSync(options, 2));

        monitor.ObserveFrame(announce);
        var snapshot = monitor.ObserveFrame(sync);

        Assert.Equal(2, snapshot.TotalFrames);
        Assert.Equal(2, snapshot.ValidFrames);
        Assert.Single(snapshot.Sources);
        Assert.Equal(1, snapshot.Sources[0].AnnounceCount);
        Assert.Equal(1, snapshot.Sources[0].SyncCount);
        Assert.Equal(0, snapshot.Sources[0].SequenceAnomalyCount);
    }
}
