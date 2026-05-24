# WPF Export Report v17

This step adds a compact **Export PDF** action to the WPF control bar.

## Scope

- Visual design remains locked.
- The WPF app references `PtpLabClock.Reporting`.
- `MainViewModel` exposes `ExportReportCommand`.
- `SaveFileDialog` lets the user choose the output PDF path.
- The report includes current adapter/profile/domain metadata, runtime counters, latest timing health snapshot, latest monitor snapshot, and event timeline.

## Notes

QuestPDF is used through the reporting project abstraction. The PDF report remains lab evidence only and is not a timing accuracy certificate.
