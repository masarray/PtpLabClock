# RAW Npcap Transport

`PtpLabClock.Pcap` contains the Layer-2 packet capture/injection implementation used by RAW mode. It uses SharpPcap as the .NET binding and expects Npcap/libpcap support to be available at runtime.

## Current state

- Adapter discovery uses `CaptureDeviceList.Instance` from SharpPcap.
- Adapter IDs encode the actual pcap device name, so the selected UI adapter maps back to the same live device at start.
- `NpcapPtpTransport` opens the selected device with promiscuous + immediate mode and a short read timeout.
- RX capture is filtered to PTP EtherType `0x88F7`.
- TX uses `SendPacket(frame, frame.Length)` with serialized Ethernet frames from `PtpLabClock.Protocol`.
- Demo Mode and protocol validation remain independent from raw packet access.

## Windows runtime requirements

1. Install Npcap.
2. Run the WPF app or CLI as Administrator when Npcap is configured to restrict capture to administrators.
3. Prefer a wired Ethernet adapter. Wi-Fi, VPN, virtual, and USB tethering adapters may capture but reject raw injection.
4. Confirm Wireshark can capture on the same adapter.

## Recommended Wireshark display filter

```text
eth.type == 0x88f7
```

## Known limitations

- Software timestamping only; this is not a certified timing source.
- The current BPF filter targets the common untagged PTP case. VLAN/QinQ capture hardening is a next step.
- Some NIC drivers silently drop injected multicast Layer-2 frames; this is a driver/adapter limitation, not a serializer issue.

## Hardening checklist

1. Add a RAW self-test button: open adapter, send one low-rate Announce, confirm local capture/Wireshark visibility.
2. Add VLAN-aware filter option:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

3. Add adapter diagnostics: pcap name, friendly name, MAC, up/down, loopback/virtual hints.
4. Add operator warnings when selected adapter looks like Wi-Fi, VPN, Hyper-V, or loopback.
5. Add a release note that Npcap is external and not redistributed by this repository.
