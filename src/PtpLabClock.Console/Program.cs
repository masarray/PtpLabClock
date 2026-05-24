// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Core.Engine;
using PtpLabClock.Core.Health;
using PtpLabClock.Core.Monitor;
using PtpLabClock.Core.Recording;
using PtpLabClock.Protocol;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Ethernet;
using PtpLabClock.Protocol.Messages;
using PtpLabClock.Protocol.Serialization;
using PtpLabClock.Pcap.Adapters;
using PtpLabClock.Pcap.Transport;

if (args.Contains("--validate-protocol"))
{
    var exportPcap = ReadString(args, "--export-pcap", string.Empty);
    RunProtocolValidation(ReadByte(args, "--domain", 0), exportPcap);
    return;
}

var adapters = new NpcapAdapterProvider().GetAdapters();

if (args.Contains("--list") || args.Length == 0)
{
    Console.WriteLine("Process Bus Timing Lab - PTP Lab Clock Simulator - adapters");
    for (var i = 0; i < adapters.Count; i++)
        Console.WriteLine($"[{i}] {adapters[i].Description}\n    {adapters[i].Id}");
    Console.WriteLine();
    Console.WriteLine("Run examples:");
    Console.WriteLine("dotnet run -- --adapter-index 0 --domain 0 --profile iec61850");
    Console.WriteLine("dotnet run -- --adapter-index 0 --domain 0 --record-pcap .\\captures\\ptp-live.pcap");
    Console.WriteLine("dotnet run -- --monitor --adapter-index 0 --domain 0");
    Console.WriteLine("dotnet run -- --health --adapter-index 0 --domain 0");
    Console.WriteLine("dotnet run -- --validate-protocol --domain 0");
    Console.WriteLine("dotnet run -- --validate-protocol --domain 0 --export-pcap .\\captures\\ptp-validation.pcap");
    return;
}

var adapterIndex = ReadInt(args, "--adapter-index", 0);
if (adapterIndex < 0 || adapterIndex >= adapters.Count)
{
    Console.Error.WriteLine("Invalid adapter index. Use --list first.");
    return;
}

var domain = ReadByte(args, "--domain", 0);
var profileText = ReadString(args, "--profile", "iec61850");
var recordPcapPath = ReadString(args, "--record-pcap", string.Empty);
var profile = profileText.Equals("generic", StringComparison.OrdinalIgnoreCase)
    ? PtpProfilePreset.GenericPtpV2
    : profileText.Equals("analyzer", StringComparison.OrdinalIgnoreCase)
        ? PtpProfilePreset.AnalyzerTest
        : PtpProfilePreset.Iec61850_9_3_Lab;

if (args.Contains("--monitor") || args.Contains("--health"))
{
    await RunPassiveMonitorAsync(adapters[adapterIndex], domain, recordPcapPath, args.Contains("--health"));
    return;
}

PcapSessionWriter? recorder = null;
try
{
    await using var transport = new NpcapPtpTransport();
    await using var engine = new PtpMasterEngine(transport);

    if (!string.IsNullOrWhiteSpace(recordPcapPath))
    {
        recorder = new PcapSessionWriter(recordPcapPath);
        engine.FrameObserved += (_, e) => recorder.WriteFrame(e.Frame, e.Timestamp);
        Console.WriteLine($"PCAP recording enabled: {recordPcapPath}");
    }

    engine.EventLogged += (_, e) => Console.WriteLine(e.Item);
    engine.FrameObserved += (_, e) => Console.WriteLine($"{e.Timestamp:HH:mm:ss.fff} {e.Summary}");
    engine.CountersUpdated += (_, e) => Console.Title = $"Announce={e.Counters.AnnounceTx} Sync={e.Counters.SyncTx} FollowUp={e.Counters.FollowUpTx} PdelayRx={e.Counters.PdelayReqRx} Last={e.Counters.LastTxSummary}";

    var adapter = adapters[adapterIndex];
    var options = new PtpEngineOptions
    {
        AdapterId = adapter.Id,
        AdapterName = adapter.Description,
        DomainNumber = domain,
        ProfilePreset = profile
    };

    await engine.StartAsync(options);
    Console.WriteLine("Running. Press ENTER to stop.");
    Console.ReadLine();
    await engine.StopAsync();
}
finally
{
    if (recorder is not null)
    {
        recorder.Dispose();
        Console.WriteLine($"PCAP saved. Packets={recorder.PacketCount}");
    }
}

