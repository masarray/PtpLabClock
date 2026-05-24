// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Core.Abstractions;

public interface IPtpTransport : IAsyncDisposable
{
    event EventHandler<PtpPacketReceivedEventArgs>? PacketReceived;

    Task OpenAsync(string adapterId, CancellationToken cancellationToken = default);
    Task StartCaptureAsync(CancellationToken cancellationToken = default);
    Task StopCaptureAsync(CancellationToken cancellationToken = default);
    Task SendAsync(byte[] frame, CancellationToken cancellationToken = default);
}
