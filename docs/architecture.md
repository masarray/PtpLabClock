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

Sends and receives Layer-2 Ethernet frames using Npcap/SharpPcap. Requires Npcap, admin rights, and a suitable adapter.

## Future Runtime Modes

- Passive monitor mode.
- PCAP replay mode.
- UDPv4/UDPv6 PTP mode.
- Remote linuxptp controller mode.
- Windows Service mode.
