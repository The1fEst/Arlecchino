using System;
using System.Collections.Generic;
using Arlecchino.Hosting;

namespace Arlecchino.Diagnostics;

/// <summary>How loud a notification is, which decides how it is coloured.</summary>
public enum NotificationLevel
{
    /// <summary>Something happened and went well.</summary>
    Information,

    /// <summary>Something worth noticing, but nothing failed.</summary>
    Warning,

    /// <summary>Something failed.</summary>
    Failure,
}

/// <summary>One thing the application said, and when it said it.</summary>
/// <param name="Time">When it was raised.</param>
/// <param name="Level">How loud it is.</param>
/// <param name="Text">What it says.</param>
public sealed record Notification(DateTimeOffset Time, NotificationLevel Level, string Text);

/// <summary>
/// What the application has to say, and for how long. The newest line sits on the output row until it
/// times out, so a message does not stay on screen for the rest of the session; it stays in the list
/// for much longer, so opening the notifications screen still shows what went past while the user was
/// looking elsewhere.
///
/// Both timeouts come from <see cref="ArlecchinoOptions"/>, and both are counted by the
/// <see cref="Ticker"/> — nothing here runs on its own thread.
/// </summary>
public sealed class Notifications
{
    private readonly List<Notification> _entries = [];
    private readonly ArlecchinoOptions _options;
    private readonly TimeProvider _time;
    private readonly Repaint _repaint;

    /// <summary>Creates the list.</summary>
    /// <param name="options">Supplies both timeouts.</param>
    /// <param name="time">Where the current time comes from.</param>
    /// <param name="ticker">Counts the timeouts between frames.</param>
    /// <param name="repaint">Asked for a frame whenever something arrives or expires.</param>
    public Notifications(ArlecchinoOptions options, TimeProvider time, Ticker ticker, Repaint repaint)
    {
        _options = options;
        _time = time;
        _repaint = repaint;

        ticker.Every(options.NotificationTimeout, Expire);
    }

    /// <summary>Everything still held, newest first.</summary>
    public IReadOnlyList<Notification> Entries
    {
        get
        {
            var newestFirst = new List<Notification>(_entries);
            newestFirst.Reverse();
            return newestFirst;
        }
    }

    /// <summary>
    /// The line the output row shows, or <c>null</c> once it has timed out. The entry itself stays in
    /// <see cref="Entries"/> until the longer timeout takes it.
    /// </summary>
    public Notification? Current
    {
        get
        {
            if (_entries.Count == 0)
            {
                return null;
            }

            var newest = _entries[^1];
            return _time.GetUtcNow() - newest.Time < _options.NotificationTimeout ? newest : null;
        }
    }

    /// <summary>
    /// How many messages to keep at most, however young they are. A list bounded only by time grows
    /// without limit when something reports in a loop, so the oldest fall off once this many are held.
    /// </summary>
    public int Capacity { get; set; } = 200;

    /// <summary>Says something. The newest line replaces whatever the output row was showing.</summary>
    /// <param name="text">What to say; an empty string clears the row instead.</param>
    /// <param name="level">How loud it is.</param>
    public void Notify(string text, NotificationLevel level = NotificationLevel.Information)
    {
        if (text.Length == 0)
        {
            Clear();
            return;
        }

        _entries.Add(new(_time.GetUtcNow(), level, text));

        var surplus = _entries.Count - Math.Max(1, Capacity);

        if (surplus > 0)
        {
            _entries.RemoveRange(0, surplus);
        }

        _repaint.Request();
    }

    /// <summary>Throws everything away, the output row included.</summary>
    public void Clear()
    {
        if (_entries.Count == 0)
        {
            return;
        }

        _entries.Clear();
        _repaint.Request();
    }

    private void Expire()
    {
        var cutoff = _time.GetUtcNow() - _options.NotificationLifetime;
        var kept = _entries.FindAll(entry => entry.Time > cutoff);

        if (kept.Count == _entries.Count)
        {
            return;
        }

        _entries.Clear();
        _entries.AddRange(kept);
    }
}
