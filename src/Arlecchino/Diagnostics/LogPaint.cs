using System;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Diagnostics;

/// <summary>
/// The log, drawn over the bottom half of whatever is on screen. It is pinned to the newest line
/// unless it has been scrolled back, so a line arriving while it is open is seen without anything
/// having to be done about it.
/// </summary>
internal sealed class LogPaint
{
    private const int LeastRows = 4;

    private readonly Surface _surface;
    private readonly ArlecchinoStrings _strings;

    /// <summary>Draws the overlay.</summary>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="strings">The words the application says things in.</param>
    public LogPaint(Surface surface, ArlecchinoStrings strings)
    {
        _surface = surface;
        _strings = strings;
    }

    /// <summary>Draws it.</summary>
    /// <param name="log">Which lines, and where it is scrolled to.</param>
    public void Draw(LogOverlay log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var entries = log.Buffer.Snapshot();
        var height = Math.Clamp(_surface.FrameHeight / 2, LeastRows, Math.Max(LeastRows, _surface.FrameHeight - 2));
        var box = _surface.Frame.Rows(_surface.FrameHeight - height, height);
        var inside = box.Border(Theme.Info, _strings.LogTitle(entries.Count)).Inset(new Margin(1, 0, 1, 0));
        var rows = Math.Max(0, inside.Height - 1);

        if (entries.Count == 0)
        {
            inside.WriteLine(0, _strings.LogEmpty(), Theme.Muted);
        }

        log.Scroll = Math.Min(log.Scroll, Math.Max(0, entries.Count - rows));

        var last = entries.Count - log.Scroll;
        var first = Math.Max(0, last - rows);

        for (var row = 0; first + row < last; row++)
        {
            var entry = entries[first + row];
            var line = $"{entry.Time:HH:mm:ss} {Name(entry.Level)} {entry.Category}: {entry.Message}";

            inside.Write(
                row,
                0,
                TextWidth.PadRight(TextWidth.Truncate(line, inside.Width), inside.Width),
                Style(entry.Level));
        }

        inside.WriteLine(inside.Height - 1, TextWidth.Truncate(_strings.LogHints(), inside.Width), Theme.Muted);
    }

    private static string Name(LogLevel level) => level switch
    {
        LogLevel.Trace => "trce",
        LogLevel.Debug => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "fail",
        LogLevel.Critical => "crit",
        _ => "none",
    };

    private static TermColor Style(LogLevel level) => level switch
    {
        LogLevel.Warning => Theme.Warning,
        LogLevel.Error or LogLevel.Critical => Theme.Error,
        LogLevel.Trace or LogLevel.Debug => Theme.Muted,
        _ => Theme.Default,
    };
}
