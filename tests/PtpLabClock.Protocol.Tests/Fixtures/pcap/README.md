<!-- SPDX-License-Identifier: Apache-2.0 -->
# Golden PCAP Fixtures

These deterministic PCAP files are generated specifically for Process Bus Timing Lab regression tests and documentation. They are not captured from vendor equipment and contain no proprietary traffic.

## Fixtures

| File | Purpose |
|---|---|
| `ptp-announce-untagged.pcap` | Untagged PTPv2 Announce frame. |
| `ptp-sync-followup-vlan.pcap` | VLAN-tagged two-step Sync and Follow_Up pair. |
| `ptp-pdelay-qinq.pcap` | QinQ Pdelay_Req visibility sample. |
| `ptp-mixed-process-bus-golden.pcap` | Mixed untagged, VLAN, and QinQ process-bus PTP sample. |

Use these files for analyzer smoke checks, documentation examples, and regression tests. They are intentionally small so reviewers can inspect them with Wireshark or a byte-level parser.
