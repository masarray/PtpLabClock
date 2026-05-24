// SPDX-License-Identifier: GPL-3.0-or-later
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PtpLabClock.Core.Diagnostics;
using PtpLabClock.Core.Health;
using PtpLabClock.Core.Monitor;

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

        QuestPDF.Settings.License = LicenseType.Community;
        new PtpSessionReportDocument(data).GeneratePdf(outputPath);
    }
}

internal sealed class PtpSessionReportDocument : IDocument
{
    private readonly PtpSessionReportData _data;

    public PtpSessionReportDocument(PtpSessionReportData data) => _data = data;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(TextStyle.Default.FontFamily("Segoe UI").FontSize(9.5f).FontColor("#1E293B"));

            page.Header().Element(ComposeHeader);
            page.Content().PaddingTop(14).Column(column =>
            {
                column.Spacing(12);
                column.Item().Element(ComposeExecutiveSummary);
                column.Item().Element(ComposeHealthSection);
                column.Item().Element(ComposeCounterSection);
                column.Item().Element(ComposeMonitorSection);
                column.Item().Element(ComposeEventTimeline);
                column.Item().Element(ComposeScopeNote);
            });
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Process Bus Timing Lab • ").FontColor("#64748B");
                text.CurrentPageNumber().FontColor("#64748B");
                text.Span(" / ").FontColor("#64748B");
                text.TotalPages().FontColor("#64748B");
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.BorderBottom(1).BorderColor("#E2E8F0").PaddingBottom(12).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(_data.Title).FontSize(19).SemiBold().FontColor("#0F172A");
                col.Item().PaddingTop(3).Text(_data.Subtitle).FontSize(10).FontColor("#64748B");
            });

            row.ConstantItem(150).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text("SESSION REPORT").FontSize(8).SemiBold().FontColor("#2563EB");
                col.Item().AlignRight().PaddingTop(4).Text(_data.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss")).FontSize(8.5f).FontColor("#64748B");
            });
        });
    }

    private void ComposeExecutiveSummary(IContainer container)
    {
        container.Element(Card).Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("Executive Summary").FontSize(13).SemiBold().FontColor("#0F172A");
            col.Item().Grid(grid =>
            {
                grid.Columns(3);
                grid.Spacing(8);
                grid.Item().Element(c => InfoTile(c, "Project", _data.ProjectName));
                grid.Item().Element(c => InfoTile(c, "Adapter", Safe(_data.AdapterName, "Not specified")));
                grid.Item().Element(c => InfoTile(c, "Mode", _data.Mode));
                grid.Item().Element(c => InfoTile(c, "Profile", Safe(_data.ProfileName, "Not specified")));
                grid.Item().Element(c => InfoTile(c, "Domain", _data.DomainNumber.ToString()));
                grid.Item().Element(c => InfoTile(c, "Wireshark", _data.WiresharkFilter));
            });
        });
    }

    private void ComposeHealthSection(IContainer container)
    {
        var health = _data.HealthSnapshot;
        container.Element(Card).Column(col =>
        {
            col.Spacing(8);
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Timing Health Validator").FontSize(13).SemiBold().FontColor("#0F172A");
                row.ConstantItem(112).Element(c => StatusPill(c, health?.OverallLevel ?? PtpHealthLevel.Info, health?.OverallText ?? "WAITING"));
            });

            if (health is null || health.Checks.Count == 0)
            {
                col.Item().Text("No health snapshot available yet.").FontColor("#64748B");
                return;
            }

            col.Item().Grid(grid =>
            {
                grid.Columns(2);
                grid.Spacing(7);
                foreach (var check in health.Checks)
                    grid.Item().Element(c => HealthCard(c, check));
            });
        });
    }

    private void ComposeCounterSection(IContainer container)
    {
        var c = _data.Counters;
        container.Element(Card).Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("PTP Runtime Counters").FontSize(13).SemiBold().FontColor("#0F172A");
            col.Item().Grid(grid =>
            {
                grid.Columns(4);
                grid.Spacing(7);
                grid.Item().Element(x => Metric(x, "Announce TX", c.AnnounceTx.ToString()));
                grid.Item().Element(x => Metric(x, "Sync TX", c.SyncTx.ToString()));
                grid.Item().Element(x => Metric(x, "Follow_Up TX", c.FollowUpTx.ToString()));
                grid.Item().Element(x => Metric(x, "Pdelay Req RX", c.PdelayReqRx.ToString()));
                grid.Item().Element(x => Metric(x, "Pdelay Resp TX", c.PdelayRespTx.ToString()));
                grid.Item().Element(x => Metric(x, "Pdelay FU TX", c.PdelayRespFollowUpTx.ToString()));
                grid.Item().Element(x => Metric(x, "Errors", c.PacketErrors.ToString()));
                grid.Item().Element(x => Metric(x, "Last Sync Seq", c.LastSyncSeq.ToString()));
            });
        });
    }

    private void ComposeMonitorSection(IContainer container)
    {
        var snapshot = _data.MonitorSnapshot;
        container.Element(Card).Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("Passive Monitor Snapshot").FontSize(13).SemiBold().FontColor("#0F172A");

            if (snapshot is null)
            {
                col.Item().Text("No passive monitor snapshot available.").FontColor("#64748B");
                return;
            }

            col.Item().Grid(grid =>
            {
                grid.Columns(4);
                grid.Spacing(7);
                grid.Item().Element(x => Metric(x, "Total Frames", snapshot.TotalFrames.ToString()));
                grid.Item().Element(x => Metric(x, "Valid Frames", snapshot.ValidFrames.ToString()));
                grid.Item().Element(x => Metric(x, "Invalid Frames", snapshot.InvalidFrames.ToString()));
                grid.Item().Element(x => Metric(x, "Live Sources", snapshot.LiveSourceCount.ToString()));
            });

            if (snapshot.Sources.Count > 0)
            {
                col.Item().PaddingTop(2).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(2.4f);
                        columns.RelativeColumn(0.9f);
                        columns.RelativeColumn(0.9f);
                        columns.RelativeColumn(0.9f);
                        columns.RelativeColumn(1.2f);
                    });

                    HeaderCell(table, "Domain");
                    HeaderCell(table, "Clock Identity");
                    HeaderCell(table, "Ann");
                    HeaderCell(table, "Sync");
                    HeaderCell(table, "FU");
                    HeaderCell(table, "Last");

                    foreach (var source in snapshot.Sources.Take(10))
                    {
                        BodyCell(table, source.Domain.ToString());
                        BodyCell(table, source.ClockIdentity);
                        BodyCell(table, source.AnnounceCount.ToString());
                        BodyCell(table, source.SyncCount.ToString());
                        BodyCell(table, source.FollowUpCount.ToString());
                        BodyCell(table, source.LastMessageType.ToString());
                    }
                });
            }
        });
    }

    private void ComposeEventTimeline(IContainer container)
    {
        container.Element(Card).Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("Event Timeline").FontSize(13).SemiBold().FontColor("#0F172A");

            var events = _data.Events.Take(24).ToArray();
            if (events.Length == 0)
            {
                col.Item().Text("No event timeline items captured.").FontColor("#64748B");
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(72);
                    columns.ConstantColumn(46);
                    columns.ConstantColumn(62);
                    columns.RelativeColumn();
                });

                HeaderCell(table, "Time");
                HeaderCell(table, "Level");
                HeaderCell(table, "Source");
                HeaderCell(table, "Message");

                foreach (var item in events)
                {
                    BodyCell(table, item.Timestamp.ToString("HH:mm:ss.fff"));
                    BodyCell(table, item.Severity);
                    BodyCell(table, item.Source);
                    BodyCell(table, item.Message);
                }
            });
        });
    }

    private void ComposeScopeNote(IContainer container)
    {
        container.Background("#FFF7ED").Border(1).BorderColor("#FED7AA").Padding(10).Column(col =>
        {
            col.Item().Text("Scope and Safety Note").FontSize(10).SemiBold().FontColor("#9A3412");
            col.Item().PaddingTop(4).Text(_data.LabDisclaimer).FontSize(9).FontColor("#9A3412");
        });
    }

    private static IContainer Card(IContainer container) => container.Background("#FFFFFF").Border(1).BorderColor("#E2E8F0").Padding(12);

    private static void InfoTile(IContainer container, string label, string value)
    {
        container.Background("#F8FAFC").Border(1).BorderColor("#E2E8F0").Padding(9).Column(col =>
        {
            col.Item().Text(label.ToUpperInvariant()).FontSize(7.5f).SemiBold().FontColor("#64748B");
            col.Item().PaddingTop(3).Text(value).FontSize(9.2f).FontColor("#0F172A");
        });
    }

    private static void Metric(IContainer container, string label, string value)
    {
        container.Background("#F8FAFC").Border(1).BorderColor("#E2E8F0").Padding(9).Column(col =>
        {
            col.Item().Text(label.ToUpperInvariant()).FontSize(7.5f).SemiBold().FontColor("#64748B");
            col.Item().PaddingTop(4).Text(value).FontSize(16).SemiBold().FontColor("#0F172A");
        });
    }

    private static void HealthCard(IContainer container, PtpHealthCheckResult check)
    {
        container.Background(LevelBackground(check.Level)).Border(1).BorderColor(LevelBorder(check.Level)).Padding(9).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(check.Name).FontSize(9.3f).SemiBold().FontColor("#0F172A");
                row.ConstantItem(52).Element(c => StatusPill(c, check.Level, check.Level.ToString().ToUpperInvariant()));
            });
            col.Item().PaddingTop(4).Text(check.Summary).FontSize(8.8f).FontColor("#334155");
        });
    }

    private static void StatusPill(IContainer container, PtpHealthLevel level, string text)
    {
        container.AlignRight().Background(LevelPill(level)).PaddingVertical(3).PaddingHorizontal(7).Text(text).FontSize(7.5f).SemiBold().FontColor(LevelText(level));
    }

    private static void HeaderCell(TableDescriptor table, string text)
    {
        table.Cell().Background("#EEF2F7").PaddingVertical(5).PaddingHorizontal(6).Text(text).FontSize(7.5f).SemiBold().FontColor("#475569");
    }

    private static void BodyCell(TableDescriptor table, string text)
    {
        table.Cell().BorderBottom(1).BorderColor("#EEF2F7").PaddingVertical(5).PaddingHorizontal(6).Text(Safe(text, "-")).FontSize(8.2f).FontColor("#1E293B");
    }

    private static string LevelBackground(PtpHealthLevel level) => level switch
    {
        PtpHealthLevel.Pass => "#ECFDF5",
        PtpHealthLevel.Warn => "#FFFBEB",
        PtpHealthLevel.Fail => "#FFF1F2",
        _ => "#F8FAFC"
    };

    private static string LevelBorder(PtpHealthLevel level) => level switch
    {
        PtpHealthLevel.Pass => "#BBF7D0",
        PtpHealthLevel.Warn => "#FDE68A",
        PtpHealthLevel.Fail => "#FECDD3",
        _ => "#E2E8F0"
    };

    private static string LevelPill(PtpHealthLevel level) => level switch
    {
        PtpHealthLevel.Pass => "#D1FAE5",
        PtpHealthLevel.Warn => "#FEF3C7",
        PtpHealthLevel.Fail => "#FFE4E6",
        _ => "#E2E8F0"
    };

    private static string LevelText(PtpHealthLevel level) => level switch
    {
        PtpHealthLevel.Pass => "#047857",
        PtpHealthLevel.Warn => "#B45309",
        PtpHealthLevel.Fail => "#BE123C",
        _ => "#475569"
    };

    private static string Safe(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
