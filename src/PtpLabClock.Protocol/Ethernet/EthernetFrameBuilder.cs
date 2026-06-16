// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Protocol.Ethernet;

public static class EthernetFrameBuilder
{
    public static byte[] Build(MacAddress destination, MacAddress source, ushort etherType, ReadOnlySpan<byte> payload)
    {
        var frame = new byte[14 + payload.Length];
        destination.Bytes.CopyTo(frame, 0);
        source.Bytes.CopyTo(frame, 6);
        WriteUInt16(frame, 12, etherType);
        payload.CopyTo(frame.AsSpan(14));
        return frame;
    }

    public static byte[] BuildVlan(
        MacAddress destination,
        MacAddress source,
        ushort vlanId,
        byte priorityCodePoint,
        ushort etherType,
        ReadOnlySpan<byte> payload)
    {
        ValidateVlan(vlanId, priorityCodePoint);

        var frame = new byte[18 + payload.Length];
        destination.Bytes.CopyTo(frame, 0);
        source.Bytes.CopyTo(frame, 6);
        WriteUInt16(frame, 12, EtherTypes.Vlan);
        WriteUInt16(frame, 14, BuildTci(vlanId, priorityCodePoint));
        WriteUInt16(frame, 16, etherType);
        payload.CopyTo(frame.AsSpan(18));
        return frame;
    }

    public static byte[] BuildQinQ(
        MacAddress destination,
        MacAddress source,
        ushort serviceVlanId,
        byte servicePriorityCodePoint,
        ushort customerVlanId,
        byte customerPriorityCodePoint,
        ushort etherType,
        ReadOnlySpan<byte> payload)
    {
        ValidateVlan(serviceVlanId, servicePriorityCodePoint);
        ValidateVlan(customerVlanId, customerPriorityCodePoint);

        var frame = new byte[22 + payload.Length];
        destination.Bytes.CopyTo(frame, 0);
        source.Bytes.CopyTo(frame, 6);
        WriteUInt16(frame, 12, EtherTypes.ProviderBridge);
        WriteUInt16(frame, 14, BuildTci(serviceVlanId, servicePriorityCodePoint));
        WriteUInt16(frame, 16, EtherTypes.Vlan);
        WriteUInt16(frame, 18, BuildTci(customerVlanId, customerPriorityCodePoint));
        WriteUInt16(frame, 20, etherType);
        payload.CopyTo(frame.AsSpan(22));
        return frame;
    }

    private static ushort BuildTci(ushort vlanId, byte priorityCodePoint)
    {
        return (ushort)(((priorityCodePoint & 0x07) << 13) | (vlanId & 0x0FFF));
    }

    private static void ValidateVlan(ushort vlanId, byte priorityCodePoint)
    {
        if (vlanId > 4094)
            throw new ArgumentOutOfRangeException(nameof(vlanId), "VLAN ID must be between 0 and 4094 for a lab Ethernet frame.");

        if (priorityCodePoint > 7)
            throw new ArgumentOutOfRangeException(nameof(priorityCodePoint), "VLAN priority/PCP must be between 0 and 7.");
    }

    private static void WriteUInt16(byte[] frame, int offset, ushort value)
    {
        frame[offset] = (byte)(value >> 8);
        frame[offset + 1] = (byte)(value & 0xFF);
    }
}
