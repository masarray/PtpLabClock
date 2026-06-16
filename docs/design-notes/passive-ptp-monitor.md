# Passive PTP Monitor Foundation v13

This phase adds the first passive observation layer for Process Bus Timing Lab.

## Goal

The application can now observe Layer-2 PTP traffic and build a live diagnostic snapshot without acting as the timing source.

## Added components

- `PtpPassiveMonitor`
- `PtpObservedMessage`
- `PtpSourceClockState`
- `PtpMonitorSnapshot`

The monitor uses `PtpFrameInspector` to decode common PTP header fields from raw Ethernet frames.

## Tracked fields

- message type
- domain number
- sequence ID
- source clock identity
- transport type: Layer-2, VLAN, QinQ
- detected source clocks
- detected domains
- message counters
- live/lost source state using a short 5-second observation window

## Console monitor mode

```powershell
dotnet run --project src\PtpLabClock.Console -- --monitor --adapter-index 0 --domain 0
```

Optional PCAP recording:

```powershell
dotnet run --project src\PtpLabClock.Console -- --monitor --adapter-index 0 --domain 0 --record-pcap captures\ptp-monitor.pcap
```

## Wireshark filter

```text
eth.type == 0x88f7
```

## Current scope

This is a foundation monitor, not yet a full compliance validator. It does not yet calculate precise rates, announce timeout per profile, BMCA, or profile-level pass/fail scoring.

## Next recommended phase

v14 should add Timing Health Validator Lite:

- PTP visibility PASS/WARN/FAIL
- multiple GM warning
- domain mismatch warning
- Follow_Up missing warning
- Pdelay activity state
- source live/lost health cards
