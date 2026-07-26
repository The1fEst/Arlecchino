using System;
using System.Collections.Generic;

namespace Arlecchino.Modals;

/// <summary>Any number of options out of a filterable list. Marks survive a change of filter.</summary>
public sealed class MultiChoiceModal : OptionListModal
{
    /// <summary>Options marked so far.</summary>
    public HashSet<string> Selected { get; init; } = new(StringComparer.Ordinal);

    /// <summary>Called with everything marked, in the order of the options.</summary>
    public required Action<IReadOnlyList<string>> OnSubmit { get; init; }

    /// <summary>Whether an option is marked.</summary>
    /// <param name="option">The option to check.</param>
    /// <returns><c>true</c> when it is marked.</returns>
    public bool IsSelected(string option) => Selected.Contains(option);

    /// <summary>Marks an option, or unmarks it when it already was.</summary>
    /// <param name="option">The option to flip.</param>
    public void Toggle(string option)
    {
        if (!Selected.Add(option))
        {
            Selected.Remove(option);
        }
    }

    /// <summary>
    /// What is marked, in the order of the options rather than the order it was clicked in, so the
    /// result does not depend on how the user got there.
    /// </summary>
    /// <returns>The marked options.</returns>
    public List<string> SelectedInOptionOrder()
    {
        var picked = new List<string>();
        foreach (var option in Options)
        {
            if (Selected.Contains(option))
            {
                picked.Add(option);
            }
        }

        return picked;
    }
}
