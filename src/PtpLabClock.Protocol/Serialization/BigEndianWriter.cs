// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Protocol.Serialization;

public sealed class BigEndianWriter
{
    private readonly byte[] _buffer;
    private int _offset;

    public BigEndianWriter(byte[] buffer)
    {
        _buffer = buffer;
    }

    public int Offset => _offset;

    public void Seek(int offset)
    {
        if (offset < 0 || offset > _buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        _offset = offset;
    }

    public void WriteByte(byte value) => _buffer[_offset++] = value;

    public void WriteUInt16(ushort value)
    {
        _buffer[_offset++] = (byte)(value >> 8);
        _buffer[_offset++] = (byte)(value & 0xFF);
    }

    public void WriteUInt32(uint value)
    {
        _buffer[_offset++] = (byte)(value >> 24);
        _buffer[_offset++] = (byte)(value >> 16);
        _buffer[_offset++] = (byte)(value >> 8);
        _buffer[_offset++] = (byte)(value & 0xFF);
    }

    public void WriteUInt64(ulong value)
    {
        _buffer[_offset++] = (byte)(value >> 56);
        _buffer[_offset++] = (byte)(value >> 48);
        _buffer[_offset++] = (byte)(value >> 40);
        _buffer[_offset++] = (byte)(value >> 32);
        _buffer[_offset++] = (byte)(value >> 24);
        _buffer[_offset++] = (byte)(value >> 16);
        _buffer[_offset++] = (byte)(value >> 8);
        _buffer[_offset++] = (byte)(value & 0xFF);
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        bytes.CopyTo(_buffer.AsSpan(_offset));
        _offset += bytes.Length;
    }

    public void Skip(int count)
    {
        Array.Clear(_buffer, _offset, count);
        _offset += count;
    }
}
