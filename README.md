# Process Bus Timing Lab

**Current module:** PTP Lab Clock Simulator  
**Internal solution name:** `PtpLabClock`  
**License:** GPL-3.0-or-later

Process Bus Timing Lab is a Windows-friendly lab tool for IEC 61850 process bus timing work. The first module, **PTP Lab Clock Simulator**, generates synthetic PTPv2 Layer-2 traffic and controlled timing scenarios so a Process Bus Analyzer can be validated without requiring a full GPS/PTP grandmaster setup for early tests.

It is designed for:

- PTP visibility checks.
- Process Bus Analyzer parser validation.
- Announce / Sync / Follow_Up flow testing.
- Pdelay response simulation.
- Domain/profile behavior checks.
- Controlled fault scenarios such as GM Lost, Missing Follow_Up, Clock Degraded, Sequence Jump, and Stop Pdelay.

It is **not** a certified timing source and must not be used as a GPS/PTP grandmaster replacement.

## Build Requirements

- Windows 10/11.
- Visual Studio 2022/2026 with .NET desktop development workload.
- .NET 8 SDK.
- Npcap installed for RAW packet mode.
- Run Visual Studio/app as Administrator for RAW packet injection/capture.

Demo Mode does not require Npcap and should always run.

## Quick Start

1. Open `PtpLabClock.sln`.
2. Set startup project to `PtpLabClock.App`.
3. Build solution.
4. Run app.
5. Select **Demo Engine** first and click **Start Engine**.
6. Confirm counters and event timeline move.
7. For real packet mode, select an Ethernet adapter and validate with Wireshark:

```text
eth.type == 0x88f7
```

## Current Project Structure

```text
src/PtpLabClock.App       WPF manager UI
src/PtpLabClock.Core      Engine, scheduler, scenarios, diagnostics
src/PtpLabClock.Protocol  PTPv2 and Ethernet packet builder/parser
src/PtpLabClock.Pcap      Npcap/SharpPcap transport
src/PtpLabClock.Config    JSON config helpers
src/PtpLabClock.Console   CLI smoke runner
```

## Current MVP Features

- Modern WPF dashboard.
- Demo Mode fallback.
- RAW Npcap transport structure.
- PTP Layer-2 EtherType `0x88F7` packet generation.
- Announce / Sync / Follow_Up TX.
- Pdelay_Req listener and Pdelay response skeleton.
- Runtime counters.
- Event timeline.
- Scenario buttons.
- GPL-3.0-or-later license.

## Product Roadmap

See:

- `AGENTS.md` — coding direction and roadmap for future agents.
- `ROADMAP.md` — product roadmap.
- `docs/implementation-structure.md` — architecture and implementation boundaries.
- `docs/global-reference-notes.md` — clean-room reference study summary.
- `docs/safety-scope.md` — safety and product-claim boundaries.

## Licensing

This project is licensed under **GPL-3.0-or-later**. See `LICENSE`.

Do not copy source code from linuxptp, PTPSync, Meinberg tools, or other references into this repository unless a license review is completed. This project follows a clean-room implementation approach.

Npcap is required for RAW packet mode but should not be bundled by default. Ask users to install Npcap separately unless redistribution rights are confirmed.
