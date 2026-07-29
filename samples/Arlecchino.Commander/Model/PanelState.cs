using System;
using System.Collections.Generic;
using Arlecchino.Commander.Files;

namespace Arlecchino.Commander.Model;

public sealed class PanelState
{
    public PanelState(IFileSource source, string folder)
    {
        Source = source;
        Folder = folder;
    }

    public IFileSource Source { get; private set; }

    public string Folder { get; set; }

    public string Cursor { get; set; } = "";

    public string Filter { get; set; } = "";

    public HashSet<string> Marks { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool ShowHidden { get; set; }

    public Sorting Sorting { get; set; }

    public bool Descending { get; set; }

    public void GoTo(string folder)
    {
        Folder = folder;
        Filter = "";
        Marks.Clear();
    }

    public void Connect(IFileSource source, string folder)
    {
        var replaced = Source;

        Source = source;
        GoTo(folder);

        if (replaced.IsRemote)
        {
            replaced.Dispose();
        }
    }
}
