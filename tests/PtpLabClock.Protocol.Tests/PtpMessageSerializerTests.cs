// SPDX-License-Identifier: Apache-2.0
using Xunit;
using PtpLabClock.Core.Engine;
using PtpLabClock.Protocol;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Ethernet;
using PtpLabClock.Protocol.Messages;
using PtpLabClock.Protocol.Serialization;

namespace PtpLabClock.Protocol.Tests;

public sealed class PtpMessageSerializerTests
{
    private static readonly PtpBuildOptions Options = new()
    {
        DomainNumber = 7,
        ClockIdentity = ClockIdentity.Parse("02-00-00-FF-FE-00-00-01"),
        PortNumber = 1,
        ClockClass = 248,
        ClockAccuracy = PtpClockAccuracy.Unknown,
        TwoStep = true
    };

    [Fact]
    public void BuildAnnounce_WritesExpectedCommonHeaderAndBodyOffsets()
    {
        var serializer = new PtpMessageSerializer();
        var message = serializer.BuildAnnounce(Options, 0x1234);

        Assert.Equal(64, message.Length);
        Assert.Equal((byte)PtpMessageType.Announce, (byte)(message[0] & 0x0F));
        Assert.Equal(2, message[1] & 0x0F);
        Assert.Equal(64, ReadUInt16(message, 2));
        Assert.Equal(7, message[4]);
        Assert.Equal(0x1234, ReadUInt16(message, 30));
        Assert.Equal(5, message[32]);
        Assert.Equal(128, message[47]);
        Assert.Equal(248, message[48]);
        Assert.Equal((byte)PtpClockAccuracy.Unknown, message[49]);
        Assert.Equal(new byte[] { 0x02, 0x00, 0x00, 0xFF, 0xFE, 0x00, 0x00, 0x01 }, message.Skip(53).Take(8).ToArray());
    }

    [Fact]
    public void BuildSyncAndFollowUp_KeepMatchingSequenceAndTwoStepFlagOnlyOnSync()
    {
        var serializer = new PtpMessageSerializer();
        var timestamp = new PtpTimestamp(123, 456);

        var sync = serializer.BuildSync(Options, 0x0042, timestamp);
        var followUp = serializer.BuildFollowUp(Options, 0x0042, timestamp);

        Assert.Equal(44, sync.Length);
        Assert.Equal(44, followUp.Length);
        Assert.Equal((byte)PtpMessageType.Sync, (byte)(sync[0] & 0x0F));
        Assert.Equal((byte)PtpMessageType.FollowUp, (byte)(followUp[0] & 0x0F));
        Assert.Equal(0x0042, ReadUInt16(sync, 30));
        Assert.Equal(0x0042, ReadUInt16(followUp, 30));
        Assert.Equal(0x0200, ReadUInt16(sync, 6));
        Assert.Equal(0, ReadUInt16(followUp, 6));
        Assert.Equal(sync.Skip(34).Take(10).ToArray(), followUp.Skip(34).Take(10).ToArray());
    }

    [Fact]
    public void BuildPdelayResponse_CopiesRequestingPortIdentity()
    {
        var serializer = new PtpMessageSerializer();
        var requester = new byte[] { 0x02, 0x00, 0x00, 0xFF, 0xFE, 0xAA, 0x10, 0x01, 0x00, 0x01 };
        var timestamp = new PtpTimestamp(789, 1000);

        var response = serializer.BuildPdelayResp(Options, 0x0102, requester, timestamp);
        var followUp = serializer.BuildPdelayRespFollowUp(Options, 0x0102, requester, timestamp);

        Assert.Equal(54, response.Length);
        Assert.Equal(54, followUp.Length);
        Assert.Equal((byte)PtpMessageType.PdelayResp, (byte)(response[0] & 0x0F));
        Assert.Equal((byte)PtpMessageType.PdelayRespFollowUp, (byte)(followUp[0] & 0x0F));
        Assert.Equal(0x0102, ReadUInt16(response, 30));
        Assert.Equal(0x0102, ReadUInt16(followUp, 30));
        Assert.Equal(requester, response.Skip(44).Take(10).ToArray());
        Assert.Equal(requester, followUp.Skip(44).Take(10).ToArray());
    }

