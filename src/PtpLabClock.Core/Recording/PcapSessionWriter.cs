// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Recording;

/// <summary>
/// Minimal Ethernet PCAP writer for lab evidence capture.
/// The file format is classic PCAP with LINKTYPE_ETHERNET so it opens directly in Wireshark.
/// </summary>
public sealed class PcapSessionWriter : IDisposable
{
    private const uint Magic = 0xA1B2C3D4;
    private const ushort MajorVersion = 2;
    private const ushort MinorVersion = 4;
    private const uint SnapLength = 65535;
    private const uint LinkTypeEthernet = 1;

    private readonly object _gate = new();
    private readonly FileStream _stream;
    private readonly BinaryWriter _writer;
    private bool _disposed;

    public PcapSessionWriter(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("PCAP file path is required.", nameof(filePath));

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        _stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        _writer = new BinaryWriter(_stream);
        WriteGlobalHeader();
    }

    public long PacketCount { get; private set; }

    public void WriteFrame(byte[] frame)
    {
        if (frame.Length == 0)
            return;

        WriteFrame(frame, DateTimeOffset.UtcNow);
    }

    public void WriteFrame(byte[] frame, DateTimeOffset timestamp)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PcapSessionWriter));

        if (frame.Length == 0)
            return;

        var utc = timestamp.ToUniversalTime();
        var seconds = utc.ToUnixTimeSeconds();
        var microseconds = (utc.Ticks % TimeSpan.TicksPerSecond) / 10;
        var capturedLength = Math.Min(frame.Length, (int)SnapLength);

        lock (_gate)
        {
            _writer.Write((uint)seconds);
            _writer.Write((uint)microseconds);
            _writer.Write((uint)capturedLength);
            _writer.Write((uint)frame.Length);
            _writer.Write(frame, 0, capturedLength);
            PacketCount++;
        }
    }

    public void Flush()
    {
        lock (_gate)
        {
            _writer.Flush();
            _stream.Flush(true);
        }
    }

    private void WriteGlobalHeader()
    {
        _writer.Write(Magic);
        _writer.Write(MajorVersion);
        _writer.Write(MinorVersion);
        _writer.Write(0); // thiszone
        _writer.Write(0u); // sigfigs
        _writer.Write(SnapLength);
        _writer.Write(LinkTypeEthernet);
        _writer.Flush();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Flush();
        _writer.Dispose();
        _stream.Dispose();
        _disposed = true;
    }
}
