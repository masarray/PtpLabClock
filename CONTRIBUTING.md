# Contributing

Thank you for considering a contribution to Process Bus Timing Lab.

This project is intentionally conservative because it generates and inspects process-bus timing traffic. Contributions should improve reliability, testability, documentation, or user safety before adding surface area.

## Development principles

- Keep Demo Mode independent from Npcap/SharpPcap and native packet drivers.
- Treat RAW packet mode as an explicit opt-in lab function.
- Do not claim this project is a certified grandmaster or relay-acceptance timing source.
- Add byte-level tests for every protocol serialization or parser change.
- Prefer small pull requests with a clear problem statement and validation evidence.
- Do not copy code, UI assets, packet fixtures, or documentation text from incompatible, proprietary, or unclear sources.

## Local validation

```powershell
.\tools\scripts\build.ps1
```

For RAW mode changes, also run:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --list
dotnet run --project .\src\PtpLabClock.Console -- --raw-self-test --adapter-index 0 --domain 0
```

Run RAW tests only on a controlled lab network.

## Pull request checklist

- [ ] The solution builds.
- [ ] Tests pass.
- [ ] Protocol changes include regression tests.
- [ ] README/docs are updated when user-visible behavior changes.
- [ ] New dependencies are justified and compatible with Apache-2.0 distribution.
- [ ] No proprietary or incompatible source code has been copied.
