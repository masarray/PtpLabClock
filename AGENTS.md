# AGENTS.md — Process Bus Timing Lab Coding Direction

This file is the authoritative direction for future coding agents working on this repository.

## Product Identity

**Product name:** Process Bus Timing Lab  
**Current module name:** PTP Lab Clock Simulator / `PtpLabClock`  
**Target users:** IEC 61850 process bus engineers, FAT/SAT engineers, protection/automation test engineers, and developers validating SV/GOOSE/PTP analyzers.

The product is **not** a GPS/PTP grandmaster replacement. It is a Windows-friendly lab simulator and diagnostic companion for quick PTP visibility checks, analyzer validation, Pdelay response testing, and controlled fault scenarios.

## License Direction

The repository is licensed as **GPL-3.0-or-later**.

Rules:

1. Keep the `LICENSE` file as the full GPLv3 text.
2. Keep `Directory.Build.props` with `PackageLicenseExpression=GPL-3.0-or-later`.
3. Add SPDX headers to new source files:
   - C#: `// SPDX-License-Identifier: GPL-3.0-or-later`
   - XAML/XML: `<!-- SPDX-License-Identifier: GPL-3.0-or-later -->`
4. Do not copy source code from linuxptp, Meinberg, or other references unless a license review explicitly permits it.
5. Do not bundle Npcap installer by default. Link to Npcap install instructions instead.
6. Maintain `THIRD-PARTY-NOTICES.md` whenever dependencies change.

## Clean-Room Reference Model

We may study public behavior and architecture, but implementation must remain our own.

### PTPSync lessons to keep

- Manager UI controls an engine/service.
- Clear adapter selection and configuration workflow.
- Service mode can be added later after the engine is stable.
- Logs and status must be visible to non-developer engineers.

### linuxptp lessons to keep

- Separate clock, port, transport, message serialization, scheduler, profile, and diagnostics.
- Transport must be abstracted so Layer-2, UDP, demo, and future replay modes do not leak into UI.
- Hardware timestamping, PHC, clock servo, BMCA, boundary clock, and transparent clock are **future/non-MVP** features.
- Current app must not adjust OS time.

### Meinberg / Track Hound lessons to keep

- Engineer needs visibility: detected GM, domain, profile, message rates, master change, interval issues.
- Status views must be readable at a glance.
- Diagnostic timeline is as important as raw packet counters.

## Architecture Boundary Rules

Never put packet/protocol logic inside WPF views or ViewModels.

Recommended responsibilities:

```text
PtpLabClock.App
  WPF manager UI only: commands, binding, visual status, event presentation.

PtpLabClock.Core
  Engine orchestration, scheduler, counters, scenarios, diagnostics, runtime state.

PtpLabClock.Protocol
  PTP header/body serialization, Ethernet frame builder, parser/validator.

PtpLabClock.Pcap
  Npcap/SharpPcap adapter discovery, capture, injection, BPF filter handling.

PtpLabClock.Config
  JSON profile/settings storage only.

PtpLabClock.Console
  CLI smoke tests and Wireshark validation runner.
```

## Implementation Roadmap

### Phase 0 — Repository hygiene and safety direction

Status: active.

- GPL-3.0-or-later license.
- Clean-room policy.
- Safety disclaimer.
- Demo Mode must always work without Npcap.
- RAW Mode must clearly say Npcap/admin/NIC requirements.

### Phase 1 — Protocol correctness first

Goal: Wireshark-decodable PTPv2 packets.

Tasks:

- Split serializer into explicit message classes: `PtpHeader`, `AnnounceBody`, `SyncBody`, `FollowUpBody`, `PdelayRespBody`.
- Add unit tests for byte offsets, messageType, versionPTP, messageLength, domain, flags, correctionField, sourcePortIdentity, sequenceId, controlField, logMessageInterval.
- Validate Ethernet frame layout: destination MAC, source MAC, EtherType `0x88F7`, payload length.
- Add PCAP sample export from Console mode.

Do not add fancy features until Wireshark validation is reliable.

### Phase 2 — RAW Layer-2 engine hardening

Goal: stable raw Ethernet TX/RX on Windows.

Tasks:

- Harden `NpcapPtpTransport` error messages.
- Add adapter capability display: description, MAC address if available, status hint.
- Use VLAN-aware BPF filter `ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)` by default.
- Add clear failure hints: not admin, Npcap missing, adapter cannot transmit, selected Wi-Fi/VPN adapter unsuitable.
- Keep Console runner for smoke tests.

### Phase 3 — Pdelay responder realism

Goal: quick IED response check and analyzer Pdelay visibility.

Tasks:

- Parse incoming `Pdelay_Req` robustly.
- Respond with `Pdelay_Resp` and `Pdelay_Resp_Follow_Up` using matching sequenceId and requestingPortIdentity.
- Track per-peer Pdelay counters.
- Add peer table: `ClockIdentity`, last seen, sequence, request rate, response count.
- Add scenario: stop Pdelay reply, delayed Pdelay reply, wrong sequence.

### Phase 4 — Passive PTP Monitor

Goal: product becomes diagnostic companion, not only simulator.

Tasks:

- Passive monitor mode for existing PTP networks.
- Detected grandmaster list.
- Domain list.
- Message rates and interval checker.
- Follow_Up pairing checker.
- Multiple GM warning.
- GM switch event detection.
- Profile guess: Generic / IEC 61850-9-3-like / C37.238-like / Unknown.

