# Changelog

All notable changes to this project are documented here.

## Unreleased

### Added

- Public-facing governance files: contribution guide, security policy, support guide, code of conduct, issue templates, pull request template, and CODEOWNERS.
- Hardened GitHub Actions for build/test, CodeQL, Dependency Review, OpenSSF Scorecard, Dependabot, and release packaging.
- Release packaging for Windows framework-dependent and self-contained artifacts.
- Release checksums and a lightweight SPDX-style release SBOM manifest.
- CLI VLAN controls for generated PTP traffic: `--vlan`, `--vlan-id`, and `--vlan-pcp`.
- WPF VLAN controls for RAW process-bus lab traffic.
- RAW Self Test support for optional VLAN-tagged Announce test frames.
- Additional protocol tests for malformed/truncated frames and sequence wraparound behavior.

### Changed

- README rewritten as a public landing page with quick-start, release, RAW mode, and safety sections.
- Documentation reorganized into product-facing docs and design notes.
- Build workflow uses least-privilege permissions and a Windows configuration matrix.

### Safety

- Public wording remains explicit that the project is a lab simulator and diagnostic companion, not a certified grandmaster or relay-acceptance timing source.
