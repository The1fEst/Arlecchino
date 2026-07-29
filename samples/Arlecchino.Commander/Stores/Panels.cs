using System;
using System.IO;
using Arlecchino.Atoms;
using Arlecchino.Commander.Files;
using Arlecchino.Commander.Model;

namespace Arlecchino.Commander.Stores;

public sealed class Panels : IArlecchinoStore, IDisposable
{
    public Panels()
    {
        Left = new(new LocalSource(), Directory.GetCurrentDirectory());
        Right = new(new LocalSource(), Listing.Home());
    }

    public PanelState Left { get; }

    public PanelState Right { get; }

    public Atom<bool> RightIsActive { get; } = new LocalAtom<bool>(false);

    public Atom<string> Viewing { get; } = new LocalAtom<string>("");

    public IFileSource ViewingSource { get; set; } = new LocalSource();

    public long ViewingSize { get; set; }

    public void Dispose()
    {
        Left.Source.Dispose();
        Right.Source.Dispose();
    }

    public void Start(string left, string right)
    {
        if (Directory.Exists(left))
        {
            Left.GoTo(Path.GetFullPath(left));
        }

        if (Directory.Exists(right))
        {
            Right.GoTo(Path.GetFullPath(right));
        }
    }
}
