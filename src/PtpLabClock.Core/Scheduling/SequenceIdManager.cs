// SPDX-License-Identifier: GPL-3.0-or-later
using PtpLabClock.Protocol.Enums;

namespace PtpLabClock.Core.Scheduling;

public sealed class SequenceIdManager
{
    private readonly object _gate = new();
    private readonly Dictionary<PtpMessageType, ushort> _values = new();

    public ushort Next(PtpMessageType type)
    {
        lock (_gate)
        {
            _values.TryGetValue(type, out var current);
            current++;
            if (current == 0) current = 1;
            _values[type] = current;
            return current;
        }
    }

    public void Jump(PtpMessageType type, ushort delta)
    {
        lock (_gate)
        {
            _values.TryGetValue(type, out var current);
            _values[type] = (ushort)(current + delta);
        }
    }

    public ushort Current(PtpMessageType type)
    {
        lock (_gate)
        {
            _values.TryGetValue(type, out var current);
            return current;
        }
    }
}
