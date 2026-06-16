# Installation

## From release packages

Download one of the Windows packages from the GitHub Releases page:

- `PtpLabClock.App.win-x64.self-contained.zip` — easiest for non-developers; includes the required .NET runtime bits.
- `PtpLabClock.App.win-x64.framework-dependent.zip` — smaller package; requires .NET 8 Desktop Runtime.
- `PtpLabClock.Console.win-x64.self-contained.zip` — CLI validation and monitor workflows.

Unzip the package to a writable folder. Keep `LICENSE`, `NOTICE`, and `THIRD-PARTY-NOTICES.md` with the binaries.

## RAW/Npcap requirements

RAW NIC mode requires Npcap installed on Windows and may require running the app as Administrator.

Recommended first test:

```powershell
PtpLabClock.Console.exe --list
PtpLabClock.Console.exe --raw-self-test --adapter-index 0 --domain 0
```

Use a wired Ethernet adapter for process-bus lab traffic. Wi-Fi, VPN, Bluetooth, and virtual adapters may capture packets but often block injection.

## From source

```powershell
dotnet restore .\PtpLabClock.sln
dotnet build .\PtpLabClock.sln -c Release
dotnet test .\PtpLabClock.sln -c Release
dotnet run --project .\src\PtpLabClock.App
```
