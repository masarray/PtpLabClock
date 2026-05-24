// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Core.Diagnostics;

public sealed class PtpEventLogItem
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string Severity { get; init; } = "INFO";
    public string Source { get; init; } = "ENGINE";
    public string Message { get; init; } = string.Empty;

    public override string ToString() => $"{Timestamp:HH:mm:ss.fff}  {Severity,-5}  {Source,-10}  {Message}";
}
