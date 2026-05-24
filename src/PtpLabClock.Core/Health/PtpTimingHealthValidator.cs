// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Core.Monitor;
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Health;

public sealed class PtpTimingHealthValidator
{
    public PtpHealthSnapshot Evaluate(PtpMonitorSnapshot snapshot, PtpHealthValidatorOptions? options = null)
    {
        options ??= new PtpHealthValidatorOptions();

        var checks = new List<PtpHealthCheckResult>
        {
            CheckVisibility(snapshot),
            CheckDomain(snapshot, options),
            CheckGrandmasterStability(snapshot, options),
            CheckFollowUpPairing(snapshot, options),
            CheckPdelayActivity(snapshot, options),
            CheckSequenceContinuity(snapshot),
        };

        checks.Add(CheckAnalyzerReadiness(checks));

        return new PtpHealthSnapshot
        {
            Timestamp = DateTime.Now,
            Checks = checks,
            OverallLevel = GetOverallLevel(checks)
        };
    }

    private static PtpHealthCheckResult CheckVisibility(PtpMonitorSnapshot snapshot)
    {
        if (snapshot.TotalFrames == 0)
        {
            return Fail("visibility", "PTP Visibility", "No PTP frames detected", "No valid or invalid PTP-like frames have been observed yet.");
        }

        if (snapshot.TotalFrames > 0 && snapshot.ValidFrames == 0)
        {
            return Fail("visibility", "PTP Visibility", "Only invalid frames detected", $"frames={snapshot.TotalFrames}, invalid={snapshot.InvalidFrames}, lastInvalid={snapshot.LastInvalidReason}");
        }

        if (snapshot.InvalidFrames > 0)
        {
            return Warn("visibility", "PTP Visibility", "PTP visible with invalid frame warnings", $"valid={snapshot.ValidFrames}, invalid={snapshot.InvalidFrames}, lastInvalid={snapshot.LastInvalidReason}");
        }

        return Pass("visibility", "PTP Visibility", "Valid PTP traffic detected", $"valid={snapshot.ValidFrames}, domains={snapshot.DetectedDomainCount}, sources={snapshot.Sources.Count}");
    }

    private static PtpHealthCheckResult CheckDomain(PtpMonitorSnapshot snapshot, PtpHealthValidatorOptions options)
    {
        if (snapshot.ValidFrames == 0)
            return Fail("domain", "Domain Match", "No valid domain observed", "No valid PTP frame is available for domain validation.");

        if (!options.ExpectedDomain.HasValue)
        {
            return snapshot.DetectedDomainCount == 1
                ? Pass("domain", "Domain Match", $"Single domain observed: {snapshot.DomainCounters.Keys.First()}", "No expected domain was configured; single-domain traffic is clean.")
                : Warn("domain", "Domain Match", $"Multiple domains observed: {snapshot.DetectedDomainCount}", string.Join(", ", snapshot.DomainCounters.Keys.OrderBy(x => x).Select(x => x.ToString())));
        }

        var expected = options.ExpectedDomain.Value;
        var hasExpected = snapshot.DomainCounters.ContainsKey(expected);
        var unexpected = snapshot.DomainCounters.Keys.Where(d => d != expected).OrderBy(d => d).ToArray();

        if (!hasExpected)
            return Fail("domain", "Domain Match", $"Expected domain {expected} not detected", $"observed={string.Join(", ", snapshot.DomainCounters.Keys.OrderBy(x => x))}");

        if (unexpected.Length > 0)
            return Warn("domain", "Domain Match", $"Expected domain {expected} detected with extra domains", $"unexpected={string.Join(", ", unexpected)}");

        return Pass("domain", "Domain Match", $"Domain {expected} only", "Observed domain matches expected lab configuration.");
    }

    private static PtpHealthCheckResult CheckGrandmasterStability(PtpMonitorSnapshot snapshot, PtpHealthValidatorOptions options)
    {
        if (snapshot.ValidFrames == 0)
            return Fail("gm", "GM Stability", "No grandmaster/source visible", "No valid PTP frame has been observed.");

        var now = snapshot.Timestamp;
        var liveAnnounceSources = snapshot.Sources
            .Where(s => s.AnnounceCount > 0 && s.IsLive(now, options.LiveTimeout))
            .ToArray();

        if (liveAnnounceSources.Length == 0)
        {
            var liveSources = snapshot.Sources.Count(s => s.IsLive(now, options.LiveTimeout));
            return Warn("gm", "GM Stability", "No live Announce source", $"liveSources={liveSources}. Sync/Pdelay may be visible but no Announce source is active.");
        }

        if (liveAnnounceSources.Length > 1)
        {
            return Warn("gm", "GM Stability", $"Multiple live Announce sources: {liveAnnounceSources.Length}", string.Join("; ", liveAnnounceSources.Select(s => $"domain={s.Domain} src={s.ClockIdentity}")));
        }

        var gm = liveAnnounceSources[0];
        return Pass("gm", "GM Stability", "Single live Announce source", $"domain={gm.Domain}, src={gm.ClockIdentity}, announce={gm.AnnounceCount}");
    }

