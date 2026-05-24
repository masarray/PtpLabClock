// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Engine;

/// <summary>
/// Lightweight byte-level sanity checker for outbound and captured Layer-2 PTP frames.
/// This is intentionally conservative: it validates the common header, EtherType/VLAN offset,
/// message length, version, domain, sequence ID, and sourcePortIdentity so Wireshark checks
/// can be matched against engine logs.
/// </summary>
public static class PtpFrameInspector
{
    private const ushort EtherTypePtp = 0x88F7;
    private const ushort EtherTypeVlan = 0x8100;
    private const ushort EtherTypeProviderBridge = 0x88A8;

    public static PtpFrameValidationResult Inspect(byte[] frame, byte? expectedDomain = null)
    {
        if (!TryGetPtpOffset(frame, out var offset, out var transport, out var error))
            return Invalid(frame, error);

        if (frame.Length < offset + 34)
            return Invalid(frame, $"PTP common header truncated. frame={frame.Length}, offset={offset}.");

        var messageType = (PtpMessageType)(frame[offset] & 0x0F);
        var version = frame[offset + 1] & 0x0F;
        var messageLength = ReadUInt16(frame, offset + 2);
        var domain = frame[offset + 4];
        var sequenceId = ReadUInt16(frame, offset + 30);
        var sourcePortIdentity = ToHex(frame.AsSpan(offset + 20, 8));

        if (version != 2)
            return Invalid(frame, $"Unsupported PTP version {version}.", offset, messageLength, messageType, version, domain, sequenceId, sourcePortIdentity, transport);

        if (messageLength < 34)
            return Invalid(frame, $"PTP messageLength too short: {messageLength}.", offset, messageLength, messageType, version, domain, sequenceId, sourcePortIdentity, transport);

        if (frame.Length < offset + messageLength)
            return Invalid(frame, $"PTP payload truncated. messageLength={messageLength}, available={frame.Length - offset}.", offset, messageLength, messageType, version, domain, sequenceId, sourcePortIdentity, transport);

        if (expectedDomain.HasValue && domain != expectedDomain.Value)
            return Invalid(frame, $"Domain mismatch. RX/TX={domain}, expected={expectedDomain.Value}.", offset, messageLength, messageType, version, domain, sequenceId, sourcePortIdentity, transport);

        return new PtpFrameValidationResult
        {
            IsValid = true,
            FrameLength = frame.Length,
            PtpOffset = offset,
            MessageLength = messageLength,
            MessageType = messageType,
            Version = version,
            Domain = domain,
            SequenceId = sequenceId,
            SourceClockIdentity = sourcePortIdentity,
            Transport = transport
        };
    }

    private static PtpFrameValidationResult Invalid(byte[] frame, string error, int offset = 0, int messageLength = 0,
        PtpMessageType messageType = default, int version = 0, byte domain = 0, ushort sequenceId = 0,
        string sourceClockIdentity = "", string transport = "Layer-2")
    {
        return new PtpFrameValidationResult
        {
            IsValid = false,
            Error = error,
            FrameLength = frame.Length,
            PtpOffset = offset,
            MessageLength = messageLength,
            MessageType = messageType,
            Version = version,
            Domain = domain,
            SequenceId = sequenceId,
            SourceClockIdentity = sourceClockIdentity,
            Transport = transport
        };
    }

    private static bool TryGetPtpOffset(byte[] frame, out int offset, out string transport, out string error)
    {
        offset = 0;
        transport = "Layer-2";
        error = string.Empty;

        if (frame.Length < 14)
        {
            error = $"Ethernet frame too short: {frame.Length}.";
            return false;
        }

        var cursor = 12;
        var etherType = ReadUInt16(frame, cursor);
        cursor += 2;

        for (var depth = 0; depth <= 2; depth++)
        {
            if (etherType == EtherTypePtp)
            {
                offset = cursor;
                transport = depth == 0 ? "Layer-2" : depth == 1 ? "Layer-2 VLAN" : "Layer-2 QinQ";
                return true;
            }

            if (etherType != EtherTypeVlan && etherType != EtherTypeProviderBridge)
            {
                error = $"Unexpected EtherType 0x{etherType:X4}; expected PTP 0x88F7 or VLAN-tagged PTP.";
                return false;
            }

            if (frame.Length < cursor + 4)
            {
                error = "VLAN header truncated.";
                return false;
            }

            cursor += 2; // TCI
            etherType = ReadUInt16(frame, cursor);
            cursor += 2;
        }

        error = "PTP EtherType not found after supported VLAN depth.";
        return false;
    }

    private static ushort ReadUInt16(byte[] buffer, int offset) => (ushort)((buffer[offset] << 8) | buffer[offset + 1]);

    private static string ToHex(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return string.Empty;

        return string.Join("-", bytes.ToArray().Select(b => b.ToString("X2")));
    }
}
