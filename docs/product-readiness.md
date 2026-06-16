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
- [ ] First signed/tagged release.
- [ ] Real screenshots from WPF UI.
- [ ] Short demo GIF or video.
- [ ] Field-lab RAW NIC compatibility matrix.
- [ ] Golden PCAP fixtures from known-good analyzer sessions.

## Next maturity targets

1. Publish `v0.1.0` with release notes and portable EXE artifacts.
2. Add real UI screenshots to `docs/assets/` and replace the CSS mockup preview when available.
3. Add a RAW NIC compatibility table for tested adapters and Npcap versions.
4. Add golden PCAP fixtures for untagged, VLAN, QinQ, Sync/Follow_Up, and Pdelay flows.
5. Add a troubleshooting wizard inside the WPF UI for Npcap permission, adapter, VLAN, and self-capture issues.
