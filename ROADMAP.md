# Process Bus Timing Lab — Product Roadmap

## Vision

Build a Windows-friendly lab instrument for IEC 61850 process bus timing work: simulate, monitor, validate, and document PTP behavior without requiring a full GPS grandmaster setup for every early test.

## Product Modules

1. **PTP Lab Clock** — synthetic master-like simulator for Announce, Sync, Follow_Up, and Pdelay response.
2. **PTP Monitor** — passive detector for GM/domain/profile/message health.
3. **Scenario Player** — controlled fault injection for analyzer validation.
4. **Timing Report** — engineering evidence pack for lab/FAT/customer discussion.
5. **Process Bus Analyzer Integration** — correlate PTP timing health with SV and GOOSE.

## MVP Done / Current

- WPF modern dashboard shell.
- Demo Mode.
- Npcap RAW Mode structure.
- Layer-2 PTP frame builder.
- Announce / Sync / Follow_Up generation.
- Pdelay response skeleton.
- Scenario buttons and event timeline.
- GPL-3.0-or-later licensing direction.

## Next 10 Engineering Milestones

1. Build error cleanup from real Visual Studio test.
2. Unit tests for PTP packet serializer.
3. Wireshark validation checklist with expected decoded fields.
4. Console packet smoke runner.
5. Per-peer Pdelay tracking.
6. Passive monitor mode.
7. Profile guess and mismatch warnings.
8. Health score cards.
9. Markdown report export.
10. Process Bus Analyzer PTP Timing tab integration.

## Non-Goals

- Do not adjust OS time.
- Do not claim nanosecond accuracy.
- Do not emulate full linuxptp servo/BMCA in MVP.
- Do not bundle Npcap installer without license review.
