// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Core.Engine;
using PtpLabClock.Pcap.Infrastructure;
using PtpLabClock.Protocol;
using PtpLabClock.Protocol.Enums;
using PtpLabClock.Protocol.Ethernet;
using PtpLabClock.Protocol.Messages;
using PtpLabClock.Protocol.Serialization;
using SharpPcap;

namespace PtpLabClock.Pcap.Diagnostics;

public sealed class NpcapRawSelfTestResult
{
    public bool AdapterOpened { get; init; }
    public bool FilterApplied { get; init; }
    public bool SendSucceeded { get; init; }
    public bool LocalCaptureObserved { get; init; }
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<string> Events { get; init; } = Array.Empty<string>();

    public bool Passed => AdapterOpened && FilterApplied && SendSucceeded;
}

public sealed class NpcapRawSelfTest
{
    private readonly PtpMessageSerializer _serializer = new();

    public async Task<NpcapRawSelfTestResult> RunAsync(
        string adapterId,
        string sourceMac,
        string clockIdentity,
        byte domainNumber,
        bool enableVlan = false,
        ushort vlanId = 100,
        byte vlanPriority = 4,
        CancellationToken cancellationToken = default)
    {
        var events = new List<string>();
        var deviceName = PcapAdapterId.ToDeviceName(adapterId);
        var device = CaptureDeviceList.Instance
            .OfType<ILiveDevice>()
            .FirstOrDefault(d => string.Equals(d.Name, deviceName, StringComparison.OrdinalIgnoreCase));

        if (device is null)
        {
            return Fail(events, "Adapter not found in Npcap device list. Refresh adapters and select a listed RAW adapter.");
        }

        var captureObserved = false;
        var captureTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnPacketArrival(object sender, PacketCapture packetCapture)
        {
            try
            {
                var raw = packetCapture.GetPacket();
                var frame = raw.Data.ToArray();
                var inspection = PtpFrameInspector.Inspect(frame, domainNumber);
                if (!inspection.IsValid)
                    return;

                captureObserved = true;
                events.Add($"LOCAL_CAPTURE: observed {inspection.MessageType} seq={inspection.SequenceId}, {inspection.Transport}, offset={inspection.PtpOffset}.");
                captureTcs.TrySetResult();
            }
            catch
            {
                // Keep self-test capture path robust. The final verdict will still include SEND status.
            }
        }

        var opened = false;
        var filterApplied = false;
        var sendSucceeded = false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            device.OnPacketArrival += OnPacketArrival;
            device.Open(new DeviceConfiguration
            {
                Mode = DeviceModes.Promiscuous,
                Immediate = true,
                ReadTimeout = 1
            });
            opened = true;
            events.Add("OPEN: adapter opened in promiscuous/immediate mode.");

            device.Filter = PtpCaptureFilters.Layer2PtpWithVlan;
            filterApplied = true;
            events.Add("FILTER: VLAN-aware PTP capture filter applied.");

            device.StartCapture();
            events.Add("CAPTURE: capture thread started.");

            var options = new PtpBuildOptions
            {
                DomainNumber = domainNumber,
                ClockIdentity = ClockIdentity.Parse(clockIdentity),
                ClockClass = 248,
                ClockAccuracy = PtpClockAccuracy.Unknown,
                TwoStep = true
            };
            var ptp = _serializer.BuildAnnounce(options, 1);
            var source = MacAddress.Parse(sourceMac);
            var frame = enableVlan
                ? EthernetFrameBuilder.BuildVlan(PtpMulticastAddresses.General, source, vlanId, vlanPriority, EtherTypes.Ptp, ptp)
                : EthernetFrameBuilder.Build(PtpMulticastAddresses.General, source, EtherTypes.Ptp, ptp);

            device.SendPacket(frame);
            sendSucceeded = true;
            events.Add(enableVlan
                ? $"SEND: one VLAN-tagged Announce test frame injected. VLAN={vlanId}, PCP={vlanPriority}."
                : "SEND: one untagged Announce test frame injected.");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var completed = await Task.WhenAny(captureTcs.Task, Task.Delay(TimeSpan.FromMilliseconds(1200), timeoutCts.Token)).ConfigureAwait(false);
            if (completed == captureTcs.Task)
                timeoutCts.Cancel();
            else
                events.Add("LOCAL_CAPTURE: not observed within 1.2 s. This can be normal on adapters/drivers that do not loop back outbound injected packets; verify with Wireshark.");

            var summary = captureObserved
                ? "RAW self-test passed: open, filter, send, and local capture succeeded."
                : "RAW self-test partially passed: open/filter/send succeeded, but local self-capture was not observed. Verify on Wireshark.";

            return new NpcapRawSelfTestResult
            {
                AdapterOpened = opened,
                FilterApplied = filterApplied,
                SendSucceeded = sendSucceeded,
                LocalCaptureObserved = captureObserved,
                Summary = summary,
                Events = events.ToArray()
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            events.Add("ERROR: " + ex.Message);
            return new NpcapRawSelfTestResult
            {
                AdapterOpened = opened,
                FilterApplied = filterApplied,
                SendSucceeded = sendSucceeded,
                LocalCaptureObserved = captureObserved,
                Summary = "RAW self-test failed. Check Npcap installation, administrator rights, adapter type, and driver injection support.",
                Events = events.ToArray()
            };
        }
        finally
        {
            device.OnPacketArrival -= OnPacketArrival;
            try { device.StopCapture(); } catch { }
            try { device.Dispose(); } catch { }
        }
    }

    private static NpcapRawSelfTestResult Fail(List<string> events, string message)
    {
        events.Add("ERROR: " + message);
        return new NpcapRawSelfTestResult
        {
            Summary = message,
            Events = events.ToArray()
        };
    }
}
