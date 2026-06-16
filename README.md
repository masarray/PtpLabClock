# Process Bus Timing Lab

**Current module:** PTP Lab Clock Simulator / PTP Monitor  
**Solution:** `PtpLabClock`  
**License:** Apache-2.0

Process Bus Timing Lab is a Windows-friendly lab and diagnostic tool for IEC 61850 process bus timing work. The current module focuses on PTPv2 Layer-2 visibility, synthetic message generation, passive health monitoring, scenario playback, and evidence export for analyzer validation.

It is designed for engineers who need to check whether PTP traffic is visible, whether an analyzer decodes Announce / Sync / Follow_Up / Pdelay messages correctly, and whether a lab network has basic timing-health symptoms worth investigating.

It is **not** a certified timing source and must not be represented as a replacement for a GPS/PTP grandmaster, hardware timestamping NIC, or final relay acceptance timing setup.

## What is in this repository

- WPF desktop dashboard with Demo Mode fallback.
- Protocol serializer for PTPv2 Layer-2 frames using EtherType `0x88F7`.
- Announce, Sync, Follow_Up, Pdelay_Resp, and Pdelay_Resp_Follow_Up builders.
- Passive PTP monitor and timing health validator.
- Scenario hooks for GM lost, missing Follow_Up, clock degraded, sequence jump, GM switch, and stop Pdelay.
- PCAP session writer for generated/captured evidence.
- Lightweight Apache-2.0 internal PDF report generator.
- xUnit protocol regression tests.
- GitHub Actions build/test workflow and release workflow.

## Important RAW mode note

`src/PtpLabClock.Pcap` now contains a SharpPcap-backed RAW transport. The repository does **not** bundle the Npcap installer or driver; users must install Npcap separately on Windows.

Current behavior:

- **Demo Mode** works without Npcap.
- **Protocol validation** works without Npcap.
- **RAW packet mode** uses Npcap/SharpPcap to enumerate live adapters, open the selected adapter in promiscuous/immediate mode, capture PTP EtherType `0x88F7`, and inject Layer-2 Ethernet frames with `SendPacket`.

This keeps the repository Apache-2.0 while making real NIC mode operational. Npcap remains an external runtime dependency.

## Requirements

- Windows 10/11 for the WPF app.
- .NET 8 SDK.
- Visual Studio 2022 or newer with .NET desktop development workload.
- Optional for RAW mode: Npcap and elevated process privileges.

## Quick start

```powershell
# Restore, build, and test
dotnet restore .\PtpLabClock.sln
dotnet build .\PtpLabClock.sln -c Release --no-restore
dotnet test .\PtpLabClock.sln -c Release --no-build

# Run protocol byte-layout validation without RAW packet access
dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0

# Run the WPF app
dotnet run --project .\src\PtpLabClock.App
```

Inside the app, select **Demo Engine** first and click **Start Engine** to validate the UI. For real traffic, run the app as Administrator, select a wired RAW adapter, then validate packets in Wireshark with `eth.type == 0x88f7`.

Recommended Wireshark filter for external validation:

```text
eth.type == 0x88f7
```

## Project structure

```text
src/PtpLabClock.App        WPF manager UI
src/PtpLabClock.Core       Engine, scheduler, monitor, health, diagnostics
src/PtpLabClock.Protocol   PTPv2 and Ethernet packet builder/parser
src/PtpLabClock.Pcap       SharpPcap/Npcap RAW Layer-2 transport
src/PtpLabClock.Config     JSON settings/profile helpers
src/PtpLabClock.Console    CLI smoke runner and protocol validator
src/PtpLabClock.Reporting  PDF/session evidence export
tests/                     Protocol and monitor regression tests
.github/workflows          CI and release automation
```

## Build scripts

```powershell
.\tools\scripts\validate-layout.ps1
.\tools\scripts\build.ps1
```

## Documentation

- `docs/quick-start.md` — user-facing setup and validation path.
- `docs/ptp-scope-and-limitations.md` — what this tool can and cannot prove.
- `docs/raw-npcap-roadmap.md` — RAW Npcap setup, limitations, and troubleshooting.
- `docs/wireshark-validation.md` — packet validation hints.
- `ROADMAP.md` — engineering roadmap.
- `AGENTS.md` — coding and architecture direction.

## License

This repository is licensed under **Apache License 2.0**. See `LICENSE`.

Clean-room rule: do not copy source code from linuxptp, Meinberg tools, PTPSync, Npcap samples, SharpPcap examples, or other references into this repository unless a license review explicitly permits it. Study behavior and public protocol documentation only; implement independently.
