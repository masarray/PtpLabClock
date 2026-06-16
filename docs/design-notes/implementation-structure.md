# Implementation Structure — Process Bus Timing Lab

This document turns the global reference study into a practical implementation structure.

## Why this structure

Existing high-quality PTP tools show three useful patterns:

- PTPSync: Windows service + manager UI + adapter/config workflow.
- linuxptp: separate clock, port, transport, message, profile, scheduler, and timestamp/servo concerns.
- Meinberg/Track Hound style tools: readable status, monitoring, troubleshooting, and traffic visibility.

Process Bus Timing Lab uses those patterns, but stays clean-room and lab-focused.

## Current Implementation Layers

```text
PtpLabClock.App
  Manager UI. No packet logic.

PtpLabClock.Core
  Runtime engine. Owns Start/Stop, counters, scenarios, scheduler, and Pdelay responder.

PtpLabClock.Protocol
  PTPv2 and Ethernet byte-level serialization.

PtpLabClock.Pcap
  RAW transport boundary; no bundled packet wrapper in the current Apache-2.0 source package.

PtpLabClock.Config
  JSON settings and future profiles.

PtpLabClock.Console
  CLI validation runner.
```

## Target Refactor

The current MVP intentionally has a compact engine. As soon as the UI build is stable, split engine responsibilities:

```text
Core/Engine/PtpMasterEngine.cs
  orchestration only

Core/Ports/PtpPortRuntime.cs
  selected port state, domain, clock identity, profile settings

Core/Scheduling/AnnounceScheduler.cs
Core/Scheduling/SyncScheduler.cs
  periodic send loops

Core/Pdelay/PdelayResponder.cs
  parse Pdelay_Req, send Pdelay_Resp, send Pdelay_Resp_Follow_Up

Core/Monitor/PassivePtpMonitor.cs
  future: observe existing PTP network

Core/Diagnostics/PtpHealthEvaluator.cs
  convert counters/events into PASS/WARNING/FAIL cards
```

## Protocol Implementation Rules

- Use Layer-2 first for process bus: EtherType `0x88F7`.
- Default multicast targets:
  - General PTP: `01-1B-19-00-00-00`
  - Peer delay: `01-80-C2-00-00-0E`
- Keep UDPv4/UDPv6 as future transport, not MVP default.
- Serialize explicitly in big-endian.
- Every message builder must be verified against Wireshark decode.
- Every parser must tolerate malformed frames without crashing the engine.

## UI Implementation Rules

- Demo Mode remains always available.
- RAW Mode must fail gracefully.
- Main window must launch even if Npcap is not installed.
- Event timeline is the first troubleshooting surface.
- Avoid deep settings in the main layout; use expandable advanced settings later.

## Validation Method

1. Start Demo Mode: counters must move and scenarios must log.
2. Start RAW Mode with admin and Npcap.
3. Confirm Wireshark filter: `eth.type == 0x88f7`.
4. Verify decoded message types: Announce, Sync, Follow_Up.
5. Attach IED/analyzer and check Pdelay_Req RX / Pdelay_Resp TX.
6. Run fault scenarios and verify Process Bus Analyzer event response.
