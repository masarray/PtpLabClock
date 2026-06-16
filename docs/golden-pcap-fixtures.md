<!-- SPDX-License-Identifier: Apache-2.0 -->
# Golden PCAP Fixtures

Process Bus Timing Lab includes deterministic PCAP fixtures for protocol regression and analyzer smoke checks. These files are generated for the project and are not copied from vendor tools, relay captures, or proprietary test sets.

Fixture location:

```text
tests/PtpLabClock.Protocol.Tests/Fixtures/pcap/
```

## Included fixtures

| Fixture | Transport | Expected frames | Purpose |
|---|---|---:|---|
| `ptp-announce-untagged.pcap` | Untagged Layer-2 PTP | 1 | Validate basic PTP EtherType `0x88F7` and Announce decode. |
| `ptp-sync-followup-vlan.pcap` | IEEE 802.1Q VLAN | 2 | Validate two-step Sync / Follow_Up pairing visibility on process-bus VLAN. |
| `ptp-pdelay-qinq.pcap` | QinQ / stacked VLAN | 1 | Validate nested VLAN offset handling for Pdelay visibility. |
| `ptp-mixed-process-bus-golden.pcap` | Mixed | 4 | Compact smoke fixture for analyzer and regression tests. |

## Recommended analyzer checks

Wireshark display filter:

```text
eth.type == 0x88f7 or ptp
```

Expected message sequence for the mixed fixture:

```text
Announce
Sync
Follow_Up
Pdelay_Req
```

## Regression coverage

The xUnit test `PtpGoldenPcapFixtureTests` loads each PCAP file, extracts Ethernet frames, and validates:

- PCAP header integrity.
- Frame count.
- PTPv2 version.
- Domain number.
- Message type.
- Transport detection: untagged, VLAN, QinQ.
- Common header readability through `PtpFrameInspector`.

These fixtures are intentionally small so reviewers can inspect the bytes, compare analyzer output, and use them as stable protocol smoke tests.
