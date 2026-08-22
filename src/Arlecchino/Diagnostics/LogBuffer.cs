using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Diagnostics;

/// <summary>One line of log, kept for the overlay.</summary>
/// <param name="Time">When it was written.</param>
/// <param name="Level">How bad it is.</param>
/// <param name="Category">Where it came from, already shortened to the last part of the name.</param>
/// <param name="Message">What was logged, exception message included.</param>
public sealed record LogEntry(DateTimeOffset Time, LogLevel Level, string Category, string Message);

/// <summary>
/// The last few log lines, held in memory for the overlay to draw, since a terminal application cannot write
/// them to the console. Logging happens on any thread, so the oldest are dropped under a lock.
/// </summary>
public sealed class LogBuffer
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

#if NET9_0_OR_GREATER
    private readonly System.Threading.Lock _trimmingLock = new();
#else
    private readonly object _trimmingLock = new();
#endif

    private readonly Repaint _repaint;

    /// <summary>Creates the buffer.</summary>
    /// <param name="repaint">Asked for a frame when a line arrives, so the overlay stays current.</param>
    public LogBuffer(Repaint repaint)
    {
        _repaint = repaint;
    }

    /// <summary>How many lines to keep before the oldest start falling off the back.</summary>
    public int Capacity { get; set; } = 200;

    /// <summary>How many lines are held. Can change between two reads if something logs meanwhile.</summary>
    public int Count => _entries.Count;

    /// <summary>
    /// The lines held, the oldest first, as they were at this moment. A copy, because anything may be
    /// logging while the overlay draws.
    /// </summary>
    /// <returns>The lines.</returns>
    public IReadOnlyList<LogEntry> Snapshot() => [.. _entries];

    /// <summary>Records a line, dropping the oldest when the buffer is full. Safe from any thread.</summary>
    /// <param name="entry">The line.</param>
    public void Add(LogEntry entry)
    {
        _entries.Enqueue(entry);

        lock (_trimmingLock)
        {
            while (_entries.Count > Capacity && _entries.TryDequeue(out _)) { }
        }

        _repaint.Request();
    }

    /// <summary>Throws away every line held.</summary>
    public void Clear()
    {
        if (_entries.IsEmpty)
        {
            return;
        }

        lock (_trimmingLock)
        {
            while (_entries.TryDequeue(out _)) { }
        }

        _repaint.Request();
    }
}
