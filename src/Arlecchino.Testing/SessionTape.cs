using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Arlecchino.Input;

namespace Arlecchino.Testing;

/// <summary>
/// A session written down: the events that go in, the waits between them, and where a frame is worth looking
/// at. A tape is written by hand rather than recorded.
///
/// <code>
/// var frames = new SessionTape()
///     .Type(":")
///     .Shot()
///     .Type("copy")
///     .Wait(200)
///     .Shot()
///     .Play(host);
///
/// Assert.Contains("Copy files", frames[^1], StringComparison.Ordinal);
/// </code>
/// </summary>
/// <seealso cref="Read"/>
/// <seealso cref="ToString"/>
public sealed class SessionTape
{
    private readonly List<Step> _steps = [];
    private readonly TimeProvider? _clock;

    private DateTimeOffset _last;

    /// <summary>Starts an empty tape, for one written by hand.</summary>
    public SessionTape() { }

    /// <summary>
    /// Starts an empty tape that measures the gaps between events itself, for one captured from an
    /// application as it runs.
    /// </summary>
    /// <param name="clock">
    /// Where the gaps are measured from. It is read as <c>GetUtcNow</c> rather than as a timestamp,
    /// because that is the face the application itself lives by — a tape measured off the
    /// high-frequency timer cannot be replayed against a clock a test moves by hand.
    /// </param>
    public SessionTape(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        _clock = clock;
        _last = clock.GetUtcNow();
    }

    /// <summary>How many steps are on the tape.</summary>
    public int Count => _steps.Count;

    /// <summary>Reads a tape back from what <see cref="ToString"/> wrote.</summary>
    /// <param name="text">The tape as text.</param>
    /// <returns>The tape.</returns>
    public static SessionTape Read(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var tape = new SessionTape();

        foreach (var line in text.Split('\n'))
        {
            var step = line.Trim();

            if (step.Length > 0)
            {
                tape._steps.Add(Step.Parse(step));
            }
        }

        return tape;
    }

    /// <summary>Writes down a key press as the terminal would report it.</summary>
    /// <param name="key">The key.</param>
    /// <param name="modifiers">What was held with it.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape Key(ConsoleKey key, KeyModifiers modifiers = default) =>
        Add(Step.OfKey(Waited(), new(key, modifiers)));

    /// <summary>Writes down text typed one character at a time.</summary>
    /// <param name="text">What was typed.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape Type(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        foreach (var character in text)
        {
            Add(Step.OfKey(Waited(), new(character)));
        }

