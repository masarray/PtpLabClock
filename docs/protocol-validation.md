# Protocol Validation

The console tool can generate deterministic validation traffic without opening a NIC.

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0
```

To export validation frames to PCAP:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0 --export-pcap .\captures\ptp-validation.pcap
```

Validation currently covers:

- Announce
- Sync
- Follow_Up
- VLAN Announce
- QinQ Sync
- Pdelay_Req
- Pdelay_Resp
- Pdelay_Resp_Follow_Up

Open the PCAP in Wireshark and use:

```text
eth.type == 0x88f7 or ptp
```
