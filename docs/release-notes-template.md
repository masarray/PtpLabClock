<!-- SPDX-License-Identifier: Apache-2.0 -->
# Release Notes Template

Use this template when publishing a GitHub Release.

## Process Bus Timing Lab `<version>`

Windows PTP lab simulator and timing-health monitor for IEC 61850 FAT, SAT, analyzer validation, and Process Bus troubleshooting.

### Downloads

| Asset | Purpose |
|---|---|
| `PtpLabClock.App.win-x64.portable.exe` | Desktop dashboard, self-contained single-file app. |
| `PtpLabClock.Console.win-x64.portable.exe` | CLI validation, RAW self-test, passive monitor, and scripts. |
| `PtpLabClock.App.win-x64.portable.zip` | App EXE plus license, notices, and release README. |
| `PtpLabClock.Console.win-x64.portable.zip` | Console EXE plus license, notices, and release README. |
| `checksums.txt` | SHA256 verification for release assets. |
| `PtpLabClock.release-sbom.spdx.json` | SPDX-style dependency and artifact manifest. |
| `ptp-validation.pcap` | Protocol smoke-validation PCAP generated during release. |

### What changed

- Add highlights here.
- Add protocol or UI changes here.
- Add documentation and test improvements here.

### How to verify

```powershell
Get-FileHash .\PtpLabClock.App.win-x64.portable.exe -Algorithm SHA256
Get-Content .\checksums.txt
```

Run protocol smoke validation from source:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0
```

RAW adapter self-test:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --list
dotnet run --project .\src\PtpLabClock.Console -- --raw-self-test --adapter-index 0 --domain 0
```

### Known limitations

- Portable EXE artifacts are not code-signed yet and may trigger Windows SmartScreen on first run.
- RAW NIC mode depends on Npcap, adapter driver support, and administrator privileges.
- Local self-capture failure does not always mean packet transmission failed; verify with external capture when possible.
- Software timestamps are diagnostic only and are not hardware-timestamped timing evidence.
- This project is not a certified PTP grandmaster, GPS clock, or relay-acceptance timing source.

### Safety boundary

Use this tool in controlled lab, FAT, SAT preparation, analyzer validation, and approved engineering networks. Do not publish synthetic traffic into operational protection networks without explicit engineering approval.