    private static PtpHealthCheckResult CheckFollowUpPairing(PtpMonitorSnapshot snapshot, PtpHealthValidatorOptions options)
    {
        var syncCount = Count(snapshot, PtpMessageType.Sync);
        var followUpCount = Count(snapshot, PtpMessageType.FollowUp);

        if (syncCount == 0 && followUpCount == 0)
            return Warn("followup", "Follow_Up Pairing", "No Sync/Follow_Up observed", "Two-step pairing cannot be validated yet.");

        if (syncCount == 0 && followUpCount > 0)
            return Warn("followup", "Follow_Up Pairing", "Follow_Up seen without Sync", $"sync={syncCount}, followUp={followUpCount}");

        var ratio = syncCount == 0 ? 0 : (double)followUpCount / syncCount;
        if (ratio >= options.FollowUpPairWarnRatio)
            return Pass("followup", "Follow_Up Pairing", "Sync/Follow_Up pairing looks healthy", $"sync={syncCount}, followUp={followUpCount}, ratio={ratio:0.00}");

        if (ratio <= options.FollowUpPairFailRatio)
            return Fail("followup", "Follow_Up Pairing", "Follow_Up missing for most Sync messages", $"sync={syncCount}, followUp={followUpCount}, ratio={ratio:0.00}");

        return Warn("followup", "Follow_Up Pairing", "Follow_Up pairing is incomplete", $"sync={syncCount}, followUp={followUpCount}, ratio={ratio:0.00}");
    }

    private static PtpHealthCheckResult CheckPdelayActivity(PtpMonitorSnapshot snapshot, PtpHealthValidatorOptions options)
    {
        var pdelayReq = Count(snapshot, PtpMessageType.PdelayReq);
        var pdelayResp = Count(snapshot, PtpMessageType.PdelayResp);
        var pdelayFollowUp = Count(snapshot, PtpMessageType.PdelayRespFollowUp);
        var total = pdelayReq + pdelayResp + pdelayFollowUp;

        if (total == 0)
        {
            return options.RequirePdelayActivity
                ? Warn("pdelay", "Pdelay Activity", "No peer-delay exchange observed", "IEC 61850-9-3 style P2P behavior is not visible yet.")
                : Info("pdelay", "Pdelay Activity", "No peer-delay exchange observed", "Pdelay activity was not required by validator options.");
        }

        if (pdelayReq > 0 && pdelayResp == 0)
            return Warn("pdelay", "Pdelay Activity", "Pdelay_Req observed without response", $"req={pdelayReq}, resp={pdelayResp}, respFU={pdelayFollowUp}");

        if (pdelayResp > 0 && pdelayFollowUp == 0)
            return Warn("pdelay", "Pdelay Activity", "Pdelay response visible but follow-up missing", $"req={pdelayReq}, resp={pdelayResp}, respFU={pdelayFollowUp}");

        return Pass("pdelay", "Pdelay Activity", "Peer-delay activity visible", $"req={pdelayReq}, resp={pdelayResp}, respFU={pdelayFollowUp}");
    }

    private static PtpHealthCheckResult CheckSequenceContinuity(PtpMonitorSnapshot snapshot)
    {
        if (snapshot.ValidFrames == 0)
            return Fail("sequence", "Sequence Continuity", "No valid sequence data", "No valid PTP frames have been observed.");

        var anomalyCount = snapshot.Sources.Sum(s => s.SequenceAnomalyCount);
        if (anomalyCount == 0)
            return Pass("sequence", "Sequence Continuity", "No simple sequence anomaly detected", "Lightweight check only; validates adjacent same-message sequence progression per source.");

        return Warn("sequence", "Sequence Continuity", $"Sequence anomaly count={anomalyCount}", "Adjacent same-message sequence jump or duplicate detected. Review event timeline / Wireshark.");
    }

    private static PtpHealthCheckResult CheckAnalyzerReadiness(IReadOnlyList<PtpHealthCheckResult> checks)
    {
        var required = checks.Where(c => c.Key is "visibility" or "domain" or "gm" or "followup" or "sequence").ToArray();
        if (required.Any(c => c.Level == PtpHealthLevel.Fail))
            return Fail("readiness", "Analyzer Readiness", "Not ready", "One or more critical PTP checks failed.");

        if (required.Any(c => c.Level == PtpHealthLevel.Warn))
            return Warn("readiness", "Analyzer Readiness", "Partially ready", "PTP traffic is visible, but some health warnings remain.");

        return Pass("readiness", "Analyzer Readiness", "Ready for analyzer validation", "Visibility, domain, GM, Follow_Up, and sequence checks are healthy.");
    }

    private static long Count(PtpMonitorSnapshot snapshot, PtpMessageType messageType)
    {
        return snapshot.MessageCounters.TryGetValue(messageType, out var value) ? value : 0;
    }

    private static PtpHealthLevel GetOverallLevel(IReadOnlyList<PtpHealthCheckResult> checks)
    {
        if (checks.Any(c => c.Level == PtpHealthLevel.Fail))
            return PtpHealthLevel.Fail;
        if (checks.Any(c => c.Level == PtpHealthLevel.Warn))
            return PtpHealthLevel.Warn;
        if (checks.Any(c => c.Level == PtpHealthLevel.Pass))
            return PtpHealthLevel.Pass;
        return PtpHealthLevel.Info;
    }

    private static PtpHealthCheckResult Pass(string key, string name, string summary, string detail) => New(key, name, PtpHealthLevel.Pass, summary, detail);
    private static PtpHealthCheckResult Info(string key, string name, string summary, string detail) => New(key, name, PtpHealthLevel.Info, summary, detail);
    private static PtpHealthCheckResult Warn(string key, string name, string summary, string detail) => New(key, name, PtpHealthLevel.Warn, summary, detail);
    private static PtpHealthCheckResult Fail(string key, string name, string summary, string detail) => New(key, name, PtpHealthLevel.Fail, summary, detail);

    private static PtpHealthCheckResult New(string key, string name, PtpHealthLevel level, string summary, string detail) => new()
    {
        Key = key,
        Name = name,
        Level = level,
        Summary = summary,
        Detail = detail
    };
}
