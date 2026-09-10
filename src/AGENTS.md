# Source Addendum — PTP Timing, Result Handling, and Diagnostics

The repository root `AGENTS.md` remains authoritative. This scoped addendum applies to `src/**` and strengthens deterministic failure handling and performance rules for protocol, transport, monitor, health, reporting, console, and WPF source code.

## Root-cause workflow

For non-trivial defects use:

REPRODUCE -> TRACE MESSAGE/STATE OWNERSHIP -> ROOT CAUSE -> SMALLEST COHERENT FIX -> REGRESSION TEST -> TIMING/FAILURE CHECK -> BUILD.

If three consecutive patches in the same subsystem still treat symptoms, stop before patch four and re-audit the architecture/state flow. Do not solve packet/state races with arbitrary sleeps, retries, duplicate state, or UI-side correction.

## Exception-free timing-sensitive paths

Expected or recoverable conditions must not use exceptions as routine control flow in:
- packet serialization/parsing;
- raw RX/TX loops;
- Pdelay pairing/response logic;
- passive monitor processing;
- sequence/interval/health evaluation;
- scheduler/timer callbacks;
- other high-frequency timing paths.

Prefer `TryXxx`, compact typed Result/status records, enums with output values, or nullable values only when failure detail is unnecessary.

Normal conditions such as malformed frame, unsupported message type, sequence discontinuity, missing Follow_Up, timeout, unavailable raw transport, unsuitable adapter, bounded queue saturation, or incomplete peer state must produce explicit status/evidence rather than repeated exception unwinding.

Exceptions from Npcap, OS/network APIs, filesystem/config/reporting, WPF, or third-party infrastructure may still occur. Catch them at meaningful boundaries and convert them into structured application status/diagnostics. Never let infrastructure exceptions unwind through deterministic packet-processing callbacks.

## Defensive packet handling

Treat all captured bytes and configuration fields as untrusted until validated. Before indexing or converting validate:
- Ethernet/PTP minimum length;
- EtherType/VLAN layout;
- messageLength against available bytes;
- message type/version/domain/flags;
- timestamp and correction-field bounds;
- sequence and identity field lengths;
- integer/range conversions.

Malformed traffic must not terminate monitor or simulator operation. Preserve raw evidence when decoding is uncertain.

## Bounded asynchronous diagnostics

High-rate protocol/timing paths must not perform expensive logging, file writes, report generation, JSON serialization, stack-trace formatting, or synchronous WPF notifications.

Emit only compact structured events/counters such as error code, subsystem, sequence/domain/message type, timestamp/counter, and small numeric context. Transport must be bounded and non-blocking for high-rate producers.

Repeated failures must be aggregated/deduplicated/rate-limited. Queue saturation must use an explicit drop/coalesce policy and retain counters. Diagnostics are observational; their failure must not stall RX/TX, Pdelay response, scheduler operation, passive monitoring, or Demo Mode.

Human-readable messages/report evidence are formatted on a lower-rate/background consumer.

## Timing truth and claims

Software arrival timestamps are evidence about the capture path, not certified PTP accuracy. Do not convert average callback timing into an accuracy claim.

Keep protocol timestamps, software arrival time, scheduler time, and UI presentation time semantically distinct. Any future hardware timestamping must be represented as a separate capability with explicit provenance.

Measure worst-case/sustained behavior where timing-sensitive code changes, not only average latency.

## State ownership

Core owns engine/session/monitor state. Protocol owns wire semantics. Transport owns adapter/raw I/O. Reporting consumes snapshots/evidence and must not become runtime state authority. WPF presents state and issues commands; it must not repair protocol state independently.

Candidate configuration/session state should be validated before activation. Failed reconfiguration must retain last-known-good state where safe or transition to an explicit stopped/faulted state; never leave a half-applied profile.

## Backpressure and UI handoff

Do not render one UI update per received/transmitted packet. Batch/coalesce high-frequency counters and health/status snapshots. Event evidence requiring retention must have an explicit bound/export policy rather than unbounded in-memory growth.

The WPF thread must not perform blocking capture I/O, packet parsing, report generation, PCAP processing, or large serialization.

## Performance/resource evidence

For relevant changes measure:
- packet/message throughput;
- scheduler jitter/deadline misses;
- Pdelay response latency distribution;
- queue depth/drop/coalesce counters;
- steady-state allocation rate;
- CPU and memory growth;
- UI update latency under sustained monitor load.

Do not add workers, queues, caches, or pools without a demonstrated need and explicit lifecycle owner.

## Definition of done

Protocol/timing changes require deterministic byte-level regression coverage where practical, malformed/failure-path testing, successful repository build/test gates, and PCAP/Wireshark-verifiable evidence when wire behavior changes. Performance-sensitive changes require measured evidence before claiming improvement.
