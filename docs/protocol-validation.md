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


## Golden PCAP fixtures

Deterministic PCAP fixtures are stored under:

```text
tests/PtpLabClock.Protocol.Tests/Fixtures/pcap/
```

They cover untagged Announce, VLAN Sync/Follow_Up, QinQ Pdelay, and a mixed process-bus smoke fixture. See [Golden PCAP fixtures](golden-pcap-fixtures.md).
