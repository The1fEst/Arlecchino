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
/// The last few log lines, held in memory. A terminal application cannot write logs to the console —
/// they would land in the middle of the frame — so they are collected here instead and shown in an
/// overlay on request. Oldest lines are dropped once the buffer is full.
///
/// Logging happens on whatever thread did the work, so the lines live in a concurrent queue and the
/// overlay draws from a snapshot rather than from the live collection. Dropping the oldest is done
/// under a lock: the check and the removal have to be one step, or two threads trimming at once take
/// the buffer below its capacity.
/// </summary>
public sealed class LogBuffer
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private readonly object _trimming = new();
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
    /// The lines held, oldest first, as they were at this moment. A copy, because anything may be
    /// logging while the overlay draws.
    /// </summary>
    /// <returns>The lines.</returns>
    public IReadOnlyList<LogEntry> Snapshot() => [.. _entries];

    /// <summary>Records a line, dropping the oldest when the buffer is full. Safe from any thread.</summary>
    /// <param name="entry">The line.</param>
    public void Add(LogEntry entry)
    {
        _entries.Enqueue(entry);

        lock (_trimming)
        {
            while (_entries.Count > Capacity && _entries.TryDequeue(out _))
            {
            }
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

        lock (_trimming)
        {
            while (_entries.TryDequeue(out _))
            {
            }
        }

        _repaint.Request();
    }
}
