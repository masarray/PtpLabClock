# Architecture

Process Bus Timing Lab is structured as a simulator-first, monitor-ready WPF application.

## Architecture Goals

- UI must launch without Npcap by using Demo Mode.
- RAW packet access must be isolated inside `PtpLabClock.Pcap`.
- PTP protocol bytes must be isolated inside `PtpLabClock.Protocol`.
- Engine orchestration must be isolated inside `PtpLabClock.Core`.
- WPF ViewModels must not know about Ethernet/PTP byte offsets.

## Runtime Flow

```text
WPF Command
  -> MainViewModel
  -> PtpMasterEngine
  -> IPtpTransport
  -> MockPtpTransport or NpcapPtpTransport
  -> Ethernet frame TX/RX
```

## Current Runtime Modes

### Demo Mode

No packets leave the machine. Useful for UI validation, training, and engine-loop smoke test.

### RAW Npcap Mode

Reserved boundary for Layer-2 Ethernet capture/injection. The current Apache-2.0 package does not bundle a packet driver wrapper; future real RAW support must stay isolated here after license review.

## Future Runtime Modes

- Passive monitor mode.
- PCAP replay mode.
- UDPv4/UDPv6 PTP mode.
- Remote linuxptp controller mode.
- Windows Service mode.
