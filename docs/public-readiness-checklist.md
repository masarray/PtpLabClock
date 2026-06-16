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

- [ ] Direct portable desktop EXE exists.
- [ ] Direct portable console EXE exists.
- [ ] ZIP packages with license and notices exist.
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


## Product proof

- [ ] README displays the real dashboard screenshot.
- [ ] GitHub Pages displays the real dashboard screenshot.
- [ ] Demo GIF is present and referenced from README or landing page.
- [ ] Golden PCAP fixtures are present and tested.
- [ ] RAW NIC compatibility matrix exists and has at least one real adapter result before public claims are broadened.
