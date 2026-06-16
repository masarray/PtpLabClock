# Timing Health Validation

Health mode evaluates passive monitor data into explainable checks.

```powershell
PtpLabClock.Console.exe --health --adapter-index 0 --domain 0
```

Current health checks cover:

- PTP visibility
- domain match
- GM/source stability
- Sync/Follow_Up pairing symptoms
- Pdelay activity
- sequence continuity
- analyzer readiness

Export report evidence:

```powershell
PtpLabClock.Console.exe --health --adapter-index 0 --domain 0 --export-report .\captures\ptp-health-report.pdf
PtpLabClock.Console.exe --health --adapter-index 0 --domain 0 --export-package .\captures\ptp-session-package.zip
```
