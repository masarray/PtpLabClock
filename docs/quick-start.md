# Quick Start

## 1. Build

```powershell
dotnet restore .\PtpLabClock.sln
dotnet build .\PtpLabClock.sln -c Release --no-restore
dotnet test .\PtpLabClock.sln -c Release --no-build
```

## 2. Validate protocol layout without RAW packet access

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0
```

Expected result: `Result: PASS`.

Optional PCAP export:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0 --export-pcap .\captures\ptp-validation.pcap
```

Open the PCAP in Wireshark and use:

```text
eth.type == 0x88f7 or ptp
```

## 3. Run the desktop app

```powershell
dotnet run --project .\src\PtpLabClock.App
```

Select **Demo Engine** first. Demo Mode does not require Npcap and should be used to validate UI flow, counters, health cards, scenario buttons, and export behavior.

## 4. RAW packet mode status

The repository includes a SharpPcap-backed `PtpLabClock.Pcap` transport. RAW mode requires Npcap installed on Windows and usually requires running the app as Administrator. Prefer a wired Ethernet NIC; many Wi-Fi/VPN adapters block raw injection. After selecting a real NIC, click **RAW Self Test** before **Start Engine**. A PASS/WARN result tells you whether open/filter/send worked and whether local self-capture was observed.


## 5. VLAN-aware PTP capture filter

For external analyzer validation, use this capture filter when VLAN-tagged process-bus traffic may be present:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```
