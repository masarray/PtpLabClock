# Process Bus Timing Lab — Product Roadmap

## Vision

Build a Windows-friendly lab instrument for IEC 61850 process bus timing work: simulate, monitor, validate, and document PTP behavior while staying honest about software-only timing limits.

## Product modules

1. **PTP Lab Clock** — synthetic master-like simulator for Announce, Sync, Follow_Up, and Pdelay response.
2. **PTP Monitor** — passive detector for GM/domain/profile/message health.
3. **Scenario Player** — controlled fault injection for analyzer validation.
4. **Timing Report** — engineering evidence pack for lab/FAT/customer discussion.
5. **Process Bus Analyzer Integration** — correlate PTP timing health with SV and GOOSE workflows.

## Current baseline

- Apache-2.0 license direction.
- Buildable solution layout, including restored `PtpLabClock.Pcap` boundary.
- WPF dashboard shell and Demo Mode.
- Layer-2 PTP frame builder.
- Announce / Sync / Follow_Up / Pdelay response generation.
- Passive monitor and timing health validator.
- Protocol regression tests.
- GitHub Actions CI and release packaging.
- Internal PDF/session evidence export without external PDF package dependency.

## Next engineering milestones

1. Run CI on GitHub and fix any environment-specific build issue.
2. Expand byte-level tests for VLAN/QinQ and malformed PTP frames.
3. Harden RAW Npcap transport with VLAN-aware filters, self-test injection, and better adapter diagnostics.
4. Add adapter capability display: MAC, medium type, link status, admin hint.
5. Improve per-source sequence tracking by message family.
6. Add Sync/Follow_Up pairing health checks by source and sequence ID.
7. Add Pdelay request/response/follow-up peer table.
8. Add profile guess and mismatch warnings.
9. Add compact UI screenshots and GitHub release assets.
10. Integrate PTP health gate concept with SV Injector/Process Bus Analyzer.

## Non-goals

- Do not adjust OS time.
- Do not claim nanosecond accuracy.
- Do not emulate full servo/BMCA/hardware timestamping in MVP.
- Do not bundle Npcap installer without redistribution review.
- Do not position software-only generated traffic as a certified timing source.
