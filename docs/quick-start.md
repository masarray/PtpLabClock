# Quick Start

## 1. Build

```powershell
dotnet restore .\PtpLabClock.sln
dotnet build .\PtpLabClock.sln -c Release --no-restore
dotnet test .\PtpLabClock.sln -c Release --no-build
```

Or run the repository script:

```powershell
.\tools\scripts\build.ps1
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

Select **Demo Engine** first. Demo Mode does not require Npcap and validates the UI flow, counters, health cards, scenario buttons, and export behavior.

## 4. Run RAW NIC self-test

RAW mode requires Npcap installed on Windows and may require Administrator privileges.

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --list
dotnet run --project .\src\PtpLabClock.Console -- --raw-self-test --adapter-index 0 --domain 0
```

VLAN-tagged self-test:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --raw-self-test --adapter-index 0 --domain 0 --vlan --vlan-id 100 --vlan-pcp 4
```

Prefer a wired Ethernet adapter. Wi-Fi, VPN, Bluetooth, and virtual adapters may capture but often block Layer-2 injection.

## 5. Start synthetic lab traffic

Untagged:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --adapter-index 0 --domain 0 --profile iec61850
```

VLAN-tagged:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --adapter-index 0 --domain 0 --profile iec61850 --vlan --vlan-id 100 --vlan-pcp 4
```

## 6. Wireshark filters

Display filter:

```text
eth.type == 0x88f7 or ptp
```

Capture filter for untagged, VLAN, and QinQ PTP:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```
