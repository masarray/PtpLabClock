# Process Bus Timing Lab Portable Release

This release contains self-contained Windows portable EXE files for Process Bus Timing Lab.

## Fastest start

- Desktop app: run `PtpLabClock.App.win-x64.portable.exe`.
- Console tool: run `PtpLabClock.Console.win-x64.portable.exe --help` or `PtpLabClock.Console.win-x64.portable.exe --list`.

The direct `.exe` artifacts are designed for the simplest user path. ZIP packages include the same EXE plus license notices and this README.

## RAW/Npcap mode

RAW NIC mode requires Npcap installed on Windows and may require Administrator privileges. Use a wired Ethernet adapter for process-bus lab traffic.

Recommended first command:

```powershell
PtpLabClock.Console.win-x64.portable.exe --raw-self-test --adapter-index 0 --domain 0
```

VLAN self-test example:

```powershell
PtpLabClock.Console.win-x64.portable.exe --raw-self-test --adapter-index 0 --domain 0 --vlan --vlan-id 100 --vlan-pcp 4
```

Wireshark display filter:

```text
eth.type == 0x88f7 or ptp
```

## Safety

This software is a lab simulator and diagnostic companion. It is not a certified timing source, GPS grandmaster, hardware-timestamped clock, or relay-acceptance timing reference.
