// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Health;

public sealed class PtpHealthCheckResult
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public PtpHealthLevel Level { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;

    public bool IsPass => Level == PtpHealthLevel.Pass;
    public bool IsWarning => Level == PtpHealthLevel.Warn;
    public bool IsFail => Level == PtpHealthLevel.Fail;

    public override string ToString() => $"{Level.ToString().ToUpperInvariant(),-4} {Name}: {Summary}";
}
