using System;
using System.Collections.Generic;
using Arlecchino.Rendering.Colors;
using Arlecchino.Atoms;
using Arlecchino.Atoms.History;

namespace Arlecchino.Sample;

public sealed class SettingsStore : IArlecchinoStore
{
    public SettingsStore()
    {
        Summary = new(() => $"{Profile.Value} · {Theme.Value} · {Volume.Value}%");
        IsComplete = new(() => Profile.Value.Length > 0 && Theme.Value.Length > 0);
    }

    public Atom<string> Profile { get; } = new HistoryAtom<string>("", "SettingsStore.Profile");

    public Atom<string> Passphrase { get; } = new HistoryAtom<string>("");

    public Atom<string> Theme { get; } = new HistoryAtom<string>("dark", "SettingsStore.Theme");

    public Atom<decimal> Volume { get; } = new HistoryAtom<decimal>(60, "SettingsStore.Volume");

    public Atom<bool> Fullscreen { get; } = new HistoryAtom<bool>(true, "SettingsStore.Fullscreen");

    public Atom<IReadOnlyList<string>> Columns { get; } = new HistoryAtom<IReadOnlyList<string>>(["Name", "Size"]);

    public Atom<DateOnly> Release { get; } = new HistoryAtom<DateOnly>(new(2026, 7, 25), "SettingsStore.Release");

    public Atom<Rgb> Accent { get; } = new HistoryAtom<Rgb>(new(63, 169, 245));

    public Computed<string> Summary { get; }

    public Computed<bool> IsComplete { get; }
}
