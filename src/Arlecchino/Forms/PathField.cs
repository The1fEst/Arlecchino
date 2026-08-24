using System;
using Arlecchino.Atoms;
using Arlecchino.Navigation;

namespace Arlecchino.Forms;

/// <summary>
/// A path on disk. It leaves the form rather than opening a dialog over it, since the picker is a view of
/// its own, and so it has to be told where to come back to.
/// </summary>
public sealed class PathField : Field<string>
{
    /// <summary>Creates the row.</summary>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="returnView">The view to return to once the picker is done.</param>
    /// <param name="pickFolder">Whether a folder is being chosen rather than a file.</param>
    public PathField(Atom<string> value, ViewRoute returnView, bool pickFolder)
        : base(value)
    {
        ReturnView = returnView;
        PickFolder = pickFolder;
    }

    /// <summary>The view to return to once the picker is done.</summary>
    public ViewRoute ReturnView { get; }

    /// <summary>Whether a folder is being chosen rather than a file.</summary>
    public bool PickFolder { get; }

    /// <summary>
    /// Where the picker opens while the row is still empty, asked for as it opens rather than as the form
    /// is built. A row already holding a path opens there instead.
    /// </summary>
    public Func<string>? Start { get; private set; }

    /// <summary>Says where the picker opens while the row is still empty.</summary>
    /// <param name="start">The folder to open on.</param>
    /// <returns>The same row, to go on building it.</returns>
    public PathField WithStart(Func<string> start)
    {
        Start = start;
        return this;
    }
}
