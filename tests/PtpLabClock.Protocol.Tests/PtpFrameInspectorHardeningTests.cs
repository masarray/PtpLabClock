// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Core.Engine;
using PtpLabClock.Core.Scheduling;
using PtpLabClock.Protocol;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Ethernet;
using PtpLabClock.Protocol.Messages;
using PtpLabClock.Protocol.Serialization;
using Xunit;

namespace PtpLabClock.Protocol.Tests;

public sealed class PtpFrameInspectorHardeningTests
{
    private static readonly PtpBuildOptions Options = new()
    {
        DomainNumber = 0,
        ClockIdentity = ClockIdentity.Parse("02-00-00-FF-FE-00-00-01"),
        ClockClass = 248,
        ClockAccuracy = PtpClockAccuracy.Unknown,
        TwoStep = true
    };

    [Fact]
    public void Inspect_RejectsDomainMismatch()
    {
        var frame = BuildAnnounce(domain: 7);

        var result = PtpFrameInspector.Inspect(frame, expectedDomain: 0);

        Assert.False(result.IsValid);
        Assert.Contains("Domain mismatch", result.Error);
    }

    [Fact]
    public void Inspect_RejectsTruncatedMessageLength()
    {
        var frame = BuildAnnounce(domain: 0);
        Array.Resize(ref frame, frame.Length - 8);

        var result = PtpFrameInspector.Inspect(frame, expectedDomain: 0);

        Assert.False(result.IsValid);
        Assert.Contains("truncated", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Inspect_RejectsInvalidPtpMessageLength()
    {
        var frame = BuildAnnounce(domain: 0);
        frame[16] = 0x00;
        frame[17] = 0x20;

        var result = PtpFrameInspector.Inspect(frame, expectedDomain: 0);

        Assert.False(result.IsValid);
        Assert.Contains("too short", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Inspect_RejectsMalformedVlanHeader()
    {
        var frame = new byte[16];
        frame[12] = 0x81;
        frame[13] = 0x00;

        var result = PtpFrameInspector.Inspect(frame, expectedDomain: 0);

        Assert.False(result.IsValid);
        Assert.Contains("VLAN header truncated", result.Error);
    }

    [Fact]
    public void SequenceIdManager_SkipsZeroAfterWraparound()
    {
        var manager = new SequenceIdManager();
        manager.Jump(PtpMessageType.Sync, 0xFFFE);

        Assert.Equal(0xFFFF, manager.Next(PtpMessageType.Sync));
        Assert.Equal(1, manager.Next(PtpMessageType.Sync));
    }

    private static byte[] BuildAnnounce(byte domain)
    {
        var serializer = new PtpMessageSerializer();
        var options = new PtpBuildOptions
        {
            DomainNumber = domain,
            ClockIdentity = Options.ClockIdentity,
            ClockClass = Options.ClockClass,
            ClockAccuracy = Options.ClockAccuracy,
            TwoStep = Options.TwoStep
        };
        return EthernetFrameBuilder.Build(
            PtpMulticastAddresses.General,
            MacAddress.Parse("02-00-00-00-00-01"),
            EtherTypes.Ptp,
            serializer.BuildAnnounce(options, 1));
    }
}
