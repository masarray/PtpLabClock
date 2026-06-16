# Third-Party Notices

Process Bus Timing Lab / PtpLabClock is licensed under **Apache-2.0**.

This project uses clean-room implementation principles. It may study public behavior, architecture, and documentation of existing tools, but incompatible or proprietary source code must not be copied into this repository.

## Runtime / Build Dependencies

| Component | Purpose | Notes |
|---|---|---|
| .NET 8 | Runtime, libraries, console app, test execution | Follow Microsoft redistribution terms for the chosen deployment model. |
| WPF | Windows desktop UI | Used only by `PtpLabClock.App`. |
| xUnit | Unit/regression tests | Restored from NuGet for test projects only. |
| Microsoft.NET.Test.Sdk | Test execution | Restored from NuGet for test projects only. |

## Optional External Components Not Bundled

| Component | Purpose | Policy |
|---|---|---|
| Npcap | RAW Layer-2 packet capture/injection on Windows | Installer/driver is not bundled. Prefer user-installed Npcap unless redistribution/OEM licensing is confirmed. |
| SharpPcap | .NET capture/injection binding for RAW mode | NuGet dependency; MIT license; not bundled as source. |

## Reference Projects Studied, Not Copied

| Reference | Lesson copied conceptually | Copy-source policy |
|---|---|---|
| linuxptp / ptp4l | clock/port/transport/message/profile separation, hardware/software timestamping boundaries, raw L2 and UDP transport thinking | Do not copy source. Use only clean-room concepts and public protocol knowledge. |
| PTPSync | Windows service + manager UI split, adapter/config workflow | Do not copy source unless license review and attribution are completed. Prefer clean-room rewrite. |
| Meinberg PTP Client / Track Hound | user-facing status, diagnostic visibility, traffic analysis workflow | Proprietary/commercial reference. Do not copy UI assets, wording, or code. |

## Scope Disclaimer

This application is a **lab simulator and diagnostic companion**. It is not a certified timing source and must not be represented as a replacement for a GPS/PTP grandmaster in protection, metering, or final acceptance testing.
