# Passive Monitor

Passive monitor mode observes PTP traffic without generating synthetic timing frames.

```powershell
PtpLabClock.Console.exe --monitor --adapter-index 0 --domain 0
```

The monitor groups observed messages by domain and source clock identity, then tracks message counts, liveness, sequence anomalies, and last seen message type.

Add PCAP recording:

```powershell
PtpLabClock.Console.exe --monitor --adapter-index 0 --domain 0 --record-pcap .\captures\ptp-live.pcap
```
