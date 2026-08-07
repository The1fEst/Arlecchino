using System;
using System.Collections.Generic;
using Arlecchino.Rendering;

using Arlecchino.Input;

namespace Arlecchino.Modals.Choosing;

/// <summary>
/// What the single- and multi-choice dialogs share: the options, the typed filter and the cursor.
/// </summary>
public abstract class OptionListModal : Modal
{
    /// <summary>Everything that can be chosen from.</summary>
    public IReadOnlyList<string> Options { get; init; } = [];

    /// <summary>Whatever has been typed to narrow the list. Editing it resets the cursor to the top.</summary>
    public string Filter { get; set; } = "";

    /// <summary>Cursor position within the options that match.</summary>
    public int Index { get; set; }

    /// <summary>Where the rows were drawn last frame, used to turn a click into a row.</summary>
    public SurfaceRegion Rows { get; set; }

    /// <summary>Index of the first option drawn, since a long list only shows a window of it.</summary>
    public int FirstVisible { get; set; }

    /// <summary>The options that pass the filter, in their original order.</summary>
    /// <returns>Matching options; all of them when nothing is typed.</returns>
    public List<string> MatchingOptions()
    {
        if (Filter.Length == 0)
        {
            return [.. Options];
        }

        var matching = new List<string>();
        foreach (var option in Options)
        {
            if (option.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            {
                matching.Add(option);
            }
        }

        return matching;
    }

    /// <summary>Acts on the row that was picked, which is what tells one kind of list from the other.</summary>
    /// <param name="frame">How to close, when picking closes.</param>
    /// <param name="picked">The option.</param>
    protected abstract void Take(ModalFrame frame, string picked);

    /// <summary>
    /// The wheel walks the list, and a click picks the row it landed on. It only takes that row when the row
    /// was already the one under the cursor, so a click never confirms something the eye had not settled on
    /// yet.
    /// </summary>
    /// <param name="frame">How to close.</param>
    /// <param name="mouse">The event that arrived.</param>
    public override void HandleMouse(ModalFrame frame, MouseEvent mouse)
    {
        var matching = MatchingOptions();

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                Index = Math.Max(0, Index - 1);
                return;
            case MouseAction.ScrolledDown:
                Index = Math.Min(Math.Max(0, matching.Count - 1), Index + 1);
                return;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left &&
                Rows.Contains(mouse.Row, mouse.Column):
                Picked(frame, matching, mouse);
                return;
        }
    }

    private void Picked(ModalFrame frame, List<string> matching, MouseEvent mouse)
    {
        var (row, _) = Rows.ToLocal(mouse.Row, mouse.Column);
        var index = FirstVisible + row;

        if (index < 0 || index >= matching.Count)
        {
            return;
        }

        var settled = index == Index;

        Index = index;

        if (settled)
        {
            Take(frame, matching[Math.Clamp(Index, 0, matching.Count - 1)]);
        }
    }
}
