# Third-Party Notices

Process Bus Timing Lab / PtpLabClock is licensed as **GPL-3.0-or-later**.

This project uses clean-room implementation principles. It learns from public behavior, architecture, and documentation of existing tools, but it must not copy proprietary or incompatible source code into this repository.

## Dependencies

| Component | Purpose | Notes |
|---|---|---|
| SharpPcap | .NET packet capture/injection wrapper | Pulled from NuGet at restore/build time. Do not vendor dependency source into this repository unless its license is reviewed. |
| Npcap | Windows packet capture/injection driver | Required for RAW packet mode. Do **not** bundle the Npcap installer without checking Npcap redistribution/OEM licensing. User-installed Npcap is the preferred path. |
| .NET / WPF | Windows desktop UI/runtime | Follow Microsoft redistribution terms for the chosen deployment model. |

## Reference Projects Studied, Not Copied

| Reference | Lesson copied conceptually | Copy-source policy |
|---|---|---|
| linuxptp / ptp4l | clock/port/transport/message/profile separation, hardware/software timestamping boundaries, raw L2 and UDP transport thinking | Do not copy source. Use only clean-room concepts and public protocol knowledge. |
| PTPSync | Windows service + manager UI split, adapter/config workflow | Do not copy source unless license review and attribution are completed. Prefer clean-room rewrite. |
| Meinberg PTP Client / Track Hound | user-facing status, diagnostic visibility, traffic analysis workflow | Proprietary/commercial reference. Do not copy UI assets, wording, or code. |

## Scope Disclaimer

This application is a **lab simulator and diagnostic companion**. It is not a certified timing source and must not be represented as a replacement for a GPS/PTP grandmaster in protection, metering, or final acceptance testing.
