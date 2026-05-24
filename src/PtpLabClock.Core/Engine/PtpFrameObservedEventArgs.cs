// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Core.Engine;

public sealed class PtpFrameObservedEventArgs : EventArgs
{
    public PtpFrameObservedEventArgs(string direction, string label, byte[] frame, PtpFrameValidationResult inspection)
    {
        Direction = direction;
        Label = label;
        Frame = frame.ToArray();
        Inspection = inspection;
        Timestamp = DateTimeOffset.Now;
    }

    public DateTimeOffset Timestamp { get; }
    public string Direction { get; }
    public string Label { get; }
    public byte[] Frame { get; }
    public PtpFrameValidationResult Inspection { get; }

    public string Summary => $"{Direction} {Label} {Inspection.Summary}";
}
