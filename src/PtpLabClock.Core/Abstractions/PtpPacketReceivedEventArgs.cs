// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Core.Abstractions;

public sealed class PtpPacketReceivedEventArgs : EventArgs
{
    public PtpPacketReceivedEventArgs(byte[] frame)
    {
        Frame = frame;
    }

    public byte[] Frame { get; }
}
