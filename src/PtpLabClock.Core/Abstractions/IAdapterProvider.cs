// SPDX-License-Identifier: GPL-3.0-or-later
namespace PtpLabClock.Core.Abstractions;

public interface IAdapterProvider
{
    IReadOnlyList<NetworkAdapterInfoDto> GetAdapters();
}
