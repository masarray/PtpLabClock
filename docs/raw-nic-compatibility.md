<!-- SPDX-License-Identifier: Apache-2.0 -->
# RAW NIC Compatibility Matrix

RAW mode depends on the selected Windows adapter, driver, Npcap installation, security policy, and whether the NIC/driver allows packet injection and local capture. This matrix is the engineering evidence log for real adapter behavior.

## How to test an adapter

Run the app or CLI as Administrator when required by local policy.

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --list

dotnet run --project .\src\PtpLabClock.Console -- \
  --raw-self-test --adapter-index 0 --domain 0

dotnet run --project .\src\PtpLabClock.Console -- \
  --raw-self-test --adapter-index 0 --domain 0 \
  --vlan --vlan-id 100 --vlan-pcp 4
```

Use Wireshark on the same adapter or a mirror/TAP port:

```text
eth.type == 0x88f7 or ptp
```

Capture filter for untagged, VLAN, and QinQ Layer-2 PTP:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

## Compatibility table

| Date | Windows | Npcap | Adapter / Driver | Adapter type | Open | Send | Local capture | External capture | VLAN | QinQ | Notes |
|---|---|---|---|---|---:|---:|---:|---:|---:|---:|---|
| _pending_ | _pending_ | _pending_ | _pending_ | Wired / USB / Wi-Fi / VPN / Virtual |  |  |  |  |  |  | Add real lab result. |

## Result definitions

| Field | Meaning |
|---|---|
| Open | Npcap exposes the adapter and `Open()` succeeds. |
| Send | `SendPacket()` returns without exception. |
| Local capture | The same Npcap session observes the transmitted self-test frame. Some drivers do not loop back injected frames even when transmit works. |
| External capture | Wireshark or an analyzer on another port sees the packet. This is stronger evidence than local capture. |
| VLAN | VLAN-tagged self-test frame is seen and decoded. |
| QinQ | Stacked VLAN PTP frame is seen and decoded. |

## Known limitations

- A successful local self-test does not prove timing accuracy.
- A failed local self-capture does not always prove TX failure; some adapters transmit but do not echo injected packets back to the local capture path.
- Wireless, VPN, loopback, and virtual adapters are not preferred for process-bus lab publication.
- Relay acceptance still requires a valid timing architecture and appropriate certified timing source.
