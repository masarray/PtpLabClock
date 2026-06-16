# Process Bus Timing Lab Release Package

This package contains Process Bus Timing Lab binaries and license notices.

## Start here

- For the WPF app, run `PtpLabClock.App.exe`.
- For CLI workflows, run `PtpLabClock.Console.exe --help` or `PtpLabClock.Console.exe --list`.

## RAW/Npcap mode

RAW NIC mode requires Npcap installed on Windows and may require Administrator privileges. Use a wired Ethernet adapter for process-bus lab traffic.

Recommended first command:

```powershell
PtpLabClock.Console.exe --raw-self-test --adapter-index 0 --domain 0
```

Wireshark display filter:

```text
eth.type == 0x88f7 or ptp
```

## Safety

This software is a lab simulator and diagnostic companion. It is not a certified timing source or relay-acceptance grandmaster.
