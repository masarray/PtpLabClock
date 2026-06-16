# PCAP Session Recorder v12

This version adds a classic Ethernet PCAP writer for validation and lab evidence capture.

## Console validation PCAP

Generate deterministic protocol validation frames without Npcap/admin rights:

```powershell
cd src\PtpLabClock.Console
dotnet run -- --validate-protocol --domain 0 --export-pcap ..\..\captures\ptp-validation.pcap
```

Open the exported file in Wireshark and use:

```text
eth.type == 0x88f7
```

Expected frames:

- Announce
- Sync
- Follow_Up
- Pdelay_Resp
- Pdelay_Resp_Follow_Up

## Live TX/RX session recording

Record observed TX/RX frames while running on a real adapter:

```powershell
dotnet run -- --adapter-index 0 --domain 0 --profile iec61850 --record-pcap ..\..\captures\ptp-live.pcap
```

The recorder subscribes to the engine `FrameObserved` event. It writes frames after TX sanity check and also records valid RX PTP frames observed by the engine.

## Scope

The PCAP writer is for lab evidence and Wireshark validation. It is not a replacement for a full packet capture engine and does not currently store custom comments, interface metadata, or pcapng enhanced packet metadata.
