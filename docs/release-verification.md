<!-- SPDX-License-Identifier: Apache-2.0 -->
# Release Verification

Process Bus Timing Lab releases are designed to be portable and easy to verify.

## Recommended download

For most users:

```text
PtpLabClock.App.win-x64.portable.exe
```

For command-line validation, RAW self-test, and scripts:

```text
PtpLabClock.Console.win-x64.portable.exe
```

ZIP packages include the executable plus `LICENSE`, `NOTICE`, third-party notices, and a quick release README.

## Verify checksum

Download `checksums.txt` from the same GitHub Release as the EXE.

PowerShell:

```powershell
Get-FileHash .\PtpLabClock.App.win-x64.portable.exe -Algorithm SHA256
Get-Content .\checksums.txt
```

The hash printed by `Get-FileHash` must match the corresponding line in `checksums.txt`.

## Windows SmartScreen

Portable EXE files are not code-signed yet. Windows SmartScreen may warn on first run, especially for new open-source releases.

This warning does not automatically mean the file is unsafe. It means Windows does not yet have reputation for that executable/signature. Verify the SHA256 checksum from the GitHub Release before running.

## RAW NIC requirements

RAW NIC mode requires:

- Windows 10/11.
- Npcap installed.
- Administrator rights when required by local policy.
- Wired Ethernet or a controlled lab adapter is preferred.
- Wi-Fi, VPN, loopback, and virtual adapters may not support injection or self-capture consistently.

## Safety boundary

This software is a lab simulator and diagnostic companion. It is not a certified PTP grandmaster, GPS clock, hardware-timestamped timing source, or relay-acceptance reference.
