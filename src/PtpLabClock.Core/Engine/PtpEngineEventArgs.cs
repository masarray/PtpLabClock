// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Core.Diagnostics;

namespace PtpLabClock.Core.Engine;

public sealed class PtpEngineEventArgs : EventArgs
{
    public PtpEngineEventArgs(PtpEventLogItem item)
    {
        Item = item;
    }

    public PtpEventLogItem Item { get; }
}

public sealed class PtpCountersEventArgs : EventArgs
{
    public PtpCountersEventArgs(PtpRuntimeCounters counters)
    {
        Counters = counters;
    }

    public PtpRuntimeCounters Counters { get; }
}
