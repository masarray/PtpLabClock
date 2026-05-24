# PDF Report v16 - QuestPDF Evidence Report

v16 adds a formatted PDF report module using QuestPDF.

## Scope

The report is designed as an engineering evidence pack for lab validation:

- session metadata
- adapter/profile/domain
- timing health summary
- PASS/WARN/FAIL checks
- runtime counters
- passive monitor snapshot
- detected source clock table
- event timeline excerpt
- lab-only safety note

It is not an official timing accuracy certificate and must not be used as a GPS/PTP grandmaster acceptance report.

## CLI Usage

Run passive health monitor and export PDF on stop:

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0 --export-report captures\ptp-health-report.pdf
```

Optional PCAP recording at the same time:

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0 --record-pcap captures\ptp-health.pcap --export-report captures\ptp-health-report.pdf
```

## QuestPDF License Note

The project uses QuestPDF via `PtpLabClock.Reporting`. The code sets:

```csharp
QuestPDF.Settings.License = LicenseType.Community;
```

Review QuestPDF's current license terms before distributing commercially, especially for companies above the stated revenue threshold.

## Next Step

v17 should integrate report export into WPF with a small button and a save-file dialog, without changing the locked visual system.
