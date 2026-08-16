using System;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Text;

/// <summary>
/// A line being typed into, cut into the runs it is drawn as: what is plain, what is selected, and the
/// symbol the caret stands on. The caret is written the other way round rather than wedged beside it.
/// </summary>
public static class EntryRuns
{
    private const string PastTheEnd = " ";

    /// <summary>Hands every run of the line to whoever is drawing it, left to right.</summary>
    /// <param name="text">The line as it is written, which for a secret is the dots.</param>
    /// <param name="caret">Where the caret is in it, or <c>-1</c> for a line that is not being typed into.</param>
    /// <param name="selection">Where the selection starts and ends in it.</param>
    /// <param name="look">The colors to write it in.</param>
    /// <param name="write">Takes one run: what it says, and how it is written.</param>
    public static void Of(
        string text,
        int caret,
        (int Start, int End) selection,
        EntryLook look,
        Action<string, IArlecchinoColor> write)
    {
        var start = Math.Clamp(selection.Start, 0, text.Length);
        var end = Math.Clamp(selection.End, start, text.Length);
        var stands = caret < 0 ? -1 : Math.Clamp(caret, 0, text.Length);

        Part(text[..start], 0, stands, look, look.Text, write);
        Part(text[start..end], start, stands, look, look.Selected, write);
        Part(text[end..], end, stands, look, look.Text, write);

        if (stands == text.Length)
        {
            write(PastTheEnd, look.Caret);
        }
    }

    /// <summary>
    /// One stretch of the line, split where the caret falls inside it. The symbol under the caret is taken
    /// whole: half of a surrogate pair written the other way round would be the character neither of them is.
    /// </summary>
    /// <param name="part">What the stretch says.</param>
    /// <param name="from">Where it starts in the whole line.</param>
    /// <param name="caret">Where the caret is in the whole line, or <c>-1</c> when there is none.</param>
    /// <param name="look">The colors to write it in.</param>
    /// <param name="style">How this stretch is written where the caret is not on it.</param>
    /// <param name="write">Takes one run.</param>
    private static void Part(
        string part,
        int from,
        int caret,
        EntryLook look,
        IArlecchinoColor style,
        Action<string, IArlecchinoColor> write)
    {
        var at = caret - from;

        if (part.Length == 0)
        {
            return;
        }

        if (at < 0 || at >= part.Length)
        {
            write(part, style);

            return;
        }

        var under = TextWidth.NextClusterEnd(part, at);

        write(part[..at], style);
        write(part[at..under], look.Caret);
        write(part[under..], style);
    }
}
