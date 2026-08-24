using System;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Collections;
using Arlecchino.Atoms.History;
using Arlecchino.Atoms.History.Collections;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Sample;

public sealed class SettingsStore : IArlecchinoStore
{
    private const string HistoryKey = "Settings";

    public SettingsStore()
    {
        Summary = new(() => $"{Profile.Value} · {Theme.Value} · {Volume.Value}%");
        IsComplete = new(() => Profile.Value.Length > 0 && Theme.Value.Length > 0);
    }

    public Atom<string> Profile { get; } = new HistoryAtom<string>("", $"{HistoryKey}.Profile");

    public Atom<string> Passphrase { get; } = new HistoryAtom<string>("", $"{HistoryKey}.Passphrase");

    public Atom<string> Theme { get; } = new HistoryAtom<string>("dark", $"{HistoryKey}.Theme");

    public Atom<decimal> Volume { get; } = new HistoryAtom<decimal>(60, $"{HistoryKey}.Volume");

    public Atom<bool> Fullscreen { get; } = new HistoryAtom<bool>(true, $"{HistoryKey}.Fullscreen");

    public AtomsList<string> Columns { get; } = new HistoryAtomsList<string>($"{HistoryKey}.Columns", ["Name", "Size"]);

    public Atom<DateOnly> Release { get; } = new HistoryAtom<DateOnly>(new(2026, 7, 25), $"{HistoryKey}.Release");

    public Atom<RGB> Accent { get; } = new HistoryAtom<RGB>(new(63, 169, 245), $"{HistoryKey}.Accent");

    public Computed<string> Summary { get; }

    public Computed<bool> IsComplete { get; }
}
