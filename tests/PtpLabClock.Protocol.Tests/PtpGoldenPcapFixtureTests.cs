// SPDX-License-Identifier: Apache-2.0
using System.Buffers.Binary;
using PtpLabClock.Core.Engine;
using PtpLabClock.Protocol;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Ethernet;
using PtpLabClock.Protocol.Messages;
using PtpLabClock.Protocol.Serialization;
using Xunit;

namespace PtpLabClock.Protocol.Tests;

public sealed class PtpGoldenPcapFixtureTests
{
    private static readonly MacAddress SourceMac = MacAddress.Parse("02-00-00-00-00-01");

    private static readonly PtpBuildOptions Options = new()
    {
        DomainNumber = 0,
        ClockIdentity = ClockIdentity.Parse("02-00-00-FF-FE-00-00-01"),
        PortNumber = 1,
        ClockClass = 248,
        ClockAccuracy = PtpClockAccuracy.Unknown,
        TwoStep = true
    };

    [Fact]
    public void GoldenPcapFixtures_ArePresentAndValidateExpectedFrames()
    {
        var expectations = new[]
        {
            new FixtureExpectation(
                "ptp-announce-untagged.pcap",
                new[] { PtpMessageType.Announce },
                "Layer-2"),
            new FixtureExpectation(
                "ptp-sync-followup-vlan.pcap",
                new[] { PtpMessageType.Sync, PtpMessageType.FollowUp },
                "Layer-2 VLAN"),
            new FixtureExpectation(
                "ptp-pdelay-qinq.pcap",
                new[] { PtpMessageType.PdelayReq },
                "Layer-2 QinQ"),
            new FixtureExpectation(
                "ptp-mixed-process-bus-golden.pcap",
                new[] { PtpMessageType.Announce, PtpMessageType.Sync, PtpMessageType.FollowUp, PtpMessageType.PdelayReq },
                null)
        };

        foreach (var expectation in expectations)
        {
            var frames = ReadPcapFixture(expectation.FileName);

            Assert.Equal(expectation.MessageTypes.Length, frames.Count);

            for (var i = 0; i < frames.Count; i++)
            {
                var result = PtpFrameInspector.Inspect(frames[i], expectedDomain: 0);

                Assert.True(result.IsValid, $"{expectation.FileName} frame {i}: {result.Error}");
                Assert.Equal(expectation.MessageTypes[i], result.MessageType);
                Assert.Equal(2, result.Version);
                Assert.Equal(0, result.Domain);

                if (expectation.ExpectedTransport is not null)
                    Assert.Equal(expectation.ExpectedTransport, result.Transport);
            }
        }
    }

    [Fact]
    public void GoldenMixedFixture_ContainsUntaggedVlanAndQinQTransports()
    {
        var frames = ReadPcapFixture("ptp-mixed-process-bus-golden.pcap");

        var transports = frames
            .Select(frame => PtpFrameInspector.Inspect(frame, expectedDomain: 0).Transport)
            .ToArray();

        Assert.Contains("Layer-2", transports);
        Assert.Contains("Layer-2 VLAN", transports);
        Assert.Contains("Layer-2 QinQ", transports);
    }

    private static IReadOnlyList<byte[]> ReadPcapFixture(string fileName)
    {
        var bytes = BuildPcapFixture(fileName);
        Assert.True(bytes.Length >= 24, $"PCAP fixture is too short: {fileName}");

        var magic = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(0, 4));
        Assert.Equal(0xA1B2C3D4u, magic);

        var frames = new List<byte[]>();
        var offset = 24;

