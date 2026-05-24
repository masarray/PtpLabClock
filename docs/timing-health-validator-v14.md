# Timing Health Validator Lite v14

Visual design is locked. This phase adds a lightweight diagnostic layer that converts passive PTP monitor snapshots into engineering-oriented PASS / WARN / FAIL results.

## Scope

The validator is intended for lab visibility and analyzer readiness checks. It is not a timing accuracy verifier and it does not certify IED synchronization quality.

## Checks

- PTP Visibility: validates whether valid PTP frames are visible.
- Domain Match: compares observed domains with the expected lab domain.
- GM Stability: checks for a single live Announce source.
- Follow_Up Pairing: checks whether Follow_Up traffic is reasonably paired with Sync traffic.
- Pdelay Activity: checks for Pdelay_Req / Pdelay_Resp / Pdelay_Resp_Follow_Up visibility.
- Sequence Continuity: lightweight adjacent same-message sequence anomaly check.
- Analyzer Readiness: aggregate diagnostic result for basic process bus analyzer validation.

## Console Usage

Passive health monitor:

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0
```

With PCAP recording:

```powershell
dotnet run --project src\PtpLabClock.Console -- --health --adapter-index 0 --domain 0 --record-pcap captures\ptp-health.pcap
```

## Notes

Follow_Up pairing uses a ratio-based check because real traffic can be sampled during capture start/stop boundaries. Pdelay activity is a visibility check, not a delay accuracy check.
