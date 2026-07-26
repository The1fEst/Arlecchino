using System;
using System.Collections.Generic;

namespace Arlecchino.Hosting;

/// <summary>
/// Work on a clock, run on the frame loop. A terminal application redraws when something asks it to,
/// so anything that changes on its own — a spinner, a clock, a list that refreshes itself, a message
/// that fades — needs someone to say when. That someone is this: schedule an action and it runs
/// between frames, on the same thread as drawing and input, with a repaint asked for afterwards.
///
/// Every schedule returns the handle that cancels it. Hand it to
/// <see cref="Navigation.ViewLifetime.Track"/> and the work stops when the screen goes away.
///
/// Missed time is not made up for: an action runs at most once per pass, so a loop that was held up —
/// a window that came back from being minimised, a long operation, a debugger — resumes with a single
/// run rather than firing everything it slept through.
/// </summary>
public sealed class Ticker
{
    private readonly List<Entry> _entries = [];
    private readonly TimeProvider _time;
    private readonly Repaint _repaint;

    /// <summary>Creates the ticker.</summary>
    /// <param name="time">Where the current time comes from; a test host supplies its own.</param>
    /// <param name="repaint">Asked for a frame after anything runs.</param>
    public Ticker(TimeProvider time, Repaint repaint)
    {
        _time = time;
        _repaint = repaint;
    }

    /// <summary>When the next scheduled action is due, or <c>null</c> when nothing is scheduled.</summary>
    public DateTimeOffset? NextDue
    {
        get
        {
            DateTimeOffset? next = null;

            foreach (var entry in _entries)
            {
                if (next is null || entry.Due < next)
                {
                    next = entry.Due;
                }
            }

            return next;
        }
    }

    /// <summary>Runs an action over and over, waiting the interval between runs.</summary>
    /// <param name="interval">How long to wait each time; anything below a millisecond is raised to one.</param>
    /// <param name="action">What to run.</param>
    /// <returns>Dispose it to stop.</returns>
    public IDisposable Every(TimeSpan interval, Action action) => Schedule(interval, action, repeating: true);

    /// <summary>Runs an action once, after the delay.</summary>
    /// <param name="delay">How long to wait; anything below a millisecond is raised to one.</param>
    /// <param name="action">What to run.</param>
    /// <returns>Dispose it to cancel before it runs.</returns>
    public IDisposable After(TimeSpan delay, Action action) => Schedule(delay, action, repeating: false);

    /// <summary>
    /// Runs whatever is due. Called by the frame loop; a headless host calls it after moving its own
    /// clock forward.
    /// </summary>
    /// <param name="onError">What to do with an action that threw; the rest still run.</param>
    public void Run(Action<Exception> onError)
    {
        if (_entries.Count == 0)
        {
            return;
        }

        var now = _time.GetUtcNow();
        var ran = false;

        foreach (var entry in _entries.ToArray())
        {
            if (entry.IsCancelled || entry.Due > now)
            {
                continue;
            }

            ran = true;

            try
            {
                entry.Action();
            }
            catch (Exception exception)
            {
                onError(exception);
            }

            if (entry.Repeating)
            {
                entry.Due = NextDueAfter(entry, now);
            }
            else
            {
                entry.Cancel();
            }

            if (entry.IsCancelled)
            {
                _entries.Remove(entry);
            }
        }

        if (ran)
        {
            _repaint.Request();
        }
    }

    private static DateTimeOffset NextDueAfter(Entry entry, DateTimeOffset now)
    {
        var next = entry.Due + entry.Interval;

        return next > now ? next : now + entry.Interval;
    }

    private IDisposable Schedule(TimeSpan interval, Action action, bool repeating)
    {
        var step = interval < TimeSpan.FromMilliseconds(1) ? TimeSpan.FromMilliseconds(1) : interval;
        var entry = new Entry(step, _time.GetUtcNow() + step, action, repeating);

        _entries.Add(entry);
        return entry;
    }

    private sealed class Entry : IDisposable
    {
        public Entry(TimeSpan interval, DateTimeOffset due, Action action, bool repeating)
        {
            Interval = interval;
            Due = due;
            Action = action;
            Repeating = repeating;
        }

        public TimeSpan Interval { get; }
        public DateTimeOffset Due { get; set; }
        public Action Action { get; }
        public bool Repeating { get; }
        public bool IsCancelled { get; private set; }

        public void Cancel() => IsCancelled = true;

        public void Dispose() => Cancel();
    }
}
