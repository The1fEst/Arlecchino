![Arlecchino](https://raw.githubusercontent.com/The1fEst/Arlecchino/master/assets/arlecchino-banner.png)

[![NuGet](https://img.shields.io/nuget/v/Arlecchino.Atoms.History?logo=nuget&label=Arlecchino.Atoms.History&color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Atoms.History)
[![Downloads](https://img.shields.io/nuget/dt/Arlecchino.Atoms.History?color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Atoms.History)
[![Build](https://img.shields.io/github/actions/workflow/status/The1fEst/Arlecchino/build.yml?branch=master&logo=github&labelColor=141317)](https://github.com/The1fEst/Arlecchino/actions/workflows/build.yml)
![Target frameworks](https://img.shields.io/badge/net8.0%20%7C%20net10.0-512BD4?logo=dotnet&logoColor=white&labelColor=141317)

Undo and redo for `Arlecchino.Core` atoms.

An atom in `Arlecchino.Core` holds a value, tells what reads it, and forgets what it held. The atoms here
are the same atoms with their changes written down, so an application can walk them back:

```csharp
public sealed class SettingsStore : ArlecchinoAsyncStore
{
    private const string HistoryKey = "Settings";

    public HistoryAtom<string> Server { get; } = new("127.0.0.1", $"{HistoryKey}.Server");
    public HistoryAtom<decimal> Port { get; } = new(40000, $"{HistoryKey}.Port");
    public Atom<string> Filter { get; } = new("");   // not on the stack, and not meant to be
}
```

Turn it on where the container is built:

```csharp
builder.Services.UseHistory(options => options.Resume = true);
```

`AtomHistory` collects the steps, one per application. `HistoryFile` writes them to a file beside the
program, one step per line, thrown away on the way out and again on the way in — unless the application
asks to resume, in which case the file outlives the run: the next one indexes it, adds to it, and every
named atom starts out holding what it last saw that atom hold.

Memory holds the last `Capacity` steps. Older ones live in the file and are seeked to one at a time when
an undo asks for them, so a long history costs an offset and a length per step rather than its values.

The second argument names the atom, and is how a step written by an earlier run finds its atom in this
one. It is written in full rather than worked out from the code around it, so that renaming a class or a
property does not quietly orphan a history that was kept.

A name stands for one piece of state: claiming it again with the same kind of atom takes the slot over,
which is what a screen's store rebuilt on the way back relies on, while claiming it with another kind is
refused.

Nothing here is needed to use atoms, and `Arlecchino` does not reference this package: without
`UseHistory` nothing records a step, and the atoms that could feed a history allocate nothing for one.
