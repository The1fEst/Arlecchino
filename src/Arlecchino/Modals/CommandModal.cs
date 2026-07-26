using System.Collections.Generic;
using Arlecchino.Rendering;

namespace Arlecchino.Modals;

/// <summary>
/// The list of what can be pressed right now. It is a reminder rather than a menu: the keys keep
/// working while it is open, so a command runs from its own key instead of from a selection.
/// </summary>
public sealed class CommandModal : Modal
{
    /// <summary>The key and label of every command available in this context.</summary>
    public IReadOnlyList<(string Key, string Label)> Commands { get; init; } = [];

    /// <summary>Where the rows were drawn last frame, used to turn a click into a command.</summary>
    public SurfaceRegion Rows { get; set; }
}
