# Global Reference Notes

These notes summarize public references used to guide implementation. They are not copied source.

## linuxptp / ptp4l

Useful lessons:

- Robust PTP implementation should separate message serialization, transport, clock/port runtime, profiles, and timestamp/servo concerns.
- Supports hardware/software timestamping, PHC, OC/BC/TC roles, UDP transports, and raw Ethernet Layer-2.
- Process Bus Timing Lab intentionally implements only the lab-simulator subset first.

## PTPSync

Useful lessons:

- Windows-friendly PTP tooling benefits from a manager UI around a background engine/service.
- Adapter selection, configuration, logs, and service state must be explicit.
- Our MVP keeps engine inside the app first; Windows Service mode is a future phase.

## Meinberg / Track Hound style tools

Useful lessons:

- Value is not only packet TX/RX; value is engineering visibility.
- Show detected GM, domains, message rates, interval health, master changes, warnings, and timeline.
- For our tool: simulator + monitor + validator + report is a stronger product than broadcaster only.

## Npcap / packet capture wrappers

Useful lessons:

- Windows raw packet capture/injection should be isolated behind a transport interface.
- RAW mode may fail due to admin rights, adapter capability, driver install, or unsupported devices.
- Demo Mode is required for reliable UX and offline training.
