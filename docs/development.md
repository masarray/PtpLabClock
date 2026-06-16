# Development

## Build

```powershell
.\tools\scripts\build.ps1
```

## Repository layout

```text
src/PtpLabClock.App        WPF dashboard
src/PtpLabClock.Core       Engine, scheduler, monitor, health, diagnostics
src/PtpLabClock.Protocol   PTPv2 and Ethernet serialization/parsing
src/PtpLabClock.Pcap       SharpPcap/Npcap RAW Layer-2 transport
src/PtpLabClock.Console    CLI monitor, self-test, validation
src/PtpLabClock.Reporting  PDF/session evidence export
tests/                     xUnit regression tests
```

## Dependency rule

Keep runtime dependencies minimal. New dependencies must be compatible with Apache-2.0 distribution and must not introduce GPL/AGPL/LGPL obligations.

## Protocol rule

Every serializer or parser change needs a byte-level regression test.
