using System;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Text;

/// <summary>
/// The slice of a line that fits beside the caret. A line longer than the room it is drawn in is scrolled
/// rather than cut off, so the caret stays on screen wherever it is in the text.
/// </summary>
internal static class LineWindow
{
    private const int ForCaretAndMarkers = 3;

    /// <summary>Cuts the line down to what fits around the caret.</summary>
    /// <param name="text">The whole line.</param>
    /// <param name="caret">Where the caret is in it.</param>
    /// <param name="room">How many columns there are.</param>
    /// <returns>What goes before the caret and what goes after.</returns>
    public static (string Before, string After) Around(string text, int caret, int room)
    {
        var before = text[..caret];
        var after = text[caret..];

        if (TextWidth.Of(text) < room)
        {
            return (before, after);
        }

        var visible = Math.Max(1, room - ForCaretAndMarkers);
        var trailing = TextWidth.Truncate(after, visible / 2);

        return (TextWidth.TruncateStart(before, visible - TextWidth.Of(trailing)), trailing);
    }
}
