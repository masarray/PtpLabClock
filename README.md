# Process Bus Timing Lab

[![Build and Test](https://github.com/masarray/PtpLabClock/actions/workflows/build.yml/badge.svg)](https://github.com/masarray/PtpLabClock/actions/workflows/build.yml)
[![CodeQL](https://github.com/masarray/PtpLabClock/actions/workflows/codeql.yml/badge.svg)](https://github.com/masarray/PtpLabClock/actions/workflows/codeql.yml)
[![OpenSSF Scorecard](https://github.com/masarray/PtpLabClock/actions/workflows/scorecard.yml/badge.svg)](https://github.com/masarray/PtpLabClock/actions/workflows/scorecard.yml)
[![GitHub Pages](https://github.com/masarray/PtpLabClock/actions/workflows/pages.yml/badge.svg)](https://github.com/masarray/PtpLabClock/actions/workflows/pages.yml)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)

**PTP Lab Clock Simulator and Process Bus Timing Monitor for IEC 61850 lab work.**

Process Bus Timing Lab is a Windows-friendly **PTPv2 Layer-2 lab simulator**, **IEC 61850 Process Bus timing visibility tool**, and **passive PTP health monitor** for FAT, SAT, commissioning preparation, analyzer validation, VLAN/QinQ verification, and controlled timing-fault scenarios.

**Product website:** https://masarray.github.io/PtpLabClock/  
**Download:** https://github.com/masarray/PtpLabClock/releases  
**License:** Apache-2.0

## What problem does it solve?

Substation automation engineers often need to know whether the test network actually carries usable PTP traffic before troubleshooting Sampled Values, GOOSE, or protection relay behavior. This project helps answer practical questions quickly:

- “Can Wireshark or my analyzer see PTP on this NIC?”
- “Are Announce / Sync / Follow_Up / Pdelay frames decoded correctly?”
- “Is this process-bus VLAN carrying visible PTP traffic?”
- “Can I reproduce GM-lost, missing-Follow_Up, sequence-jump, or degraded-clock symptoms?”
- “Can I export a short PDF/ZIP evidence package for engineering discussion?”

> **Safety boundary:** this project is a lab simulator and diagnostic companion. It is **not** a certified timing source, GPS grandmaster, hardware-timestamped clock, or relay-acceptance timing reference.

## Best-fit use cases

- IEC 61850 Process Bus lab validation.
- IEC/IEEE 61850-9-3 visibility checks.
- PTPv2 Layer-2 analyzer validation.
- Sampled Values / GOOSE timing-context troubleshooting.
- VLAN and QinQ PTP frame visibility testing.
- Npcap / SharpPcap RAW NIC smoke testing on Windows.
- FAT/SAT preparation and issue evidence export.

## Highlights

- WPF dashboard with Demo Mode fallback.
- Console tool for adapter listing, protocol validation, RAW self-test, passive monitor, health monitor, and evidence export.
- PTPv2 Layer-2 serializer for Announce, Sync, Follow_Up, Pdelay_Req, Pdelay_Resp, and Pdelay_Resp_Follow_Up.
- Untagged, VLAN, and QinQ Ethernet frame builders.
- SharpPcap/Npcap RAW transport isolated behind a dedicated project.
- VLAN-aware capture filter for untagged, VLAN, and QinQ PTP.
- Passive monitor grouped by domain and source clock identity.
- Timing-health validator with explainable PASS/WARN/FAIL checks.
- PDF report and ZIP session evidence export.
- xUnit protocol regression tests and GitHub Actions CI.
- Apache-2.0 license.

## Portable release packages

Release automation produces direct self-contained Windows single-file EXE artifacts:

| Package | Use when |
|---|---|
| `PtpLabClock.App.win-x64.portable.exe` | You want the easiest Windows desktop app: download and run. |
| `PtpLabClock.Console.win-x64.portable.exe` | You want CLI validation, RAW self-test, monitor, or scripting. |
| `PtpLabClock.App.win-x64.portable.zip` | You want the app EXE bundled with license notices and README. |
| `PtpLabClock.Console.win-x64.portable.zip` | You want the CLI EXE bundled with license notices and README. |

Each release also includes `checksums.txt`, `PtpLabClock.release-sbom.spdx.json`, and a protocol validation PCAP artifact.

## 60-second source build

```powershell
git clone https://github.com/masarray/PtpLabClock.git
cd PtpLabClock
dotnet restore .\PtpLabClock.sln
dotnet build .\PtpLabClock.sln -c Release
dotnet test .\PtpLabClock.sln -c Release
dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0
```

Run the WPF app:

```powershell
dotnet run --project .\src\PtpLabClock.App
```

Build local portable EXE artifacts:

```powershell
.\tools\scripts\package-release.ps1
```

## RAW NIC quick start

RAW mode requires Npcap installed on Windows and may require Administrator privileges.

```powershell
# List RAW adapters exposed by Npcap
dotnet run --project .\src\PtpLabClock.Console -- --list

# Untagged RAW self-test
dotnet run --project .\src\PtpLabClock.Console -- --raw-self-test --adapter-index 0 --domain 0

# VLAN-tagged RAW self-test
dotnet run --project .\src\PtpLabClock.Console -- --raw-self-test --adapter-index 0 --domain 0 --vlan --vlan-id 100 --vlan-pcp 4

# Start synthetic IEC 61850 lab profile traffic with VLAN tagging
dotnet run --project .\src\PtpLabClock.Console -- --adapter-index 0 --domain 0 --profile iec61850 --vlan --vlan-id 100 --vlan-pcp 4
```

Recommended Wireshark display filter:

```text
eth.type == 0x88f7 or ptp
```

Recommended capture filter for untagged, VLAN, and QinQ Layer-2 PTP:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

## Project structure

```text
src/PtpLabClock.App        WPF dashboard
src/PtpLabClock.Core       Engine, scheduler, monitor, health, diagnostics
src/PtpLabClock.Protocol   PTPv2 and Ethernet serialization/parsing
src/PtpLabClock.Pcap       SharpPcap/Npcap RAW Layer-2 transport
src/PtpLabClock.Config     JSON settings/profile helpers
src/PtpLabClock.Console    CLI validation, monitor, RAW self-test
src/PtpLabClock.Reporting  PDF/session evidence export
tests/                     xUnit regression tests
.github/workflows          CI, security, release, and GitHub Pages automation
```

## Documentation

Start with [`docs/index.md`](docs/index.md) for repository documentation and the SEO landing page at [`docs/index.html`](docs/index.html):

- [Quick start](docs/quick-start.md)
- [Installation](docs/installation.md)
- [RAW NIC mode](docs/raw-nic-mode.md)
- [Protocol validation](docs/protocol-validation.md)
- [Passive monitor](docs/passive-monitor.md)
- [Timing health validation](docs/health-validation.md)
- [Wireshark validation](docs/wireshark-validation.md)
- [Limitations](docs/limitations.md)
- [Development](docs/development.md)

## Open-source hygiene

- License: Apache-2.0.
- Security policy: [`SECURITY.md`](SECURITY.md).
- Contribution guide: [`CONTRIBUTING.md`](CONTRIBUTING.md).
- Third-party notices: [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).
- Clean-room rule: do not copy incompatible or proprietary source code, UI assets, packet fixtures, or documentation text.

## Search terms this project intentionally serves

`PTP lab clock`, `IEC 61850 PTP monitor`, `IEC 61850-9-3 lab`, `PTPv2 Layer 2 analyzer`, `Process Bus timing monitor`, `Sampled Values timing`, `GOOSE timing context`, `VLAN PTP`, `QinQ PTP`, `Npcap PTP`, `SharpPcap raw Ethernet`, `FAT SAT commissioning PTP`.

## Limitations

This project does not provide hardware timestamping, clock servo discipline, BMCA-complete grandmaster behavior, conformance certification, or relay-acceptance timing guarantees. Use certified PTP grandmaster equipment for protection, metering, and final commissioning workflows.
