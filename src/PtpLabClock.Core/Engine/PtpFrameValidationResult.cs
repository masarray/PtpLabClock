// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Engine;

public sealed class PtpFrameValidationResult
{
    public bool IsValid { get; init; }
    public string Error { get; init; } = string.Empty;
    public int PtpOffset { get; init; }
    public int FrameLength { get; init; }
    public int MessageLength { get; init; }
    public PtpMessageType MessageType { get; init; }
    public int Version { get; init; }
    public byte Domain { get; init; }
    public ushort SequenceId { get; init; }
    public string SourceClockIdentity { get; init; } = string.Empty;
    public string Transport { get; init; } = "Layer-2";

    public string Summary => IsValid
        ? $"{MessageType} seq={SequenceId} domain={Domain} len={MessageLength} offset={PtpOffset} src={SourceClockIdentity}"
        : $"Invalid PTP frame: {Error}";
}
