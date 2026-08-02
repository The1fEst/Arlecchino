using System;
using System.Collections.Generic;
using Arlecchino.Rendering;

namespace Arlecchino.Modals.Choosing;

/// <summary>
/// What the single- and multi-choice dialogs share: the options, the typed filter and the cursor.
/// </summary>
public abstract class OptionListModal : Modal
{
    /// <summary>Everything that can be chosen from.</summary>
    public IReadOnlyList<string> Options { get; init; } = [];

    /// <summary>What has been typed to narrow the list. Editing it resets the cursor to the top.</summary>
    public string Filter { get; set; } = "";

    /// <summary>Cursor position within the options that currently match.</summary>
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
}
