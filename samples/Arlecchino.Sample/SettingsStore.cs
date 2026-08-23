using System;
using System.Collections.Generic;
using Arlecchino.Rendering.Colors;
using Arlecchino.Atoms;
using Arlecchino.Atoms.History;

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

    public Atom<IReadOnlyList<string>> Columns { get; } =
        new HistoryAtom<IReadOnlyList<string>>(["Name", "Size"], $"{HistoryKey}.Columns");

    public Atom<DateOnly> Release { get; } = new HistoryAtom<DateOnly>(new(2026, 7, 25), $"{HistoryKey}.Release");

    public Atom<Rgb> Accent { get; } = new HistoryAtom<Rgb>(new(63, 169, 245), $"{HistoryKey}.Accent");

    public Computed<string> Summary { get; }

    public Computed<bool> IsComplete { get; }
}
