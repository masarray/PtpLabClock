# Security Policy

## Supported versions

Security fixes are handled on the default branch and the latest tagged release.

## Reporting a vulnerability

Please report security issues privately by opening a GitHub security advisory or by contacting the maintainer directly. Do not publish exploit details before a fix or mitigation is available.

Useful reports include:

- affected version or commit,
- operating system and .NET version,
- whether RAW/Npcap mode was enabled,
- adapter type and driver context if relevant,
- minimal reproduction steps,
- expected and observed impact.

## Scope

In scope:

- unsafe file handling,
- package/release integrity issues,
- malformed packet parsing crashes,
- unexpected network behavior in RAW mode,
- denial-of-service defects caused by crafted capture traffic.

Out of scope:

- timing accuracy limitations caused by software timestamping,
- inability of Wi-Fi/VPN/virtual adapters to inject Layer-2 frames,
- issues caused by modified third-party packet drivers or unsafe lab networks.

## Safety boundary

This project is a lab simulator and diagnostic companion. It must not be used as a certified timing source for protection, metering, revenue, or final commissioning acceptance.
