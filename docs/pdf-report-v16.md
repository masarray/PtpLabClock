# PDF Report v16 - Internal Evidence Report

v16 adds a formatted PDF report module using a small internal writer inside `PtpLabClock.Reporting`.

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

## CLI usage

Run passive health monitor and export PDF on stop:

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0 --export-report captures\ptp-health-report.pdf
```

Optional PCAP recording at the same time:

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0 --record-pcap captures\ptp-health.pcap --export-report captures\ptp-health-report.pdf
```

## License note

No external PDF rendering package is referenced by the current reporting module. If a third-party PDF library is added later, update `THIRD-PARTY-NOTICES.md` and verify license compatibility before release.
