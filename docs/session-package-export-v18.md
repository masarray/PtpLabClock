# v18 - Session Package Export

Process Bus Timing Lab can export a complete session evidence package as a ZIP file.

## Package contents

| File | Purpose |
|---|---|
| `report.pdf` | Formatted engineering report |
| `session.json` | Machine-readable session snapshot |
| `events.csv` | Event timeline export |
| `metadata.json` | Package metadata and recommended Wireshark filter |
| `README.txt` | Human-readable package guide and scope note |

## WPF usage

Use the `Export Package` button in the action bar. The app opens a save dialog and writes a `.zip` evidence bundle.

## Console usage

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0 --export-package captures\ptp-session-package.zip
```

With PDF and PCAP at the same time:

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0 --record-pcap captures\ptp-health.pcap --export-report captures\ptp-health-report.pdf --export-package captures\ptp-session-package.zip
```

## Scope note

The package is lab diagnostic evidence only. It is not a timing-accuracy acceptance record and must not be used as a replacement for a GPS/PTP grandmaster validation report.
