using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.State;

namespace Arlecchino.Sample;

public sealed class SettingsStore : IStore
{
    public SettingsStore()
    {
        Summary = new(() => $"{Profile.Value} · {Theme.Value} · {Volume.Value}%");
        IsComplete = new(() => Profile.Value.Length > 0 && Theme.Value.Length > 0);
    }

    public State<string> Profile { get; } = new TrackedState<string>("");

    public State<string> Passphrase { get; } = new TrackedState<string>("");

    public State<string> Theme { get; } = new TrackedState<string>("dark");

    public State<decimal> Volume { get; } = new TrackedState<decimal>(60);

    public State<bool> Fullscreen { get; } = new TrackedState<bool>(true);

    public State<IReadOnlyList<string>> Columns { get; } = new TrackedState<IReadOnlyList<string>>(["Name", "Size"]);

    public State<DateOnly> Release { get; } = new TrackedState<DateOnly>(new(2026, 7, 25));

    public State<Rgb> Accent { get; } = new TrackedState<Rgb>(new(63, 169, 245));

    public Computed<string> Summary { get; }

    public Computed<bool> IsComplete { get; }
}
