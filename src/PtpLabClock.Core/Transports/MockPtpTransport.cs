// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Core.Abstractions;
using PtpLabClock.Protocol.Ethernet;

namespace PtpLabClock.Core.Transports;

/// <summary>
/// UI-safe transport used when Npcap is not installed, Visual Studio is not elevated,
/// or the user only wants to validate the dashboard workflow. It does not send packets.
/// It periodically injects a synthetic Pdelay_Req into the engine so the responder path,
/// counters, and event timeline can be tested without hardware.
/// </summary>
public sealed class MockPtpTransport : IPtpTransport
{
    private CancellationTokenSource? _cts;
    private Task? _rxTask;
    private ushort _sequence;

    public event EventHandler<PtpPacketReceivedEventArgs>? PacketReceived;

    public Task OpenAsync(string adapterId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StartCaptureAsync(CancellationToken cancellationToken = default)
    {
        _cts = new CancellationTokenSource();
        _rxTask = Task.Run(() => RunSyntheticRxAsync(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopCaptureAsync(CancellationToken cancellationToken = default)
    {
        if (_cts is null) return;
        _cts.Cancel();
        try
        {
            if (_rxTask is not null)
                await _rxTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected during stop
        }
    }

    public Task SendAsync(byte[] frame, CancellationToken cancellationToken = default)
    {
        // Intentionally no-op. This mode is for UI/engine validation only.
        return Task.CompletedTask;
    }

    private async Task RunSyntheticRxAsync(CancellationToken token)
    {
        await Task.Delay(1200, token).ConfigureAwait(false);
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(2500));
        while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
        {
            PacketReceived?.Invoke(this, new PtpPacketReceivedEventArgs(BuildPdelayReq(++_sequence)));
        }
    }

    private static byte[] BuildPdelayReq(ushort sequenceId)
    {
        var payload = new byte[44];
        payload[0] = 0x02; // messageType = Pdelay_Req
        payload[1] = 0x02; // PTP v2
        payload[2] = 0x00;
        payload[3] = 0x2C; // 44 bytes
        payload[4] = 0x00; // domain
        // flags/correction/reserved remain zero
        var sourcePortIdentity = new byte[] { 0x02, 0x00, 0x00, 0xFF, 0xFE, 0xAA, 0x10, 0x01, 0x00, 0x01 };
        Buffer.BlockCopy(sourcePortIdentity, 0, payload, 20, sourcePortIdentity.Length);
        payload[30] = (byte)(sequenceId >> 8);
        payload[31] = (byte)(sequenceId & 0xFF);
        payload[32] = 5;     // controlField for peer-delay response family in many dissectors
        payload[33] = 0x7F;  // logMessageInterval not applicable

        var src = MacAddress.Parse("02-00-00-AA-10-01");
        return EthernetFrameBuilder.Build(PtpMulticastAddresses.PeerDelay, src, EtherTypes.Ptp, payload);
    }

    public async ValueTask DisposeAsync()
    {
        await StopCaptureAsync().ConfigureAwait(false);
        _cts?.Dispose();
    }
}
