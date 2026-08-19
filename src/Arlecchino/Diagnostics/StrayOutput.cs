using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Diagnostics;

/// <summary>
/// Stands in front of the console's own writers, so that text written by anything but the drawing is
/// caught rather than landing on a frame. It is logged under <c>stdout</c> or <c>stderr</c> instead.
/// </summary>
internal sealed class StrayOutput
{
    private static StrayOutput? console;

    private Action? _flushLines;
    private LogBuffer? _log;
    private TimeProvider _time = TimeProvider.System;

    private volatile bool _holding;

    private StrayOutput(TextWriter terminal)
    {
        Terminal = terminal;
    }

    /// <summary>
    /// The console's own writer, kept aside when the console was taken over. Frames go here, so that the
    /// drawing reaches the terminal instead of coming back around as a stray line.
    /// </summary>
    public TextWriter Terminal { get; }

    /// <summary>
    /// Whether what is written to the console is being caught. False until there is a log to catch it
    /// into, since text with nowhere to go is better on the screen than gone.
    /// </summary>
    public bool Holding => _holding && _log is not null;

    /// <summary>
    /// Puts a catcher in front of standard output and standard error, before the host is built and a
    /// logging provider takes the writer it will use. Asking twice answers with the same one.
    /// </summary>
    /// <returns>What holds the console, since there is a single console to take over.</returns>
    public static StrayOutput TakeOverTheConsole()
    {
        if (console is { } taken)
        {
            return taken;
        }

        var strays = new StrayOutput(Console.Out);

        Console.SetError(new StrayWriter(strays, Console.Error, "stderr", LogLevel.Warning));
        Console.SetOut(new StrayWriter(strays, Console.Out, "stdout", LogLevel.Information));

        console = strays;

        return strays;
    }

    /// <summary>
    /// Takes a writer's word for how to see off a line it gathered but never finished. Every writer
    /// built against this one says so as it is created.
    /// </summary>
    /// <param name="flush">What sees the unfinished line off, called when the console is given back.</param>
    public void FlushWith(Action flush) => _flushLines += flush;

    /// <summary>Points the catching at a log, which is what makes it start catching at all.</summary>
    /// <param name="log">Where caught lines are kept for the overlay to draw.</param>
    /// <param name="time">
    /// Where the timestamps come from, so a session played back from a tape stamps them as it did when it
    /// was recorded.
    /// </param>
    public void SendTo(LogBuffer log, TimeProvider time)
    {
        _log = log;
        _time = time;
    }

    /// <summary>Starts catching, because a frame is about to be on the screen.</summary>
    public void Hold() => _holding = true;

    /// <summary>
    /// Stops catching and lets what is written through to the console again, which is what a program
    /// lent the terminal, and the shell after the application ends, expect. A line gathered but never
    /// finished is logged rather than dropped.
    /// </summary>
    public void Release()
    {
        _holding = false;

        _flushLines?.Invoke();
    }

    /// <summary>Records one caught line.</summary>
    /// <param name="line">The line, with escape sequences already taken out of it.</param>
    /// <param name="category">Which stream it came off.</param>
    /// <param name="level">How it is colored in the overlay.</param>
    public void Caught(string line, string category, LogLevel level) =>
        _log?.Add(new(_time.GetLocalNow(), level, category, line));
}