        return this;
    }

    /// <summary>Writes down a click.</summary>
    /// <param name="row">Row, counted from the top of the terminal.</param>
    /// <param name="column">Column, counted from its left edge.</param>
    /// <param name="button">Which button.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape Click(int row, int column, MouseButton button = MouseButton.Left) =>
        Add(Step.OfMouse(Waited(), new(MouseAction.Pressed, button, row, column, default)));

    /// <summary>Writes down a turn of the wheel.</summary>
    /// <param name="row">Row the pointer was over.</param>
    /// <param name="column">Column the pointer was over.</param>
    /// <param name="down">Whether it turned down.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape Scroll(int row, int column, bool down) =>
        Add(Step.OfMouse(
            Waited(),
            new(down ? MouseAction.ScrolledDown : MouseAction.ScrolledUp, MouseButton.None, row, column, default)));

    /// <summary>Writes down a paste.</summary>
    /// <param name="text">What was pasted.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape Paste(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return Add(Step.OfPaste(Waited(), text));
    }

    /// <summary>Writes down a wait, which is what makes timeouts and work on a clock replayable.</summary>
    /// <param name="milliseconds">How long was waited.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape Wait(int milliseconds) => Add(Step.OfWait(TimeSpan.FromMilliseconds(milliseconds)));

    /// <summary>
    /// Marks that a frame is worth looking at here. Playing the tape hands one back for every mark, so
    /// a tape says not only what happened but where to look.
    /// </summary>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape Shot() => Add(Step.OfFrame(Waited()));

    /// <summary>
    /// Writes down a key exactly as a terminal reports one — character and key together — for a test
    /// that drives the tape from events it built itself rather than from the members above.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape RecordKey(KeyPress key) => Add(Step.OfKey(Waited(), key));

    /// <summary>Writes down a mouse event exactly as a terminal reports one.</summary>
    /// <param name="mouse">The event.</param>
    /// <returns>The tape, so steps chain.</returns>
    public SessionTape RecordMouse(MouseEvent mouse) => Add(Step.OfMouse(Waited(), mouse));

    /// <summary>The tape as text, one step to a line, ready to be written to a file.</summary>
    /// <returns>The tape.</returns>
    public override string ToString()
    {
        var text = new StringBuilder();

        foreach (var step in _steps)
        {
            text.Append(step.Write()).Append('\n');
        }

        return text.ToString();
    }

    /// <summary>
    /// Plays the tape into a host, waiting what it waited and doing what it did, and hands back a frame
    /// for every mark on it.
    /// </summary>
    /// <param name="host">The application to play into.</param>
    /// <returns>One frame per mark, in order.</returns>
    public List<string> Play(ArlecchinoTestHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        var frames = new List<string>();

        foreach (var step in _steps)
        {
            if (step.After > TimeSpan.Zero)
            {
                host.Advance(step.After);
            }

            switch (step.Kind)
            {
                case StepKind.Key:
                    host.Send(step.Key);
                    break;

                case StepKind.Mouse:
                    host.Send(step.Mouse);
                    break;

                case StepKind.Paste:
                    host.SendPaste(step.Text);
                    break;

                case StepKind.Frame:
                    frames.Add(host.Frame());
                    break;
            }
        }

        return frames;
    }

    private SessionTape Add(Step step)
    {
        _steps.Add(step);

        return this;
    }

    private TimeSpan Waited()
    {
        if (_clock is null)
        {
            return TimeSpan.Zero;
        }

        var now = _clock.GetUtcNow();
        var waited = now - _last;

        _last = now;

        return waited > TimeSpan.Zero ? waited : TimeSpan.Zero;
    }

    private enum StepKind : byte
    {
        Key,
        Mouse,
        Paste,
        Frame,
        Wait,
    }

    private readonly record struct Step(
        TimeSpan After,
        StepKind Kind,
        KeyPress Key,
        MouseEvent Mouse,
        string Text)
    {
        public static Step OfKey(TimeSpan after, KeyPress key) => new(after, StepKind.Key, key, default, "");

        public static Step OfMouse(TimeSpan after, MouseEvent mouse) =>
            new(after, StepKind.Mouse, default, mouse, "");

        public static Step OfPaste(TimeSpan after, string text) => new(after, StepKind.Paste, default, default, text);

        public static Step OfFrame(TimeSpan after) => new(after, StepKind.Frame, default, default, "");

        public static Step OfWait(TimeSpan after) => new(after, StepKind.Wait, default, default, "");

        public static Step Parse(string line)
        {
            var parts = line.Split(' ');
            var after = TimeSpan.FromMilliseconds(long.Parse(parts[0], CultureInfo.InvariantCulture));

            return parts[1] switch
            {
                "key" => OfKey(
                    after,
                    new(
                        Enum.Parse<ConsoleKey>(parts[3]),
                        (KeyModifiers)int.Parse(parts[4], CultureInfo.InvariantCulture),
                        (char)int.Parse(parts[2], CultureInfo.InvariantCulture))),
                "mouse" => OfMouse(
                    after,
                    new(
                        Enum.Parse<MouseAction>(parts[2]),
                        Enum.Parse<MouseButton>(parts[3]),
                        int.Parse(parts[4], CultureInfo.InvariantCulture),
                        int.Parse(parts[5], CultureInfo.InvariantCulture),
                        (KeyModifiers)int.Parse(parts[6], CultureInfo.InvariantCulture))),
                "paste" => OfPaste(after, line[(line.IndexOf(" paste ", StringComparison.Ordinal) + 7)..]),
                "frame" => OfFrame(after),
                "wait" => OfWait(after),
                _ => throw new FormatException($"a tape cannot say '{line}'"),
            };
        }

        public string Write()
        {
            var after = ((long)After.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);

            return Kind switch
            {
                StepKind.Key => string.Join(
                    ' ',
                    after,
                    "key",
                    ((int)Key.Character).ToString(CultureInfo.InvariantCulture),
                    Key.Key,
                    ((int)Key.Modifiers).ToString(CultureInfo.InvariantCulture)),
                StepKind.Mouse => string.Join(
                    ' ',
                    after,
                    "mouse",
                    Mouse.Action,
                    Mouse.Button,
                    Mouse.Row.ToString(CultureInfo.InvariantCulture),
                    Mouse.Column.ToString(CultureInfo.InvariantCulture),
                    ((int)Mouse.Modifiers).ToString(CultureInfo.InvariantCulture)),
                StepKind.Paste => $"{after} paste {Text}",
                StepKind.Frame => $"{after} frame",
                _ => $"{after} wait",
            };
        }
    }
}
