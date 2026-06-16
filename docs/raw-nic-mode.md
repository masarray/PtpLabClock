# RAW NIC Mode

RAW mode sends and captures Layer-2 PTP frames through SharpPcap/Npcap.

## What RAW mode does

- Lists live Npcap adapters.
- Opens the selected adapter in promiscuous/immediate mode.
- Applies a VLAN-aware PTP capture filter.
- Sends untagged or VLAN-tagged Ethernet frames with EtherType `0x88F7`.
- Captures PTP frames for monitor and health validation.

## Capture filter

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

## CLI examples

```powershell
# List adapters
PtpLabClock.Console.exe --list

# Untagged self-test
PtpLabClock.Console.exe --raw-self-test --adapter-index 0 --domain 0

# VLAN-tagged self-test
PtpLabClock.Console.exe --raw-self-test --adapter-index 0 --domain 0 --vlan --vlan-id 100 --vlan-pcp 4

# Start synthetic traffic with VLAN tagging
PtpLabClock.Console.exe --adapter-index 0 --domain 0 --profile iec61850 --vlan --vlan-id 100 --vlan-pcp 4
```

## Troubleshooting

| Symptom | Likely cause | Action |
|---|---|---|
| No adapters listed | Npcap missing, service blocked, or permission issue | Install Npcap, restart, run as Administrator |
| Adapter opens but send fails | Adapter/driver blocks injection | Use wired Ethernet; avoid Wi-Fi/VPN/virtual adapters |
| Send succeeds but local capture is not observed | Driver does not loop back outbound injected packets | Verify externally in Wireshark or on a tap/SPAN port |
| VLAN frames not visible | Switch port or capture filter mismatch | Check VLAN ID/PCP and Wireshark capture filter |

## Safety boundary

RAW mode is for controlled lab networks. It is not a certified timing source and does not provide hardware timestamp accuracy.
