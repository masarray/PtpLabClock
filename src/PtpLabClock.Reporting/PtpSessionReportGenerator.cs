// SPDX-License-Identifier: Apache-2.0
using System.Globalization;
using System.Text;
using PtpLabClock.Core.Health;

namespace PtpLabClock.Reporting;

public static class PtpSessionReportGenerator
{
    public static void GeneratePdf(PtpSessionReportData data, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output PDF path is required.", nameof(outputPath));

        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        SimplePdfWriter.Write(outputPath, BuildReportLines(data));
    }

    private static IReadOnlyList<string> BuildReportLines(PtpSessionReportData data)
    {
        var lines = new List<string>
        {
            data.Title,
            data.Subtitle,
            $"Generated : {data.GeneratedAt:yyyy-MM-dd HH:mm:ss}",
            string.Empty,
            "EXECUTIVE SUMMARY",
            $"Project   : {Safe(data.ProjectName, "Lab Validation")}",
            $"Operator  : {Safe(data.OperatorName, "Not specified")}",
            $"Adapter   : {Safe(data.AdapterName, "Not specified")}",
            $"Mode      : {Safe(data.Mode, "Monitor")}",
            $"Profile   : {Safe(data.ProfileName, "Not specified")}",
            $"Domain    : {data.DomainNumber}",
            $"Started   : {data.SessionStartedAt:yyyy-MM-dd HH:mm:ss}",
            $"Ended     : {data.SessionEndedAt:yyyy-MM-dd HH:mm:ss}",
            $"Filter    : {data.WiresharkFilter}",
            string.Empty,
            "RUNTIME COUNTERS",
            $"Announce TX             : {data.Counters.AnnounceTx}",
            $"Sync TX                 : {data.Counters.SyncTx}",
            $"Follow_Up TX            : {data.Counters.FollowUpTx}",
            $"Pdelay Req RX           : {data.Counters.PdelayReqRx}",
            $"Pdelay Resp TX          : {data.Counters.PdelayRespTx}",
            $"Pdelay Resp Follow_Up TX: {data.Counters.PdelayRespFollowUpTx}",
            $"Packet Errors           : {data.Counters.PacketErrors}",
            $"Last TX                 : {Safe(data.Counters.LastTxSummary, "none")}",
            string.Empty
        };

        AppendHealth(lines, data);
        AppendMonitor(lines, data);
        AppendEvents(lines, data);

        lines.Add(string.Empty);
        lines.Add("SCOPE NOTE");
        lines.Add(WrapLine(data.LabDisclaimer, 94));
        lines.Add("This report documents lab visibility and diagnostic evidence only. It is not a timing accuracy certificate.");
        return lines;
    }

    private static void AppendHealth(List<string> lines, PtpSessionReportData data)
    {
        lines.Add("TIMING HEALTH");
        if (data.HealthSnapshot is null)
        {
            lines.Add("No health snapshot available.");
            lines.Add(string.Empty);
            return;
        }

        lines.Add($"Overall : {data.HealthSnapshot.OverallText} ({data.HealthSnapshot.Summary})");
        lines.Add($"Checks  : pass={data.HealthSnapshot.PassCount}, warn={data.HealthSnapshot.WarningCount}, fail={data.HealthSnapshot.FailCount}");
        foreach (var check in data.HealthSnapshot.Checks.Take(12))
        {
            var prefix = check.Level == PtpHealthLevel.Fail ? "FAIL" : check.Level == PtpHealthLevel.Warn ? "WARN" : check.Level.ToString().ToUpperInvariant();
            lines.Add($"- {prefix,-5} {check.Name}: {check.Summary}");
        }
        lines.Add(string.Empty);
    }

    private static void AppendMonitor(List<string> lines, PtpSessionReportData data)
    {
        lines.Add("PASSIVE MONITOR SNAPSHOT");
        if (data.MonitorSnapshot is null)
        {
            lines.Add("No passive monitor snapshot available.");
            lines.Add(string.Empty);
            return;
        }

        var snapshot = data.MonitorSnapshot;
        lines.Add(snapshot.Summary);
        lines.Add($"Frames: total={snapshot.TotalFrames}, valid={snapshot.ValidFrames}, invalid={snapshot.InvalidFrames}, domains={snapshot.DetectedDomainCount}, liveSources={snapshot.LiveSourceCount}");

        foreach (var source in snapshot.Sources.Take(12))
        {
            lines.Add($"- domain={source.Domain} src={source.ClockIdentity} Ann={source.AnnounceCount} Sync={source.SyncCount} FU={source.FollowUpCount} PdelayReq={source.PdelayReqCount} SeqWarn={source.SequenceAnomalyCount} Last={source.LastMessageType}/{source.LastSequenceId}");
        }
        lines.Add(string.Empty);
    }

