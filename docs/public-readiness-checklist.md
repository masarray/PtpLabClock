# Public Readiness Checklist

Use this checklist before publishing a release.

## Repository trust

- [ ] Build and Test workflow is green.
- [ ] CodeQL workflow is green.
- [ ] Dependency Review is enabled for pull requests.
- [ ] OpenSSF Scorecard workflow is enabled.
- [ ] Dependabot is enabled for NuGet and GitHub Actions.
- [ ] SECURITY.md, CONTRIBUTING.md, SUPPORT.md, CODE_OF_CONDUCT.md, and CHANGELOG.md are present.

## Release quality

- [ ] Framework-dependent WPF package exists.
- [ ] Self-contained WPF package exists.
- [ ] Self-contained console package exists.
- [ ] `checksums.txt` is attached.
- [ ] `PtpLabClock.release-sbom.spdx.json` is attached.
- [ ] Protocol validation PCAP is attached.
- [ ] Release notes mention RAW/Npcap requirements and safety limitations.

## Technical validation

- [ ] `--validate-protocol --domain 0` returns PASS.
- [ ] Validation PCAP decodes in Wireshark.
- [ ] RAW Self Test succeeds on at least one wired Ethernet adapter.
- [ ] VLAN Self Test is validated when publishing VLAN claims.
- [ ] Passive monitor sees traffic on the target lab network.

## Safety wording

- [ ] README does not claim certified grandmaster behavior.
- [ ] Docs do not claim relay-acceptance timing accuracy.
- [ ] RAW mode is described as lab-only and software-timestamped.