static async Task RunPassiveMonitorAsync(PtpLabClock.Core.Abstractions.NetworkAdapterInfoDto adapter, byte domain, string recordPcapPath, bool showHealth)
{
    Console.WriteLine(showHealth ? "Process Bus Timing Lab - Timing Health Validator" : "Process Bus Timing Lab - Passive PTP Monitor");
    Console.WriteLine($"Adapter : {adapter.Description}");
    Console.WriteLine($"Domain  : {domain}");
    Console.WriteLine("Filter  : eth.type == 0x88f7");
    Console.WriteLine("Press ENTER to stop.");
    Console.WriteLine();

    var monitor = new PtpPassiveMonitor();
    var validator = new PtpTimingHealthValidator();
    var healthOptions = PtpHealthValidatorOptions.ForLabDomain(domain);
    PcapSessionWriter? recorder = null;
    await using var transport = new NpcapPtpTransport();

    if (!string.IsNullOrWhiteSpace(recordPcapPath))
    {
        recorder = new PcapSessionWriter(recordPcapPath);
        Console.WriteLine($"PCAP recording enabled: {recordPcapPath}");
        Console.WriteLine();
    }

    transport.PacketReceived += (_, e) =>
    {
        recorder?.WriteFrame(e.Frame);
        var snapshot = monitor.ObserveFrame(e.Frame, "RX");
        if (snapshot.LastMessage is { } last)
            Console.WriteLine($"{last.Timestamp:HH:mm:ss.fff} {last.Summary} [{last.Transport}, len={last.MessageLength}]");
    };

    await transport.OpenAsync(adapter.Id);
    await transport.StartCaptureAsync();

    using var cts = new CancellationTokenSource();
    var statusTask = Task.Run(async () =>
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(2000, cts.Token).ConfigureAwait(false);
                var snapshot = monitor.GetSnapshot();
                Console.Title = $"PTP Monitor frames={snapshot.TotalFrames} domains={snapshot.DetectedDomainCount} sources={snapshot.Sources.Count} live={snapshot.LiveSourceCount}";
                Console.WriteLine($"-- {snapshot.Summary}");

                foreach (var source in snapshot.Sources.Take(5))
                {
                    var state = source.IsLive(DateTime.Now, TimeSpan.FromSeconds(5)) ? "LIVE" : "LOST";
                    Console.WriteLine($"   {state,-4} domain={source.Domain} src={source.ClockIdentity} Ann={source.AnnounceCount} Sync={source.SyncCount} FU={source.FollowUpCount} PdelayReq={source.PdelayReqCount} SeqWarn={source.SequenceAnomalyCount} Last={source.LastMessageType}/{source.LastSequenceId}");
                }

                if (showHealth)
                {
                    var health = validator.Evaluate(snapshot, healthOptions);
                    Console.WriteLine($"   HEALTH {health.Summary}");
                    foreach (var check in health.Checks)
                        Console.WriteLine($"      {check.Level.ToString().ToUpperInvariant(),-4} {check.Name,-20} {check.Summary}");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    });

    Console.ReadLine();
    cts.Cancel();
    await transport.StopCaptureAsync();
    try { await statusTask.ConfigureAwait(false); } catch (OperationCanceledException) { }

    if (recorder is not null)
    {
        recorder.Dispose();
        Console.WriteLine($"PCAP saved. Packets={recorder.PacketCount}");
    }
}

