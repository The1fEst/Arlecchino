using System;
using System.Collections.Generic;
using Arlecchino.Navigation;

namespace Arlecchino.State;

/// <summary>A shortcut in the file picker's sidebar, for somewhere the user goes often.</summary>
/// <param name="Name">What the shortcut is called.</param>
/// <param name="Path">Where it leads.</param>
/// <param name="Icon">An optional glyph drawn before the name.</param>
public sealed record FilePickerPlace(string Name, string Path, string Icon = "");

/// <summary>
/// Everything the file picker needs for one round of picking. Unlike the modals, the picker is a view
/// of its own, so the request also carries where to go once it is done.
/// </summary>
/// <param name="Title">Heading shown above the listing.</param>
/// <param name="PickFolder">Whether a folder is being chosen rather than a file.</param>
/// <param name="InitialPath">Where browsing starts.</param>
/// <param name="ReturnView">The view to return to, whether or not anything was picked.</param>
/// <param name="OnPicked">Called with the full path that was chosen.</param>
public sealed record FilePickerRequest(
    string Title,
    bool PickFolder,
    string InitialPath,
    ViewRoute ReturnView,
    Action<string> OnPicked)
{
    /// <summary>Shortcuts offered in the sidebar.</summary>
    public IReadOnlyList<FilePickerPlace> Places { get; init; } = [];

    /// <summary>
    /// Decides which files are worth showing, by full path. Folders are always listed, since they have
    /// to be walked through to reach anything.
    /// </summary>
    public Func<string, bool>? FileFilter { get; init; }
}
