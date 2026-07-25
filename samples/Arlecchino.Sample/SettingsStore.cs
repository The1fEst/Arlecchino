using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.State;

namespace Arlecchino.Sample;

public sealed class SettingsStore
{
    public SettingsStore()
    {
        Summary = new(() => $"{Profile.Value} · {Theme.Value} · {Volume.Value}%");
        IsComplete = new(() => Profile.Value.Length > 0 && Theme.Value.Length > 0);
    }

    public State<string> Profile { get; } = new("");

    public State<string> Passphrase { get; } = new("");

    public State<string> Theme { get; } = new("dark");

    public State<decimal> Volume { get; } = new(60);

    public State<bool> Fullscreen { get; } = new(true);

    public State<IReadOnlyList<string>> Columns { get; } = new(["Name", "Size"]);

    public State<DateOnly> Release { get; } = new(new(2026, 7, 25));

    public State<Rgb> Accent { get; } = new(new(63, 169, 245));

    public Computed<string> Summary { get; }

    public Computed<bool> IsComplete { get; }
}