    private static void AppendEvents(List<string> lines, PtpSessionReportData data)
    {
        lines.Add("EVENT TIMELINE");
        var events = data.Events.Take(30).ToArray();
        if (events.Length == 0)
        {
            lines.Add("No event timeline items captured.");
            lines.Add(string.Empty);
            return;
        }

        foreach (var item in events)
            lines.Add($"{item.Timestamp:HH:mm:ss.fff} {item.Severity,-5} {item.Source,-10} {item.Message}");

        lines.Add(string.Empty);
    }

    private static string Safe(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string WrapLine(string text, int width)
    {
        if (text.Length <= width) return text;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var current = new StringBuilder();
        foreach (var word in words)
        {
            if (current.Length + word.Length + 1 > width)
            {
                lines.Add(current.ToString());
                current.Clear();
            }
            if (current.Length > 0) current.Append(' ');
            current.Append(word);
        }
        if (current.Length > 0) lines.Add(current.ToString());
        return string.Join(Environment.NewLine, lines);
    }
}

internal static class SimplePdfWriter
{
    private const double PageWidth = 595.28;
    private const double PageHeight = 841.89;
    private const double MarginLeft = 36;
    private const double StartY = 805;
    private const int MaxLinesPerPage = 52;

    public static void Write(string outputPath, IReadOnlyList<string> lines)
    {
        var pages = lines.Count == 0
            ? new[] { Array.Empty<string>() }
            : lines.Select(Sanitize).Chunk(MaxLinesPerPage).Select(chunk => chunk.ToArray()).ToArray();

        var objects = new List<string>
        {
            string.Empty,
            "<< /Type /Catalog /Pages 2 0 R >>",
            string.Empty,
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        var pageObjectNumbers = new List<int>();
        foreach (var pageLines in pages)
        {
            var pageObjectNumber = objects.Count;
            var contentObjectNumber = pageObjectNumber + 1;
            pageObjectNumbers.Add(pageObjectNumber);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth.ToString(CultureInfo.InvariantCulture)} {PageHeight.ToString(CultureInfo.InvariantCulture)}] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
            var content = BuildContentStream(pageLines);
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream");
        }

        objects[2] = $"<< /Type /Pages /Kids [{string.Join(' ', pageObjectNumbers.Select(n => $"{n} 0 R"))}] /Count {pageObjectNumbers.Count} >>";

        var bytes = new List<byte>();
        AppendAscii(bytes, "%PDF-1.4\n%\u00E2\u00E3\u00CF\u00D3\n");
        var offsets = new List<int> { 0 };

        for (var i = 1; i < objects.Count; i++)
        {
            offsets.Add(bytes.Count);
            AppendAscii(bytes, $"{i} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xrefOffset = bytes.Count;
        AppendAscii(bytes, $"xref\n0 {objects.Count}\n");
        AppendAscii(bytes, "0000000000 65535 f \n");
        for (var i = 1; i < objects.Count; i++)
            AppendAscii(bytes, $"{offsets[i]:D10} 00000 n \n");

        AppendAscii(bytes, $"trailer\n<< /Size {objects.Count} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        File.WriteAllBytes(outputPath, bytes.ToArray());
    }

    private static string BuildContentStream(IReadOnlyList<string> lines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BT");
        sb.AppendLine("/F1 10 Tf");
        sb.AppendLine($"{MarginLeft.ToString(CultureInfo.InvariantCulture)} {StartY.ToString(CultureInfo.InvariantCulture)} Td");
        foreach (var line in lines)
        {
            sb.Append('(').Append(Escape(line)).AppendLine(") Tj");
            sb.AppendLine("0 -14 Td");
        }
        sb.Append("ET");
        return sb.ToString();
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static string Sanitize(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
            sb.Append(ch is >= ' ' and <= '~' ? ch : '-');
        return sb.ToString();
    }

    private static void AppendAscii(List<byte> target, string text) => target.AddRange(Encoding.ASCII.GetBytes(text));
}
