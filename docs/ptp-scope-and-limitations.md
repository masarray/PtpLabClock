# PTP Scope and Limitations

Process Bus Timing Lab is a lab simulator and diagnostic companion. It helps engineers see and validate PTP traffic behavior, but it is not a certified timing source.

## Safe uses

- Validate that Announce / Sync / Follow_Up / Pdelay messages are serialized and decoded correctly.
- Generate protocol validation PCAP files.
- Exercise analyzer UI and parser logic.
- Observe passive PTP traffic and summarize basic timing-health symptoms.
- Document lab evidence for discussion, troubleshooting, and early FAT preparation.

## Not safe to claim

- Do not claim grandmaster accuracy.
- Do not claim relay acceptance timing validity.
- Do not claim hardware timestamping performance.
- Do not adjust OS time.
- Do not replace a GPS grandmaster, boundary clock, or real PTP-capable test set.

## Why software-only PTP is limited

Windows user-space timers and ordinary NIC transmit paths do not provide deterministic hardware timestamps. A relay may require valid PTP profile behavior, domain, clock quality, peer delay behavior, and stable synchronization state before accepting sampled value traffic. Synthetic software frames can help analyzer testing, but they cannot prove protection-grade synchronization.
