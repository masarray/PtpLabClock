## Summary

What changed and why?

## Validation

- [ ] `dotnet build .\PtpLabClock.sln -c Release`
- [ ] `dotnet test .\PtpLabClock.sln -c Release`
- [ ] `dotnet run --project .\src\PtpLabClock.Console -- --validate-protocol --domain 0`

## RAW/Npcap impact

- [ ] No RAW/Npcap impact
- [ ] RAW mode tested with `--raw-self-test`
- [ ] Wireshark validation performed with `eth.type == 0x88f7 or ptp`

## Safety and docs

- [ ] No timing-source overclaim introduced
- [ ] README/docs updated when user-visible behavior changed
- [ ] New dependencies reviewed for license and security posture
