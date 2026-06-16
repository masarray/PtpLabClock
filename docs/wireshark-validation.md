# Wireshark Validation

Use Wireshark to verify the frames generated or captured by Process Bus Timing Lab.

## Display filter

```text
eth.type == 0x88f7 or ptp
```

## Capture filter

Use this when you need to capture untagged, VLAN, and QinQ Layer-2 PTP:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

## Validation path

1. Run protocol validation and export a PCAP.
2. Open the PCAP in Wireshark.
3. Confirm Announce, Sync, Follow_Up, Pdelay_Req, Pdelay_Resp, and Pdelay_Resp_Follow_Up decode as PTPv2.
4. For RAW mode, run RAW Self Test and verify the selected adapter or external tap/SPAN sees the frame.

## Important notes

Some adapters do not loop back locally injected packets to the capture path. A RAW Self Test can therefore report send success while local capture is not observed. In that case, validate with external Wireshark capture on a tap/SPAN/mirror port.
