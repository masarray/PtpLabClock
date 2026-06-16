# RAW Npcap Transport

`PtpLabClock.Pcap` contains the Layer-2 packet capture/injection implementation used by RAW mode. It uses SharpPcap as the .NET binding and expects Npcap/libpcap support to be available at runtime.

## Current state

- Adapter discovery uses `CaptureDeviceList.Instance` from SharpPcap.
- Adapter IDs encode the actual pcap device name, so the selected UI adapter maps back to the same live device at start.
- Adapter discovery now exposes MAC address, pcap name, up/down status, Wi-Fi/virtual hints, and a user-facing diagnostic summary.
- Selecting a real adapter automatically derives the default Source MAC and EUI-64 style Clock Identity from the NIC MAC when available.
- `NpcapPtpTransport` opens the selected device with promiscuous + immediate mode and a short read timeout.
- RX capture uses a VLAN-aware PTP capture filter for untagged, single-tagged VLAN, and common QinQ traffic.
- TX uses `SendPacket(frame)` with serialized Ethernet frames from `PtpLabClock.Protocol`.
- The WPF app includes a **RAW Self Test** action that opens the adapter, applies the filter, sends one Announce frame, and reports whether local capture observed it.
- Demo Mode and protocol validation remain independent from raw packet access.

## Windows runtime requirements

1. Install Npcap.
2. Run the WPF app or CLI as Administrator when Npcap is configured to restrict capture to administrators.
3. Prefer a wired Ethernet adapter. Wi-Fi, VPN, virtual, and USB tethering adapters may capture but reject raw injection.
4. Confirm Wireshark can capture on the same adapter.

## Recommended Wireshark filters

Display filter:

```text
eth.type == 0x88f7 or ptp
```

Capture filter for untagged, VLAN, and QinQ PTP:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

## Known limitations

- Software timestamping only; this is not a certified timing source.
- Local self-capture of injected packets is driver-dependent. A self-test may send successfully but not observe the outbound packet locally; confirm with Wireshark or a second analyzer port.
- Some NIC drivers silently drop injected multicast Layer-2 frames; this is a driver/adapter limitation, not a serializer issue.
- VLAN TX support exists in the protocol/engine layer, but the current WPF surface does not yet expose full VLAN controls.

## Next hardening checklist

1. Add WPF VLAN controls: Enable VLAN, VID, PCP.
2. Add a CLI `--raw-self-test` command for headless validation.
3. Add optional PCAP replay mode for deterministic analyzer regression.
4. Add adapter capability notes to release pages.
5. Add richer PTP profile presets when exact deployment requirements are known.
