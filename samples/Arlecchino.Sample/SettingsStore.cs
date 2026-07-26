using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.State;

namespace Arlecchino.Sample;

public sealed class SettingsStore : IArlecchinoStore
{
    public SettingsStore()
    {
        Summary = new(() => $"{Profile.Value} · {Theme.Value} · {Volume.Value}%");
        IsComplete = new(() => Profile.Value.Length > 0 && Theme.Value.Length > 0);
    }

    public Atom<string> Profile { get; } = new TrackedAtom<string>("");

    public Atom<string> Passphrase { get; } = new TrackedAtom<string>("");

    public Atom<string> Theme { get; } = new TrackedAtom<string>("dark");

    public Atom<decimal> Volume { get; } = new TrackedAtom<decimal>(60);

    public Atom<bool> Fullscreen { get; } = new TrackedAtom<bool>(true);

    public Atom<IReadOnlyList<string>> Columns { get; } = new TrackedAtom<IReadOnlyList<string>>(["Name", "Size"]);

    public Atom<DateOnly> Release { get; } = new TrackedAtom<DateOnly>(new(2026, 7, 25));

    public Atom<Rgb> Accent { get; } = new TrackedAtom<Rgb>(new(63, 169, 245));

    public Computed<string> Summary { get; }

    public Computed<bool> IsComplete { get; }
}
