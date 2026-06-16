// SPDX-License-Identifier: Apache-2.0
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Engine;

public sealed class PtpProfileDefaultSettings
{
    public required PtpProfilePreset Preset { get; init; }
    public required string DisplayName { get; init; }
    public required string ScopeNote { get; init; }
    public byte DomainNumber { get; init; }
    public byte ClockClass { get; init; } = 248;
    public PtpClockAccuracy ClockAccuracy { get; init; } = PtpClockAccuracy.Unknown;
    public ushort OffsetScaledLogVariance { get; init; } = 0xFFFF;
    public byte Priority1 { get; init; } = 128;
    public byte Priority2 { get; init; } = 128;
    public bool EnableAnnounce { get; init; } = true;
    public bool EnableSync { get; init; } = true;
    public bool EnableFollowUp { get; init; } = true;
    public bool EnablePdelayResponder { get; init; } = true;
    public bool TwoStep { get; init; } = true;
    public int AnnounceIntervalMs { get; init; } = 1000;
    public int SyncIntervalMs { get; init; } = 1000;
    public bool EnableVlan { get; init; }
    public ushort VlanId { get; init; } = 0;
    public byte VlanPriority { get; init; } = 4;

    public PtpEngineOptions CreateOptions()
    {
        return new PtpEngineOptions
        {
            ProfilePreset = Preset,
            DomainNumber = DomainNumber,
            ClockClass = ClockClass,
            ClockAccuracy = ClockAccuracy,
            OffsetScaledLogVariance = OffsetScaledLogVariance,
            Priority1 = Priority1,
            Priority2 = Priority2,
            EnableAnnounce = EnableAnnounce,
            EnableSync = EnableSync,
            EnableFollowUp = EnableFollowUp,
            EnablePdelayResponder = EnablePdelayResponder,
            TwoStep = TwoStep,
            AnnounceIntervalMs = AnnounceIntervalMs,
            SyncIntervalMs = SyncIntervalMs,
            EnableVlan = EnableVlan,
            VlanId = VlanId,
            VlanPriority = VlanPriority
        };
    }
}

public static class PtpProfileDefaults
{
    public static PtpProfileDefaultSettings For(PtpProfilePreset preset)
    {
        return preset switch
        {
            PtpProfilePreset.Iec61850_9_3_Lab => new PtpProfileDefaultSettings
            {
                Preset = preset,
                DisplayName = "IEC 61850-9-3 Lab",
                ScopeNote = "Power-utility PTP visibility profile. Software timestamps only; not a certified grandmaster.",
                DomainNumber = 0,
                ClockClass = 248,
                ClockAccuracy = PtpClockAccuracy.Unknown,
                OffsetScaledLogVariance = 0xFFFF,
                Priority1 = 128,
                Priority2 = 128,
                AnnounceIntervalMs = 1000,
                SyncIntervalMs = 1000,
                EnablePdelayResponder = true,
                TwoStep = true,
                EnableVlan = false,
                VlanPriority = 4
            },
            PtpProfilePreset.AnalyzerTest => new PtpProfileDefaultSettings
            {
                Preset = preset,
                DisplayName = "Analyzer Test",
                ScopeNote = "Safe diagnostic profile for Wireshark/analyzer validation and controlled fault scenarios.",
                DomainNumber = 7,
                ClockClass = 248,
                ClockAccuracy = PtpClockAccuracy.Unknown,
                OffsetScaledLogVariance = 0xFFFF,
                Priority1 = 128,
                Priority2 = 128,
                AnnounceIntervalMs = 1000,
                SyncIntervalMs = 1000,
                EnablePdelayResponder = true,
                TwoStep = true,
                EnableVlan = false,
                VlanPriority = 0
            },
            _ => new PtpProfileDefaultSettings
            {
                Preset = PtpProfilePreset.GenericPtpV2,
                DisplayName = "Generic PTPv2",
                ScopeNote = "Generic IEEE 1588 Layer-2 visibility mode without IEC process-bus claims.",
                DomainNumber = 0,
                ClockClass = 248,
                ClockAccuracy = PtpClockAccuracy.Unknown,
                OffsetScaledLogVariance = 0xFFFF,
                Priority1 = 128,
                Priority2 = 128,
                AnnounceIntervalMs = 2000,
                SyncIntervalMs = 1000,
                EnablePdelayResponder = true,
                TwoStep = true,
                EnableVlan = false,
                VlanPriority = 0
            }
        };
    }
}
