# Wireshark Validation Mode v11

This phase keeps the visual design locked and adds engineering verification around the packet engine.

## Console protocol validation

Run without Npcap or administrator rights:

```powershell
cd src\PtpLabClock.Console
dotnet run -- --validate-protocol --domain 0
```

Expected output includes PASS rows for:

- Announce
- Sync
- Follow_Up
- Pdelay_Resp
- Pdelay_Resp_Follow_Up

Each row prints message type, sequence ID, domain, PTP message length, PTP offset, and source clock identity.

## RAW packet validation

Run the WPF app or console in RAW mode. Then use this Wireshark display filter:

```text
eth.type == 0x88f7
```

For VLAN process-bus networks, capture filter support is VLAN-aware in the Npcap transport:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

## Engine log additions

Every TX frame is now inspected before sending. The event log appends a bracketed summary:

```text
TX SYNC Sync seq=12. [Layer-2, len=44, offset=14]
```

Malformed TX frames are skipped and logged as `TXCHK` errors.

## Scope reminder

The tool remains a synthetic lab simulator for analyzer validation and quick visibility checks. It is not a GPS/PTP grandmaster replacement and is not a timing accuracy acceptance source.
