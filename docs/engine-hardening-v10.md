# Engine Hardening v10 Notes

This patch moves the engine from UI-demo grade toward Wireshark-validation grade.

## Implemented

- Thread-safe `SequenceIdManager` using a private lock.
- Single TX queue in `PtpMasterEngine` using `System.Threading.Channels`.
- One TX sender loop calls the transport, avoiding concurrent `Npcap.SendPacket` calls from timers/RX callbacks.
- Cleaner stop lifecycle using cancellation + `Task.WhenAll` for Announce, Sync, and TX sender tasks.
- Shared timestamp pairs:
  - `Sync` and its `Follow_Up` use the same captured timestamp.
  - `Pdelay_Resp` and `Pdelay_Resp_Follow_Up` use the same response timestamp.
- Stronger Pdelay parser validation:
  - PTP EtherType detection with untagged, single VLAN, and double-tagged frames.
  - PTP version check.
  - message length check.
  - domain mismatch rejection.
- VLAN-aware Npcap BPF filter:
  - `ether proto 0x88f7`
  - `vlan and ether proto 0x88f7`
  - `vlan and vlan and ether proto 0x88f7`
- Adapter sorting favors likely physical Ethernet adapters and pushes loopback/WAN/VPN/virtual adapters lower.
- Runtime counters now track last sequence IDs and last peer clock identity for future monitor views.

## Still Not Implemented

- Hardware timestamping.
- OS clock adjustment.
- BMCA.
- Clock servo.
- Boundary/transparent clock behavior.
- True timing accuracy validation.

## Smoke Test Target

1. Demo Mode starts/stops repeatedly without UI lockup.
2. RAW Mode opens selected adapter and sends PTP Layer-2 frames.
3. Wireshark filter: `eth.type == 0x88f7`.
4. Announce, Sync, Follow_Up should be visible.
5. If a Pdelay_Req is seen, the app should respond with Pdelay_Resp and Pdelay_Resp_Follow_Up using the same sequence ID.
6. Event timeline should show TX/RX messages with sequence IDs.

## Next Recommended Step

Add protocol unit tests for byte offsets, message lengths, flags, sequence IDs, and timestamp pairing. Then add a passive monitor parser for Announce/Sync/Follow_Up.
