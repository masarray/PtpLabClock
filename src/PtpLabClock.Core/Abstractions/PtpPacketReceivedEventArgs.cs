// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Abstractions;

public sealed class PtpPacketReceivedEventArgs : EventArgs
{
    public PtpPacketReceivedEventArgs(byte[] frame)
    {
        Frame = frame;
    }

    public byte[] Frame { get; }
}
