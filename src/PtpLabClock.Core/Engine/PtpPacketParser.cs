// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Engine;

internal static class PtpPacketParser
{
    private const ushort EtherTypePtp = 0x88F7;
    private const ushort EtherTypeVlan = 0x8100;
    private const ushort EtherTypeProviderBridge = 0x88A8;

    public static bool TryReadPdelayReq(byte[] frame, out ushort sequenceId, out byte[] requestingPortIdentity)
    {
        return TryReadPdelayReq(frame, expectedDomain: null, out sequenceId, out requestingPortIdentity, out _);
    }

    public static bool TryReadPdelayReq(byte[] frame, byte? expectedDomain, out ushort sequenceId, out byte[] requestingPortIdentity, out string rejectReason)
    {
        sequenceId = 0;
        requestingPortIdentity = Array.Empty<byte>();
        rejectReason = string.Empty;

        if (!TryGetPtpOffset(frame, out var offset))
        {
            rejectReason = "Not a Layer-2 PTP frame.";
            return false;
        }

        if (frame.Length < offset + 34)
        {
            rejectReason = "PTP frame shorter than common header.";
            return false;
        }

        var messageType = (PtpMessageType)(frame[offset] & 0x0F);
        if (messageType != PtpMessageType.PdelayReq)
        {
            rejectReason = $"Ignored PTP message type {messageType}.";
            return false;
        }

        var version = frame[offset + 1] & 0x0F;
        if (version != 2)
        {
            rejectReason = $"Unsupported PTP version {version}.";
            return false;
        }

        var messageLength = (ushort)((frame[offset + 2] << 8) | frame[offset + 3]);
        if (messageLength < 44 || frame.Length < offset + messageLength)
        {
            rejectReason = $"Malformed Pdelay_Req length {messageLength}.";
            return false;
        }

        var domain = frame[offset + 4];
        if (expectedDomain.HasValue && domain != expectedDomain.Value)
        {
            rejectReason = $"Pdelay_Req domain mismatch. RX={domain}, expected={expectedDomain.Value}.";
            return false;
        }

        sequenceId = (ushort)((frame[offset + 30] << 8) | frame[offset + 31]);
        requestingPortIdentity = frame.AsSpan(offset + 20, 10).ToArray();
        return true;
    }

    private static bool TryGetPtpOffset(byte[] frame, out int offset)
    {
        offset = 0;
        if (frame.Length < 14) return false;

        var cursor = 12;
        var etherType = ReadUInt16(frame, cursor);
        cursor += 2;

        // Support untagged PTP, single VLAN, and common double-tagged QinQ frames.
        for (var depth = 0; depth < 2; depth++)
        {
            if (etherType == EtherTypePtp)
            {
                offset = cursor;
                return true;
            }

            if (etherType != EtherTypeVlan && etherType != EtherTypeProviderBridge)
                return false;

            if (frame.Length < cursor + 4)
                return false;

            cursor += 2; // skip TCI
            etherType = ReadUInt16(frame, cursor);
            cursor += 2;
        }

        if (etherType == EtherTypePtp)
        {
            offset = cursor;
            return true;
        }

        return false;
    }

    private static ushort ReadUInt16(byte[] buffer, int offset) => (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
}
