// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Health;

public sealed class PtpHealthSnapshot
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public PtpHealthLevel OverallLevel { get; init; } = PtpHealthLevel.Info;
    public IReadOnlyList<PtpHealthCheckResult> Checks { get; init; } = Array.Empty<PtpHealthCheckResult>();

    public int PassCount => Checks.Count(c => c.Level == PtpHealthLevel.Pass);
    public int InfoCount => Checks.Count(c => c.Level == PtpHealthLevel.Info);
    public int WarningCount => Checks.Count(c => c.Level == PtpHealthLevel.Warn);
    public int FailCount => Checks.Count(c => c.Level == PtpHealthLevel.Fail);
    public bool HasFailure => FailCount > 0;
    public bool HasWarning => WarningCount > 0;

    public string OverallText => OverallLevel.ToString().ToUpperInvariant();
    public string Summary => $"overall={OverallText} pass={PassCount} warn={WarningCount} fail={FailCount} info={InfoCount}";
}
