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
eth.type == 0x88f7
```

## 3. Run the desktop app

```powershell
dotnet run --project .\src\PtpLabClock.App
```

Select **Demo Engine** first. Demo Mode does not require Npcap and should be used to validate UI flow, counters, health cards, scenario buttons, and export behavior.

## 4. RAW packet mode status

The repository includes a SharpPcap-backed `PtpLabClock.Pcap` transport. RAW mode requires Npcap installed on Windows and usually requires running the app as Administrator. Prefer a wired Ethernet NIC; many Wi-Fi/VPN adapters block raw injection.
