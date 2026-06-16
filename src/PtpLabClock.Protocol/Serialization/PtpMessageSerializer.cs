// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Messages;

namespace PtpLabClock.Protocol.Serialization;

public sealed class PtpMessageSerializer
{
    public byte[] BuildAnnounce(PtpBuildOptions options, ushort sequenceId)
    {
        const ushort length = 64;
        var buffer = CreateHeader(PtpMessageType.Announce, length, options, sequenceId, controlField: 5, logInterval: options.AnnounceLogInterval, flags: 0);
        var writer = new BigEndianWriter(buffer) { };
        writer.Seek(34);
        PtpTimestamp.Now().WriteTo(writer);
        writer.WriteUInt16(37); // currentUtcOffset, lab placeholder
        writer.WriteByte(0);
        writer.WriteByte(options.Priority1);
        writer.WriteByte(options.ClockClass);
        writer.WriteByte((byte)options.ClockAccuracy);
        writer.WriteUInt16(options.OffsetScaledLogVariance);
        writer.WriteByte(options.Priority2);
        writer.WriteBytes(options.ClockIdentity.Bytes);
        writer.WriteUInt16(0); // stepsRemoved
        writer.WriteByte((byte)options.TimeSource);
        return buffer;
    }

    public byte[] BuildSync(PtpBuildOptions options, ushort sequenceId)
    {
        return BuildSync(options, sequenceId, PtpTimestamp.Now());
    }

    public byte[] BuildSync(PtpBuildOptions options, ushort sequenceId, PtpTimestamp originTimestamp)
    {
        const ushort length = 44;
        ushort flags = options.TwoStep ? (ushort)0x0200 : (ushort)0;
        var buffer = CreateHeader(PtpMessageType.Sync, length, options, sequenceId, controlField: 0, logInterval: options.SyncLogInterval, flags: flags);
        var writer = new BigEndianWriter(buffer);
        writer.Seek(34);
        originTimestamp.WriteTo(writer);
        return buffer;
    }

    public byte[] BuildFollowUp(PtpBuildOptions options, ushort sequenceId)
    {
        return BuildFollowUp(options, sequenceId, PtpTimestamp.Now());
    }

    public byte[] BuildFollowUp(PtpBuildOptions options, ushort sequenceId, PtpTimestamp preciseOriginTimestamp)
    {
        const ushort length = 44;
        var buffer = CreateHeader(PtpMessageType.FollowUp, length, options, sequenceId, controlField: 2, logInterval: options.SyncLogInterval, flags: 0);
        var writer = new BigEndianWriter(buffer);
        writer.Seek(34);
        preciseOriginTimestamp.WriteTo(writer);
        return buffer;
    }

    public byte[] BuildPdelayReq(PtpBuildOptions options, ushort sequenceId)
    {
        return BuildPdelayReq(options, sequenceId, PtpTimestamp.Now());
    }

    public byte[] BuildPdelayReq(PtpBuildOptions options, ushort sequenceId, PtpTimestamp originTimestamp)
    {
        const ushort length = 54;
        var buffer = CreateHeader(PtpMessageType.PdelayReq, length, options, sequenceId, controlField: 5, logInterval: 0x7F, flags: 0);
        var writer = new BigEndianWriter(buffer);
        writer.Seek(34);
        originTimestamp.WriteTo(writer);
        writer.Skip(10); // reserved
        return buffer;
    }

    public byte[] BuildPdelayResp(PtpBuildOptions options, ushort sequenceId, ReadOnlySpan<byte> requestingPortIdentity)
    {
        return BuildPdelayResp(options, sequenceId, requestingPortIdentity, PtpTimestamp.Now());
    }

    public byte[] BuildPdelayResp(PtpBuildOptions options, ushort sequenceId, ReadOnlySpan<byte> requestingPortIdentity, PtpTimestamp requestReceiptTimestamp)
    {
        const ushort length = 54;
        var buffer = CreateHeader(PtpMessageType.PdelayResp, length, options, sequenceId, controlField: 5, logInterval: 0x7F, flags: 0x0200);
        var writer = new BigEndianWriter(buffer);
        writer.Seek(34);
        requestReceiptTimestamp.WriteTo(writer);
        WritePortIdentityOrZero(writer, requestingPortIdentity);
        return buffer;
    }

    public byte[] BuildPdelayRespFollowUp(PtpBuildOptions options, ushort sequenceId, ReadOnlySpan<byte> requestingPortIdentity)
    {
        return BuildPdelayRespFollowUp(options, sequenceId, requestingPortIdentity, PtpTimestamp.Now());
    }

    public byte[] BuildPdelayRespFollowUp(PtpBuildOptions options, ushort sequenceId, ReadOnlySpan<byte> requestingPortIdentity, PtpTimestamp responseOriginTimestamp)
    {
        const ushort length = 54;
        var buffer = CreateHeader(PtpMessageType.PdelayRespFollowUp, length, options, sequenceId, controlField: 5, logInterval: 0x7F, flags: 0);
        var writer = new BigEndianWriter(buffer);
        writer.Seek(34);
        responseOriginTimestamp.WriteTo(writer);
        WritePortIdentityOrZero(writer, requestingPortIdentity);
        return buffer;
    }

    private static byte[] CreateHeader(PtpMessageType messageType, ushort length, PtpBuildOptions options, ushort sequenceId, byte controlField, sbyte logInterval, ushort flags)
    {
        var buffer = new byte[length];
        var writer = new BigEndianWriter(buffer);
        writer.WriteByte((byte)((0 << 4) | ((byte)messageType & 0x0F))); // transportSpecific + messageType
        writer.WriteByte(0x02); // PTP version 2
        writer.WriteUInt16(length);
        writer.WriteByte(options.DomainNumber);
        writer.WriteByte(0); // reserved
        writer.WriteUInt16(flags);
        writer.WriteUInt64(0); // correctionField
        writer.WriteUInt32(0); // reserved
        writer.WriteBytes(options.ClockIdentity.Bytes);
        writer.WriteUInt16(options.PortNumber);
        writer.WriteUInt16(sequenceId);
        writer.WriteByte(controlField);
        writer.WriteByte(unchecked((byte)logInterval));
        return buffer;
    }

    private static void WritePortIdentityOrZero(BigEndianWriter writer, ReadOnlySpan<byte> requestingPortIdentity)
    {
        if (requestingPortIdentity.Length >= 10)
            writer.WriteBytes(requestingPortIdentity[..10]);
        else
            writer.Skip(10);
    }
}
