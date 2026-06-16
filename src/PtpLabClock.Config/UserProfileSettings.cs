// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Config;

public sealed class UserProfileSettings
{
    public string LastAdapterId { get; set; } = string.Empty;
    public string LastSourceMac { get; set; } = "02-00-00-00-00-01";
    public string LastClockIdentity { get; set; } = "02-00-00-FF-FE-00-00-01";
    public byte LastDomain { get; set; } = 0;
}
