// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Protocol.Ethernet;

public static class EthernetFrameBuilder
{
    public static byte[] Build(MacAddress destination, MacAddress source, ushort etherType, ReadOnlySpan<byte> payload)
    {
        var frame = new byte[14 + payload.Length];
        destination.Bytes.CopyTo(frame, 0);
        source.Bytes.CopyTo(frame, 6);
        frame[12] = (byte)(etherType >> 8);
        frame[13] = (byte)(etherType & 0xFF);
        payload.CopyTo(frame.AsSpan(14));
        return frame;
    }
}
