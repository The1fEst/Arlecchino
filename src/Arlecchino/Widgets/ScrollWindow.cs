using System;

namespace Arlecchino.Widgets;

/// <summary>
/// The slice of a long list that fits on screen. Every scrolling widget works this out the same way,
/// so the arithmetic lives here rather than in each of them.
/// </summary>
/// <param name="First">Index of the first item shown.</param>
/// <param name="Count">How many items are shown.</param>
public readonly record struct ScrollWindow(int First, int Count)
{
    /// <summary>Index of the last item shown. Reads as one before <see cref="First"/> when nothing fits.</summary>
    public int Last => First + Count - 1;

    /// <summary>Whether an item is on screen, which is what decides if it needs drawing.</summary>
    /// <param name="index">Index in the full list.</param>
    /// <returns><c>true</c> when the item falls inside the window.</returns>
    public bool Contains(int index) => index >= First && index < First + Count;

    /// <summary>
    /// Places the window so the selection sits in the middle, sliding it back at the ends of the list
    /// so the rows are always filled rather than trailing off into blanks.
    /// </summary>
    /// <param name="selected">Index that has to stay visible.</param>
    /// <param name="itemCount">Length of the full list.</param>
    /// <param name="rows">How many rows there are to draw into.</param>
    /// <returns>The slice to draw; empty when there is nothing to show or nowhere to show it.</returns>
    public static ScrollWindow Around(int selected, int itemCount, int rows)
    {
        if (itemCount <= 0 || rows <= 0)
        {
            return new(0, 0);
        }

        var visible = Math.Min(rows, itemCount);
        var first = Math.Clamp(selected - visible / 2, 0, Math.Max(0, itemCount - visible));

        return new(first, visible);
    }
}
