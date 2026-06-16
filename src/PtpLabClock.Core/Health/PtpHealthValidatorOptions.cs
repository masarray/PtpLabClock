// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Health;

public sealed class PtpHealthValidatorOptions
{
    public byte? ExpectedDomain { get; init; }
    public TimeSpan LiveTimeout { get; init; } = TimeSpan.FromSeconds(5);
    public double FollowUpPairWarnRatio { get; init; } = 0.80;
    public double FollowUpPairFailRatio { get; init; } = 0.30;
    public bool RequirePdelayActivity { get; init; } = true;

    public static PtpHealthValidatorOptions ForLabDomain(byte domain) => new()
    {
        ExpectedDomain = domain,
        LiveTimeout = TimeSpan.FromSeconds(5),
        RequirePdelayActivity = true
    };
}