        while (offset < bytes.Length)
        {
            Assert.True(offset + 16 <= bytes.Length, $"Truncated PCAP record header in {fileName}.");

            var includedLength = (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 8, 4));
            var originalLength = (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 12, 4));
            offset += 16;

            Assert.True(includedLength > 0, $"Invalid included length in {fileName}.");
            Assert.Equal(originalLength, includedLength);
            Assert.True(offset + includedLength <= bytes.Length, $"Truncated PCAP frame payload in {fileName}.");

            frames.Add(bytes.AsSpan(offset, includedLength).ToArray());
            offset += includedLength;
        }

        return frames;
    }

    private static byte[] BuildPcapFixture(string fileName)
    {
        var serializer = new PtpMessageSerializer();

        var frames = fileName switch
        {
            "ptp-announce-untagged.pcap" => new[]
            {
                EthernetFrameBuilder.Build(
                    PtpMulticastAddresses.General,
                    SourceMac,
                    EtherTypes.Ptp,
                    serializer.BuildAnnounce(Options, 1))
            },
            "ptp-sync-followup-vlan.pcap" => new[]
            {
                EthernetFrameBuilder.BuildVlan(
                    PtpMulticastAddresses.General,
                    SourceMac,
                    vlanId: 100,
                    priorityCodePoint: 4,
                    etherType: EtherTypes.Ptp,
                    payload: serializer.BuildSync(Options, 2, new PtpTimestamp(10, 20))),
                EthernetFrameBuilder.BuildVlan(
                    PtpMulticastAddresses.General,
                    SourceMac,
                    vlanId: 100,
                    priorityCodePoint: 4,
                    etherType: EtherTypes.Ptp,
                    payload: serializer.BuildFollowUp(Options, 2, new PtpTimestamp(10, 20)))
            },
            "ptp-pdelay-qinq.pcap" => new[]
            {
                EthernetFrameBuilder.BuildQinQ(
                    PtpMulticastAddresses.PeerDelay,
                    SourceMac,
                    serviceVlanId: 200,
                    servicePriorityCodePoint: 4,
                    customerVlanId: 100,
                    customerPriorityCodePoint: 4,
                    etherType: EtherTypes.Ptp,
                    payload: serializer.BuildPdelayReq(Options, 3, new PtpTimestamp(11, 22)))
            },
            "ptp-mixed-process-bus-golden.pcap" => new[]
            {
                EthernetFrameBuilder.Build(
                    PtpMulticastAddresses.General,
                    SourceMac,
                    EtherTypes.Ptp,
                    serializer.BuildAnnounce(Options, 1)),
                EthernetFrameBuilder.BuildVlan(
                    PtpMulticastAddresses.General,
                    SourceMac,
                    vlanId: 100,
                    priorityCodePoint: 4,
                    etherType: EtherTypes.Ptp,
                    payload: serializer.BuildSync(Options, 2, new PtpTimestamp(10, 20))),
                EthernetFrameBuilder.BuildVlan(
                    PtpMulticastAddresses.General,
                    SourceMac,
                    vlanId: 100,
                    priorityCodePoint: 4,
                    etherType: EtherTypes.Ptp,
                    payload: serializer.BuildFollowUp(Options, 2, new PtpTimestamp(10, 20))),
                EthernetFrameBuilder.BuildQinQ(
                    PtpMulticastAddresses.PeerDelay,
                    SourceMac,
                    serviceVlanId: 200,
                    servicePriorityCodePoint: 4,
                    customerVlanId: 100,
                    customerPriorityCodePoint: 4,
                    etherType: EtherTypes.Ptp,
                    payload: serializer.BuildPdelayReq(Options, 3, new PtpTimestamp(11, 22)))
            },
            _ => throw new ArgumentOutOfRangeException(nameof(fileName), fileName, "Unknown golden PCAP fixture.")
        };

        return WritePcap(frames);
    }

    private static byte[] WritePcap(IEnumerable<byte[]> frames)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(0xA1B2C3D4u); // little-endian PCAP, microsecond timestamps
        writer.Write((ushort)2);
        writer.Write((ushort)4);
        writer.Write(0);
        writer.Write(0u);
        writer.Write(65535u);
        writer.Write(1u); // LINKTYPE_ETHERNET

        var index = 0;
        foreach (var frame in frames)
        {
            writer.Write(1_700_000_000u + (uint)index);
            writer.Write(1_000u + (uint)(index * 1_000));
            writer.Write((uint)frame.Length);
            writer.Write((uint)frame.Length);
            writer.Write(frame);
            index++;
        }

        return stream.ToArray();
    }

    private sealed record FixtureExpectation(
        string FileName,
        PtpMessageType[] MessageTypes,
        string? ExpectedTransport);
}