### Phase 5 — Validator and Scenario Player

Goal: analyzer-grade diagnostic states.

Tasks:

- Add health cards: Visibility, Profile, Stability, Pdelay, Analyzer Readiness.
- Add scenario timeline player with start/stop/restore.
- Scenarios: GM Lost, Wrong Domain, Missing Follow_Up, Clock Degraded, Sequence Jump, GM Switch, Sync Jitter, Multiple GM conflict.
- Each scenario must emit event-log entries suitable for report export.

### Phase 6 — Reports and evidence pack

Goal: FAT/lab/customer communication.

Tasks:

- Export Markdown first, then DOCX/PDF later.
- Include selected adapter, profile settings, domain, GM identity, scenario actions, counters, warnings, and timeline.
- Keep report wording conservative: lab simulator, not timing accuracy validation.

### Phase 7 — Process Bus Analyzer integration

Goal: become part of a full Process Bus engineering suite.

Tasks:

- Add PTP Timing tab in Process Bus Analyzer.
- Correlate PTP health with SV stream stale/live and GOOSE event timeline.
- Unified event journal: PTP lost → SV timestamp warning → GOOSE/SV observation.

## UI/UX Direction

Use a modern web-dashboard feel, not default WPF.

Rules:

- Calm bright premium background.
- Segmented pill navigation/profile controls.
- Smooth hover/press states; buttons should feel tactile but not chaotic.
- Typography: regular/medium, clear hierarchy, not bulky.
- Use numbers boldly only where they are key counters.
- Do not overload the main screen with raw fields.
- Main hierarchy:
  1. status and mode,
  2. profile/domain/GM identity,
  3. live flow counters,
  4. scenarios,
  5. event timeline.

## Safety and Product Claims

Allowed product claims:

- Synthetic PTP traffic generator.
- Lab clock simulator.
- Analyzer validation companion.
- Quick sync visibility check.
- Pdelay response simulator.
- PTP diagnostic monitor.

Forbidden product claims unless future certified implementation exists:

- GPS grandmaster replacement.
- Certified timing source.
- Nanosecond accuracy validation.
- Protection-class time reference.
- FAT/SAT official grandmaster.

## Coding Style

- Prefer small, readable classes.
- Avoid heavy frameworks unless truly needed.
- No Prism/MVVM framework until the UI becomes too large.
- Keep ViewModels simple and testable.
- Use async cancellation correctly.
- Never block UI thread during capture/transmit.
- Prefer explicit error logs over swallowed exceptions.
- Demo Mode must not break when RAW Mode fails.

## Immediate Next Patch After This Version

1. Fix any Visual Studio build errors reported by user.
2. Add protocol unit tests for byte offsets, message lengths, flags, sequence IDs, and timestamp pairing.
3. Add Console/Wireshark validation runner that prints selected adapter + TX sequence IDs.
4. Add passive parser for Announce, Sync, and Follow_Up.
5. Add peer table for Pdelay requesters and detected grandmasters.
6. Start Passive Monitor mode.


## v11 Engine Validation Direction

Visual design is locked unless a change is strictly required for usability. New work must focus on engine verification:

- Run `dotnet run -- --validate-protocol --domain 0` from `src/PtpLabClock.Console` after packet builder changes.
- Keep TX frames Wireshark-verifiable with `eth.type == 0x88f7`.
- Do not bypass `PtpFrameInspector` for outbound frame sanity checks.
- Event logs should help correlate engine TX/RX sequence IDs with Wireshark captures.
- Any future UDP or VLAN feature must preserve the Layer-2 validation baseline first.

## v12 Engine Direction - PCAP Evidence

- Visual design is locked unless a change directly supports test evidence visibility.
- Prefer byte-level protocol validation and Wireshark-verifiable outputs over new visual features.
- PCAP files must be classic Ethernet PCAP unless pcapng metadata becomes necessary.
- Keep recorder code dependency-free and simple; do not introduce heavy capture frameworks beyond Npcap/SharpPcap already used by transport.
- Use `--validate-protocol --export-pcap` for deterministic builder evidence before live NIC testing.
- Use `--record-pcap` for live TX/RX evidence capture during raw adapter tests.


## v13 Passive Monitor Direction

Visual design is locked. Do not redesign the UI unless a new data surface is strictly required.

Engine direction after v13:
- Passive monitor must decode RX PTP frames using `PtpFrameInspector`.
- Track detected domains, message types, source clock identities, sequence IDs, and last-seen timestamps.
- Console monitor mode is the first validation surface.
- Do not claim compliance or timing accuracy from passive monitoring alone.
- Next phase should convert monitor snapshots into Timing Health Validator Lite statuses.

## v14 Timing Health Validator Lite

- Visual design remains locked.
- Health diagnostics must be derived from passive monitor snapshots.
- Use PASS / WARN / FAIL wording for engineering readability.
- Do not claim timing accuracy validation.
- Initial health checks are PTP Visibility, Domain Match, GM Stability, Follow_Up Pairing, Pdelay Activity, Sequence Continuity, and Analyzer Readiness.
- Console `--health` mode is the reference path for validator testing.

## v15 Health Dashboard UI Integration

- Visual design remains locked.
- WPF dashboard now surfaces Timing Health Validator results as compact diagnostic cards.
- Health UI is driven by `PtpMasterEngine.MonitorSnapshotUpdated` and `PtpTimingHealthValidator`.
- Do not add major visual redesign in health-card work; keep it compact and diagnostic-oriented.
