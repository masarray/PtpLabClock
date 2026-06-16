# Health Dashboard UI Integration v15

This step integrates the Timing Health Validator into the WPF dashboard without changing the locked visual design direction.

## Scope

- Adds a compact `Timing Health` section under the main Lab Clock / Scenario area.
- Displays seven diagnostic cards:
  - PTP Visibility
  - Domain Match
  - GM Stability
  - Follow_Up Pairing
  - Pdelay Activity
  - Sequence Continuity
  - Analyzer Readiness
- Updates cards from `PtpMasterEngine.MonitorSnapshotUpdated`.
- Uses `PtpTimingHealthValidator` with the configured lab domain.

## Design Rules

- Visual design remains locked.
- No redesign of core cards, dropdowns, or buttons.
- Health cards must remain compact and readable at a glance.
- PASS / WARN / FAIL colors are soft backgrounds, not aggressive industrial alarm colors.

## Limitation

The health dashboard evaluates observed PTP traffic and simulator monitor snapshots. It is a lab diagnostic aid, not timing accuracy validation and not a replacement for certified GPS/PTP grandmaster verification.
