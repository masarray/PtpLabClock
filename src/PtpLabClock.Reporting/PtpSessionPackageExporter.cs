// SPDX-License-Identifier: GPL-3.0-or-later
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using PtpLabClock.Core.Diagnostics;

namespace PtpLabClock.Reporting;

public static class PtpSessionPackageExporter
{
    public static void ExportZip(PtpSessionReportData data, string outputZipPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (string.IsNullOrWhiteSpace(outputZipPath))
            throw new ArgumentException("Output ZIP path is required.", nameof(outputZipPath));

        var fullPath = Path.GetFullPath(outputZipPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        var tempDir = Path.Combine(Path.GetTempPath(), "PtpLabClockSession_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var pdfPath = Path.Combine(tempDir, "report.pdf");
            var jsonPath = Path.Combine(tempDir, "session.json");
            var eventsPath = Path.Combine(tempDir, "events.csv");
            var readmePath = Path.Combine(tempDir, "README.txt");
            var metadataPath = Path.Combine(tempDir, "metadata.json");

            PtpSessionReportGenerator.GeneratePdf(data, pdfPath);
            File.WriteAllText(jsonPath, BuildSessionJson(data), Encoding.UTF8);
            File.WriteAllText(eventsPath, BuildEventsCsv(data.Events), Encoding.UTF8);
            File.WriteAllText(readmePath, BuildReadme(data), Encoding.UTF8);
            File.WriteAllText(metadataPath, BuildMetadataJson(data), Encoding.UTF8);

            using var archive = ZipFile.Open(fullPath, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(pdfPath, "report.pdf", CompressionLevel.Optimal);
            archive.CreateEntryFromFile(jsonPath, "session.json", CompressionLevel.Optimal);
            archive.CreateEntryFromFile(eventsPath, "events.csv", CompressionLevel.Optimal);
            archive.CreateEntryFromFile(readmePath, "README.txt", CompressionLevel.Optimal);
            archive.CreateEntryFromFile(metadataPath, "metadata.json", CompressionLevel.Optimal);
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    private static string BuildSessionJson(PtpSessionReportData data)
    {
        var payload = new
        {
            generatedAt = data.GeneratedAt,
            sessionStartedAt = data.SessionStartedAt,
            sessionEndedAt = data.SessionEndedAt,
            title = data.Title,
            subtitle = data.Subtitle,
            projectName = data.ProjectName,
            operatorName = data.OperatorName,
            adapterName = data.AdapterName,
            profileName = data.ProfileName,
            domainNumber = data.DomainNumber,
            mode = data.Mode,
            wiresharkFilter = data.WiresharkFilter,
            counters = data.Counters,
            health = data.HealthSnapshot is null ? null : new
            {
                overall = data.HealthSnapshot.OverallLevel.ToString(),
                summary = data.HealthSnapshot.Summary,
                pass = data.HealthSnapshot.PassCount,
                warning = data.HealthSnapshot.WarningCount,
                fail = data.HealthSnapshot.FailCount,
                checks = data.HealthSnapshot.Checks.Select(x => new
                {
                    name = x.Name,
                    level = x.Level.ToString(),
                    summary = x.Summary,
                    detail = x.Detail
                }).ToArray()
            },
            monitor = data.MonitorSnapshot is null ? null : new
            {
                summary = data.MonitorSnapshot.Summary,
                totalFrames = data.MonitorSnapshot.TotalFrames,
                validFrames = data.MonitorSnapshot.ValidFrames,
                invalidFrames = data.MonitorSnapshot.InvalidFrames,
                detectedDomains = data.MonitorSnapshot.DetectedDomainCount,
                liveSources = data.MonitorSnapshot.LiveSourceCount,
                sources = data.MonitorSnapshot.Sources.Select(x => new
                {
                    domain = x.Domain,
                    clockIdentity = x.ClockIdentity,
                    announce = x.AnnounceCount,
                    sync = x.SyncCount,
                    followUp = x.FollowUpCount,
                    pdelayReq = x.PdelayReqCount,
                    pdelayResp = x.PdelayRespCount,
                    sequenceWarnings = x.SequenceAnomalyCount,
                    lastSeen = x.LastSeen,
                    lastMessageType = x.LastMessageType.ToString(),
                    lastSequenceId = x.LastSequenceId
                }).ToArray()
            },
            events = data.Events.Select(e => new
            {
                timestamp = e.Timestamp,
                severity = e.Severity,
                source = e.Source,
                message = e.Message
            }).ToArray(),
            disclaimer = data.LabDisclaimer
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string BuildMetadataJson(PtpSessionReportData data)
    {
        var payload = new
        {
            packageType = "ProcessBusTimingLab.SessionEvidencePackage",
            packageVersion = "1.0",
            generatedAt = DateTime.Now,
            reportFile = "report.pdf",
            sessionJson = "session.json",
            eventsCsv = "events.csv",
            recommendedWiresharkFilter = data.WiresharkFilter,
            safetyScope = "Lab simulator / diagnostic evidence only. Not a GPS/PTP grandmaster replacement."
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string BuildEventsCsv(IReadOnlyList<PtpEventLogItem> events)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Severity,Source,Message");
        foreach (var e in events)
        {
            sb.Append(Csv(e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"))).Append(',')
              .Append(Csv(e.Severity)).Append(',')
              .Append(Csv(e.Source)).Append(',')
              .Append(Csv(e.Message)).AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildReadme(PtpSessionReportData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Process Bus Timing Lab - Session Evidence Package");
        sb.AppendLine("=================================================");
        sb.AppendLine();
        sb.AppendLine($"Generated : {data.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Project   : {data.ProjectName}");
        sb.AppendLine($"Adapter   : {data.AdapterName}");
        sb.AppendLine($"Profile   : {data.ProfileName}");
        sb.AppendLine($"Domain    : {data.DomainNumber}");
        sb.AppendLine($"Mode      : {data.Mode}");
        sb.AppendLine();
        sb.AppendLine("Files");
        sb.AppendLine("-----");
        sb.AppendLine("report.pdf     - formatted engineering report");
        sb.AppendLine("session.json   - machine-readable session snapshot");
        sb.AppendLine("events.csv     - event timeline export");
        sb.AppendLine("metadata.json  - package metadata and recommended Wireshark filter");
        sb.AppendLine();
        sb.AppendLine("Recommended Wireshark filter");
        sb.AppendLine("----------------------------");
        sb.AppendLine(data.WiresharkFilter);
        sb.AppendLine();
        sb.AppendLine("Scope note");
        sb.AppendLine("----------");
        sb.AppendLine(data.LabDisclaimer);
        return sb.ToString();
    }

    private static string Csv(string? value)
    {
        value ??= string.Empty;
        return '"' + value.Replace("\"", "\"\"") + '"';
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }
}
