// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Monitor;

public sealed class PtpObservedMessage
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string Direction { get; init; } = "RX";
    public PtpMessageType MessageType { get; init; }
    public byte Domain { get; init; }
    public ushort SequenceId { get; init; }
    public string SourceClockIdentity { get; init; } = string.Empty;
    public string Transport { get; init; } = "Layer-2";
    public int FrameLength { get; init; }
    public int MessageLength { get; init; }

    public string Summary => $"{Direction} {MessageType} seq={SequenceId} domain={Domain} src={SourceClockIdentity}";
}
