# Wireshark Validation

Use this filter for Layer-2 PTP:

```text
eth.type == 0x88f7
```

Expected MVP messages:

- Announce
- Sync
- Follow_Up
- Pdelay_Resp / Pdelay_Resp_Follow_Up when an IED/analyzer sends Pdelay_Req

Validation checklist:

1. Destination MAC for general messages: `01:1b:19:00:00:00`.
2. Destination MAC for peer-delay messages: `01:80:c2:00:00:0e`.
3. EtherType: `0x88f7`.
4. PTP version: 2.
5. Domain matches UI.
6. SourcePortIdentity remains stable unless GM Switch scenario is activated.
7. Sequence IDs increment.
8. Sync and Follow_Up sequence IDs match.
9. Fault scenarios appear in event timeline and in packet behavior.
