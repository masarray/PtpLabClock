<!-- SPDX-License-Identifier: Apache-2.0 -->
# Product Readiness Audit

Process Bus Timing Lab is positioned as a public-facing Windows engineering tool for PTPv2 Layer-2 lab visibility, IEC 61850 Process Bus timing checks, and passive timing-health evidence.

## Current product posture

| Area | Status | Notes |
|---|---|---|
| License readiness | Strong | Apache-2.0 repository license, package metadata, notices, and REUSE coverage. |
| Protocol serializer | Strong | Announce, Sync, Follow_Up, Pdelay, VLAN, and QinQ regression coverage. |
| RAW transport | Improving | SharpPcap/Npcap transport is isolated and guarded by RAW self-test. Field reliability still depends on NIC/driver/Npcap behavior. |
| Public website | Strong | SEO landing page, canonical URL, OpenGraph metadata, JSON-LD, sitemap, robots.txt, and Pages workflow. |
| Release automation | Strong | GitHub Release publishes direct portable `.exe` artifacts, ZIP packages with notices, checksums, SBOM-style manifest, and validation PCAP. |
| Product trust | Good | Build/test, CodeQL, Scorecard, Dependency Review, Dependabot, governance files, and security policy. |

## Product boundaries

The product must remain honest about timing capability:

- It is a lab simulator and diagnostic companion.
- It can help validate visibility, analyzer decoding, VLAN/QinQ handling, Pdelay response behavior, and timing-health symptoms.
- It is not a certified PTP grandmaster, GPS clock, hardware-timestamped timing source, or relay-acceptance reference.

## Public-facing checklist

- [x] Apache-2.0 license and metadata.
- [x] README optimized for engineering search intent.
- [x] GitHub Pages landing page.
- [x] SEO metadata, OpenGraph, Twitter card, canonical URL, JSON-LD.
- [x] `robots.txt` and `sitemap.xml`.
- [x] Release workflow with portable single EXE artifacts.
- [x] Checksums and release manifest.
- [x] Build/test/security automation.
- [x] Governance and contribution docs.
- [x] First tagged release with portable EXE artifacts.
- [x] Real screenshots from the WPF UI are used in README and GitHub Pages.
- [x] Short guided demo GIF based on the real WPF dashboard screenshot.
- [x] RAW NIC compatibility matrix document and evidence table template.
- [x] Deterministic golden PCAP fixtures for regression and analyzer smoke checks.

## Next maturity targets

1. Populate the RAW NIC compatibility table with real adapter, driver, and Npcap test results.
2. Add known-good analyzer screenshots for each golden PCAP fixture.
3. Add an in-app troubleshooting wizard for Npcap permission, adapter, VLAN, and self-capture issues.
4. Consider Authenticode code-signing for portable EXE artifacts to reduce Windows SmartScreen friction.
5. Add formal artifact attestations and full SBOM generation when repository policy is ready.
