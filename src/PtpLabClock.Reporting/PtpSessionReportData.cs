// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Core.Diagnostics;
using PtpLabClock.Core.Health;
using PtpLabClock.Core.Monitor;

namespace PtpLabClock.Reporting;

public sealed class PtpSessionReportData
{
    public string Title { get; init; } = "Process Bus Timing Lab - Session Report";
    public string Subtitle { get; init; } = "PTP timing visibility, health, and evidence summary";
    public string ProjectName { get; init; } = "Lab Validation";
    public string OperatorName { get; init; } = Environment.UserName;
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
    public DateTime SessionStartedAt { get; init; } = DateTime.Now;
    public DateTime SessionEndedAt { get; init; } = DateTime.Now;
    public string AdapterName { get; init; } = string.Empty;
    public string ProfileName { get; init; } = string.Empty;
    public byte DomainNumber { get; init; }
    public string Mode { get; init; } = "Monitor";
    public string WiresharkFilter { get; init; } = "eth.type == 0x88f7";
    public string LabDisclaimer { get; init; } = "Synthetic and diagnostic evidence only. Not a GPS/PTP grandmaster replacement and not valid for timing accuracy acceptance.";
    public PtpRuntimeCounters Counters { get; init; } = new();
    public PtpMonitorSnapshot? MonitorSnapshot { get; init; }
    public PtpHealthSnapshot? HealthSnapshot { get; init; }
    public IReadOnlyList<PtpEventLogItem> Events { get; init; } = Array.Empty<PtpEventLogItem>();
}