static void RunProtocolValidation(byte domain, string exportPcapPath)
{
    Console.WriteLine("Process Bus Timing Lab - Protocol validation");
    Console.WriteLine($"Domain: {domain}");
    Console.WriteLine();

    var frames = BuildValidationFrames(domain);
    var hasFailure = false;

    PcapSessionWriter? pcap = null;
    try
    {
        if (!string.IsNullOrWhiteSpace(exportPcapPath))
        {
            pcap = new PcapSessionWriter(exportPcapPath);
            Console.WriteLine($"Exporting validation PCAP: {exportPcapPath}");
            Console.WriteLine();
        }

        foreach (var frame in frames)
        {
            var result = Validate(frame.Label, frame.Frame, domain);
            hasFailure |= !result.IsValid;
            pcap?.WriteFrame(frame.Frame);
        }
    }
    finally
    {
        pcap?.Dispose();
    }

    Console.WriteLine();
    Console.WriteLine("Wireshark display filter: eth.type == 0x88f7");
    Console.WriteLine(hasFailure ? "Result: FAIL" : "Result: PASS");
}

static IReadOnlyList<(string Label, byte[] Frame)> BuildValidationFrames(byte domain)
{
    var serializer = new PtpMessageSerializer();
    var build = new PtpBuildOptions
    {
        DomainNumber = domain,
        ClockIdentity = ClockIdentity.Parse("02-00-00-FF-FE-00-00-01"),
        ClockClass = 248,
        ClockAccuracy = PtpClockAccuracy.Unknown,
        TwoStep = true
    };
    var srcMac = MacAddress.Parse("02-00-00-00-00-01");
    var requester = new byte[] { 0x02, 0x00, 0x00, 0xFF, 0xFE, 0xAA, 0x10, 0x01, 0x00, 0x01 };
    var syncTimestamp = PtpTimestamp.Now();
    var responseTimestamp = PtpTimestamp.Now();

    return new List<(string Label, byte[] Frame)>
    {
        ("Announce", EthernetFrameBuilder.Build(PtpMulticastAddresses.General, srcMac, EtherTypes.Ptp, serializer.BuildAnnounce(build, 1))),
        ("Sync", EthernetFrameBuilder.Build(PtpMulticastAddresses.General, srcMac, EtherTypes.Ptp, serializer.BuildSync(build, 2, syncTimestamp))),
        ("Follow_Up", EthernetFrameBuilder.Build(PtpMulticastAddresses.General, srcMac, EtherTypes.Ptp, serializer.BuildFollowUp(build, 2, syncTimestamp))),
        ("Pdelay_Resp", EthernetFrameBuilder.Build(PtpMulticastAddresses.PeerDelay, srcMac, EtherTypes.Ptp, serializer.BuildPdelayResp(build, 3, requester, responseTimestamp))),
        ("Pdelay_Resp_Follow_Up", EthernetFrameBuilder.Build(PtpMulticastAddresses.PeerDelay, srcMac, EtherTypes.Ptp, serializer.BuildPdelayRespFollowUp(build, 3, requester, responseTimestamp)))
    };
}

static PtpFrameValidationResult Validate(string label, byte[] frame, byte domain)
{
    var result = PtpFrameInspector.Inspect(frame, domain);
    var status = result.IsValid ? "PASS" : "FAIL";
    Console.WriteLine($"{status,-4} {label,-22} {result.Summary}");
    return result;
}

static int ReadInt(string[] args, string key, int fallback)
{
    var i = Array.IndexOf(args, key);
    return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out var value) ? value : fallback;
}

static byte ReadByte(string[] args, string key, byte fallback)
{
    var value = ReadInt(args, key, fallback);
    return (byte)Math.Clamp(value, 0, 255);
}

static string ReadString(string[] args, string key, string fallback)
{
    var i = Array.IndexOf(args, key);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
}
