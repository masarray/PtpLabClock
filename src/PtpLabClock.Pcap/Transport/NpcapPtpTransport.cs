// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Core.Abstractions;
using PtpLabClock.Pcap.Infrastructure;
using SharpPcap;

namespace PtpLabClock.Pcap.Transport;

/// <summary>
/// Layer-2 PTP transport over Npcap/libpcap using SharpPcap.
/// </summary>
public sealed class NpcapPtpTransport : IPtpTransport
{
    private readonly object _sendGate = new();
    private ILiveDevice? _device;
    private bool _opened;
    private bool _capturing;

    public event EventHandler<PtpPacketReceivedEventArgs>? PacketReceived;

    public Task OpenAsync(string adapterId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_opened)
            return Task.CompletedTask;

        var deviceName = PcapAdapterId.ToDeviceName(adapterId);
        var device = CaptureDeviceList.Instance
            .OfType<ILiveDevice>()
            .FirstOrDefault(d => string.Equals(d.Name, deviceName, StringComparison.OrdinalIgnoreCase));

        if (device is null)
        {
            throw new InvalidOperationException(
                "Npcap adapter was not found. Refresh adapters, select a listed RAW adapter, and make sure Npcap is installed.");
        }

        try
        {
            device.OnPacketArrival += OnPacketArrival;
            device.Open(new DeviceConfiguration
            {
                Mode = DeviceModes.Promiscuous,
                Immediate = true,
                ReadTimeout = 1
            });

            // Layer-2 PTP EtherType for untagged, single VLAN and common QinQ frames.
            device.Filter = PtpCaptureFilters.Layer2PtpWithVlan;

            _device = device;
            _opened = true;
        }
        catch (Exception ex)
        {
            device.OnPacketArrival -= OnPacketArrival;
            try { device.Dispose(); } catch { }

            throw new InvalidOperationException(
                "Failed to open RAW adapter through Npcap. Run the app as Administrator, confirm Npcap is installed, and avoid Wi-Fi/VPN adapters that block injection. " +
                ex.Message,
                ex);
        }

        return Task.CompletedTask;
    }

    public Task StartCaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var device = EnsureDevice();

        if (!_capturing)
        {
            device.StartCapture();
            _capturing = true;
        }

        return Task.CompletedTask;
    }

    public Task StopCaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_device is not null && _capturing)
        {
            try { _device.StopCapture(); }
            finally { _capturing = false; }
        }

        return Task.CompletedTask;
    }

    public Task SendAsync(byte[] frame, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (frame is null || frame.Length < 14)
            throw new ArgumentException("Ethernet frame is empty or shorter than the Ethernet header.", nameof(frame));

        var device = EnsureDevice();

        try
        {
            lock (_sendGate)
            {
                device.SendPacket(frame);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Npcap failed to inject the Ethernet frame. Check administrator rights, adapter type, driver state, and whether the NIC allows raw packet injection. " +
                "Wired Ethernet adapters are more reliable than Wi-Fi/VPN/virtual adapters for Layer-2 process-bus tests. " +
                ex.Message,
                ex);
        }

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopCaptureAsync(CancellationToken.None).ConfigureAwait(false);

        if (_device is not null)
        {
            _device.OnPacketArrival -= OnPacketArrival;
            try { _device.Dispose(); } catch { }
            _device = null;
        }

        _opened = false;
    }

    private ILiveDevice EnsureDevice()
    {
        if (!_opened || _device is null)
            throw new InvalidOperationException("RAW packet transport is not open.");

        return _device;
    }

    private void OnPacketArrival(object sender, PacketCapture packetCapture)
    {
        try
        {
            var raw = packetCapture.GetPacket();
            if (raw.Data.Length == 0)
                return;

            var frame = raw.Data.ToArray();
            PacketReceived?.Invoke(this, new PtpPacketReceivedEventArgs(frame));
        }
        catch
        {
            // Packet arrival callbacks run on the capture thread. Do not allow a
            // malformed packet or downstream handler exception to kill capture.
        }
    }
}
