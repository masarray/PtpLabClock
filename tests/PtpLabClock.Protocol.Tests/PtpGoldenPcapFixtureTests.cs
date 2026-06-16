// SPDX-License-Identifier: Apache-2.0
using System.Buffers.Binary;
using PtpLabClock.Core.Engine;
using PtpLabClock.Protocol.Enums;
using Xunit;

namespace PtpLabClock.Protocol.Tests;

public sealed class PtpGoldenPcapFixtureTests
{
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
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "pcap", fileName);
        Assert.True(File.Exists(path), $"Missing PCAP fixture: {path}");

        var bytes = File.ReadAllBytes(path);
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

    private sealed record FixtureExpectation(
        string FileName,
        PtpMessageType[] MessageTypes,
        string? ExpectedTransport);
}
