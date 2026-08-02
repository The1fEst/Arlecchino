using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Local;
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

/// <summary>
/// Something the user can do about a notification, offered when the entry is opened: stop the copy
/// that is running, retry what failed, go to what it is about.
/// </summary>
/// <param name="Label">What the action is called; a delegate, so it can be translated.</param>
/// <param name="Run">What it does. The dialog closes first, so this may open one of its own.</param>
public sealed record NotificationAction(Func<string> Label, Action Run);

/// <summary>
/// One thing the application said, and when it said it. A plain message needs no more than the three
/// values it is built with; something still running fills in <see cref="Progress"/>, and something
/// worth reading in full fills in <see cref="Detail"/> and <see cref="Actions"/>, which the
/// notifications screen offers when the entry is opened.
/// </summary>
/// <param name="Time">When it was raised.</param>
/// <param name="Level">How loud it is.</param>
/// <param name="Text">What it says in one line.</param>
public sealed record Notification(DateTimeOffset Time, NotificationLevel Level, string Text)
{
    private NotificationLevel? _ended;

    /// <summary>
    /// The line to show while something is still running, read every frame. Left alone for anything
    /// that is already over, which is most notifications.
    /// </summary>
    public Func<string>? Progress { get; init; }

    /// <summary>
    /// How far along the work is, from <c>0</c> to <c>1</c>, read every frame beside
    /// <see cref="Progress"/>. A bar is drawn for it wherever the entry is shown; work whose size is
    /// not known answers <c>null</c> and gets the text alone.
    /// </summary>
    public Func<double?>? Share { get; init; }

    /// <summary>
    /// The whole story, shown when the entry is opened: the errors a copy collected, the output of a
    /// command, the stack of what failed. Falls back to <see cref="Text"/> when it is not set.
    /// </summary>
    public Func<string>? Detail { get; set; }

    /// <summary>
    /// What can be done about it, offered when the entry is opened. Work that has ended clears these,
    /// since stopping something that is over is not an offer worth making.
    /// </summary>
    public IReadOnlyList<NotificationAction> Actions { get; set; } = [];

    /// <summary>What came of the work, once it ended. Set through <see cref="Notifications.Settle"/>.</summary>
    public string? Ended { get; private set; }

    /// <summary>Whether the work this entry reports is still going.</summary>
    public bool IsRunning => Ended is null && Progress is not null;

    /// <summary>
    /// How loud it is now: what it was raised as, or what it turned out to be once the work it reports
    /// ended — a copy that starts as a plain message and finds three files locked says so.
    /// </summary>
    public NotificationLevel Loudness => _ended ?? Level;

    /// <summary>The single line to draw: what came of it, what is happening now, or what was said.</summary>
    public string Line => Ended ?? Progress?.Invoke() ?? Text;

    /// <summary>
    /// Records what the work came to. The entry itself stays where it is, which is what lets a line
    /// someone is already reading turn from "copying" into what was copied.
    /// </summary>
    /// <param name="text">What came of it.</param>
    /// <param name="level">How loud that is.</param>
    /// <param name="when">The moment it ended, from which its timeouts are counted.</param>
    internal void Complete(string text, NotificationLevel level, DateTimeOffset when)
    {
        Ended = text;
        _ended = level;
        Actions = [];
        Since = when;
    }

    /// <summary>
    /// When the entry last had something new to say — raised, or ended. Work that ran for an hour is
    /// not stale the moment it finishes, so the timeouts are counted from here rather than from
    /// <see cref="Time"/>.
    /// </summary>
    public DateTimeOffset Since { get; private set; } = Time;

    /// <summary>The full text to read: <see cref="Detail"/> when there is one, the line otherwise.</summary>
    /// <returns>What to show in the dialog.</returns>
    public string Whole() => Detail?.Invoke() ?? Line;

    /// <summary>How full a bar for this entry should be, or <c>null</c> when there is nothing to draw.</summary>
    /// <returns>A fraction between <c>0</c> and <c>1</c>.</returns>
    public double? Filled() =>
        IsRunning && Share?.Invoke() is { } share ? Math.Clamp(share, 0, 1) : null;
}

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
    private readonly LocalAtomsList<Notification> _entries = new();
    private readonly ArlecchinoOptions _options;
    private readonly TimeProvider _time;

    /// <summary>Creates the list.</summary>
    /// <param name="options">Supplies both timeouts.</param>
    /// <param name="time">Where the current time comes from.</param>
    /// <param name="ticker">Counts the timeouts between frames.</param>
    public Notifications(ArlecchinoOptions options, TimeProvider time, Ticker ticker)
    {
        _options = options;
        _time = time;

        ticker.Every(options.NotificationTimeout, Expire);
    }

    /// <summary>Everything still held, newest first.</summary>
    public IReadOnlyList<Notification> Entries
    {
        get
        {
            var newestFirst = new List<Notification>(_entries.Value);
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

            var newest = _entries.Value[^1];

            return newest.IsRunning || _time.GetUtcNow() - newest.Since < _options.NotificationTimeout
                ? newest
                : null;
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

        Raise(new(_time.GetUtcNow(), level, text));
    }

    /// <summary>
    /// Says something that carries more than a line — work still running, a report to read in full,
    /// something to do about it. The entry is built by the caller, so it can be kept and taken back
    /// with <see cref="Withdraw"/> once whatever it reports is over.
    /// </summary>
    /// <param name="entry">The notification to raise.</param>
    /// <returns>The same entry, so a caller can hold on to it in one expression.</returns>
    public Notification Raise(Notification entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _entries.Add(entry);

        var surplus = _entries.Count - Math.Max(1, Capacity);

        if (surplus > 0)
        {
            _entries.RemoveRange(0, surplus);
        }

        return entry;
    }

    /// <summary>
    /// Turns a line that was reporting work into what came of that work, in place. The entry keeps its
    /// spot in the list and its identity, so a dialog someone already has open changes under them
    /// rather than going stale, and the entry starts ageing like any other message.
    /// </summary>
    /// <param name="entry">The entry that was reporting.</param>
    /// <param name="text">What came of the work.</param>
    /// <param name="level">How loud that is.</param>
    public void Settle(Notification entry, string text, NotificationLevel level = NotificationLevel.Information)
    {
        ArgumentNullException.ThrowIfNull(entry);

        entry.Complete(text, level, _time.GetUtcNow());
        _entries.Touch();
    }

    /// <summary>
    /// Takes one entry back, for work whose line should not be kept at all.
    /// </summary>
    /// <param name="entry">The entry to remove; one that is no longer held is ignored.</param>
    public void Withdraw(Notification entry) => _entries.Remove(entry);

    /// <summary>
    /// Throws away everything that has been said, the output row included — except work that is still
    /// running, which keeps its line. A copy does not stop because its line was cleared, and a job
    /// running with nothing on screen to show for it is worse than a list that would not empty.
    /// </summary>
    public void Clear() => _entries.Reset(Kept(static entry => entry.IsRunning));

    private void Expire()
    {
        var cutoff = _time.GetUtcNow() - _options.NotificationLifetime;

        _entries.Reset(Kept(entry => entry.IsRunning || entry.Since > cutoff));
    }

    private List<Notification> Kept(Func<Notification, bool> worth)
    {
        var kept = new List<Notification>(_entries.Count);

        foreach (var entry in _entries)
        {
            if (worth(entry))
            {
                kept.Add(entry);
            }
        }

        return kept;
    }
}
