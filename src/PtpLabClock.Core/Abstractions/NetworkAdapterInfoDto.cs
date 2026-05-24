// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Core.Abstractions;

public sealed class NetworkAdapterInfoDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsDemo { get; init; }

    public string ModeLabel => IsDemo ? "DEMO" : "RAW";

    public override string ToString() => string.IsNullOrWhiteSpace(Description) ? Name : Description;
}
