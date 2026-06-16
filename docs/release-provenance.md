<!-- SPDX-License-Identifier: Apache-2.0 -->
# Release Provenance and Signing Notes

## Current release trust model

Process Bus Timing Lab release automation provides:

- Build/test gate in GitHub Actions.
- Portable self-contained Windows EXE artifacts.
- ZIP packages with license and notices.
- SHA256 checksums.
- SPDX-style release manifest.
- Protocol validation PCAP generated during the release workflow.

## Current limitation

Portable EXE artifacts are not code-signed yet. Windows SmartScreen may show a warning for new releases.

## Recommended verification

Use the release `checksums.txt` file:

```powershell
Get-FileHash .\PtpLabClock.App.win-x64.portable.exe -Algorithm SHA256
```

Compare the hash with `checksums.txt`.

## Future signing path

For a higher-trust public release pipeline:

1. Use Authenticode code signing for Windows EXE artifacts.
2. Add GitHub Artifact Attestations when repository policy is ready.
3. Publish a full SPDX or CycloneDX SBOM.
4. Keep release notes explicit about RAW NIC requirements and timing limitations.
5. Keep checksums for all direct `.exe`, `.zip`, and validation `.pcap` artifacts.