    [Fact]
    public void EthernetFrameInspector_RecognizesLayer2PtpFrame()
    {
        var serializer = new PtpMessageSerializer();
        var sourceMac = MacAddress.Parse("02-00-00-00-00-01");
        var frame = EthernetFrameBuilder.Build(PtpMulticastAddresses.General, sourceMac, EtherTypes.Ptp, serializer.BuildAnnounce(Options, 1));

        var result = PtpFrameInspector.Inspect(frame, expectedDomain: 7);

        Assert.True(result.IsValid, result.Error);
        Assert.Equal(14, result.PtpOffset);
        Assert.Equal(PtpMessageType.Announce, result.MessageType);
        Assert.Equal(7, result.Domain);
        Assert.Equal(1, result.SequenceId);
        Assert.Equal("02-00-00-FF-FE-00-00-01", result.SourceClockIdentity);
    }


    [Fact]
    public void BuildVlan_WritesExpectedTagAndInspectorFindsPtpAtOffset18()
    {
        var serializer = new PtpMessageSerializer();
        var sourceMac = MacAddress.Parse("02-00-00-00-00-01");
        var frame = EthernetFrameBuilder.BuildVlan(
            PtpMulticastAddresses.General,
            sourceMac,
            vlanId: 100,
            priorityCodePoint: 4,
            EtherTypes.Ptp,
            serializer.BuildAnnounce(Options, 2));

        Assert.Equal(EtherTypes.Vlan, ReadUInt16(frame, 12));
        Assert.Equal((ushort)((4 << 13) | 100), ReadUInt16(frame, 14));
        Assert.Equal(EtherTypes.Ptp, ReadUInt16(frame, 16));

        var result = PtpFrameInspector.Inspect(frame, expectedDomain: 7);
        Assert.True(result.IsValid, result.Error);
        Assert.Equal(18, result.PtpOffset);
        Assert.Equal("Layer-2 VLAN", result.Transport);
    }

    [Fact]
    public void BuildQinQ_WritesExpectedTagsAndInspectorFindsPtpAtOffset22()
    {
        var serializer = new PtpMessageSerializer();
        var sourceMac = MacAddress.Parse("02-00-00-00-00-01");
        var frame = EthernetFrameBuilder.BuildQinQ(
            PtpMulticastAddresses.General,
            sourceMac,
            serviceVlanId: 20,
            servicePriorityCodePoint: 4,
            customerVlanId: 100,
            customerPriorityCodePoint: 4,
            EtherTypes.Ptp,
            serializer.BuildSync(Options, 3));

        Assert.Equal(EtherTypes.ProviderBridge, ReadUInt16(frame, 12));
        Assert.Equal((ushort)((4 << 13) | 20), ReadUInt16(frame, 14));
        Assert.Equal(EtherTypes.Vlan, ReadUInt16(frame, 16));
        Assert.Equal((ushort)((4 << 13) | 100), ReadUInt16(frame, 18));
        Assert.Equal(EtherTypes.Ptp, ReadUInt16(frame, 20));

        var result = PtpFrameInspector.Inspect(frame, expectedDomain: 7);
        Assert.True(result.IsValid, result.Error);
        Assert.Equal(22, result.PtpOffset);
        Assert.Equal("Layer-2 QinQ", result.Transport);
    }

    [Fact]
    public void BuildVlan_RejectsOutOfRangeVlanFields()
    {
        var payload = new byte[44];
        var sourceMac = MacAddress.Parse("02-00-00-00-00-01");

        Assert.Throws<ArgumentOutOfRangeException>(() => EthernetFrameBuilder.BuildVlan(PtpMulticastAddresses.General, sourceMac, 4095, 0, EtherTypes.Ptp, payload));
        Assert.Throws<ArgumentOutOfRangeException>(() => EthernetFrameBuilder.BuildVlan(PtpMulticastAddresses.General, sourceMac, 1, 8, EtherTypes.Ptp, payload));
    }

    [Fact]
    public void BuildPdelayReq_ProducesReadableCommonHeader()
    {
        var serializer = new PtpMessageSerializer();
        var pdelayReq = serializer.BuildPdelayReq(Options, 0x3333, new PtpTimestamp(10, 20));

        Assert.Equal(54, pdelayReq.Length);
        Assert.Equal((byte)PtpMessageType.PdelayReq, (byte)(pdelayReq[0] & 0x0F));
        Assert.Equal(0x3333, ReadUInt16(pdelayReq, 30));
    }

    private static ushort ReadUInt16(byte[] buffer, int offset) => (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
}
