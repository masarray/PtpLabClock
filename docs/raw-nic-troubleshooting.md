<!-- SPDX-License-Identifier: Apache-2.0 -->
# RAW NIC Troubleshooting Playbook

Use this playbook when RAW mode does not start, does not send, or does not appear in Wireshark.

## 1. No adapters listed

Likely causes:

- Npcap is not installed.
- Npcap service is disabled.
- The app is not running with enough privileges.
- Only loopback, VPN, Wi-Fi, or virtual adapters are exposed.

Actions:

```powershell
dotnet run --project .\src\PtpLabClock.Console -- --list
```

Then reinstall/update Npcap and run the app as Administrator.

## 2. Adapter opens but send fails

Likely causes:

- Driver blocks injection.
- Adapter is down or disconnected.
- Selected adapter is virtual/VPN/Wi-Fi.
- Security software prevents packet injection.

Actions:

- Prefer wired Ethernet.
- Disable unused VPN/virtual adapters during the test.
- Use a controlled lab switch/TAP.
- Try another USB Ethernet adapter and record the result in `docs/raw-nic-compatibility.md`.

## 3. Send succeeds but local capture does not see the packet

This can happen with some drivers. Local self-capture is useful, but external capture is stronger evidence.

Actions:

- Run Wireshark on the same adapter.
- If possible, capture on a mirror/TAP/external analyzer.
- Use this display filter:

```text
eth.type == 0x88f7 or ptp
```

## 4. VLAN packet not visible

Actions:

- Enable VLAN tag in the app or CLI.
- Confirm VLAN ID and PCP.
- Use this capture filter:

```text
ether proto 0x88f7 or (vlan and ether proto 0x88f7) or (vlan and vlan and ether proto 0x88f7)
```

## 5. Relay/analyzer still does not accept timing

This tool is a simulator and timing-visibility companion, not a certified timing source. Relay acceptance may depend on real grandmaster quality, domain, profile, BMCA behavior, hardware timestamping, sync accuracy, and network architecture.

Use a proper certified PTP grandmaster for final protection or metering validation.

## Evidence to capture in an issue

Include:

- Windows version.
- Npcap version.
- Adapter model and driver.
- Whether the app ran as Administrator.
- RAW self-test output.
- Wireshark screenshot or PCAP.
- VLAN/QinQ configuration.
- Whether external capture saw the frame.
