// SPDX-License-Identifier: Apache-2.0
namespace PtpLabClock.Core.Abstractions;

public interface IAdapterProvider
{
    IReadOnlyList<NetworkAdapterInfoDto> GetAdapters();
}
