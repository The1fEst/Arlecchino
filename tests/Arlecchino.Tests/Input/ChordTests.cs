using System;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.State;
using Arlecchino.Tests.Support;
using Xunit;

namespace Arlecchino.Tests.Input;

/// <summary>
/// Bindings of two keystrokes, and the alternatives a binding can carry. Both are what the builder is
/// for: a binding is the combination it is named after, and everything else is added to it.
/// </summary>
public sealed class ChordTests
{
    [Fact]
    public void AChordIsNeitherOfItsHalvesOnItsOwn()
    {
        var binding = new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T);

        Assert.True(binding.IsChord);
        Assert.False(binding.Matches(new(ConsoleKey.X, KeyModifiers.Control)));
        Assert.False(binding.Matches(new(ConsoleKey.T)));

        Assert.True(binding.Opens(new(ConsoleKey.X, KeyModifiers.Control)));
        Assert.True(binding.Closes(new(ConsoleKey.T)));
    }

    [Fact]
    public void AKeyOfOneStrokeOpensNothing()
    {
        var binding = new KeyBinding(ConsoleKey.S, KeyModifiers.Control);

        Assert.False(binding.IsChord);
        Assert.False(binding.Opens(new(ConsoleKey.S, KeyModifiers.Control)));
        Assert.False(binding.Closes(new(ConsoleKey.S, KeyModifiers.Control)));
    }

    [Fact]
    public void EveryAlternativeAddedIsMatched()
    {
        var binding = new KeyBinding(ConsoleKey.Insert, KeyModifiers.Control)
            .AddAlternative(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift)
            .AddAlternative(ConsoleKey.C, KeyModifiers.Super);

        Assert.True(binding.Matches(new(ConsoleKey.Insert, KeyModifiers.Control)));
        Assert.True(binding.Matches(new(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift)));
        Assert.True(binding.Matches(new(ConsoleKey.C, KeyModifiers.Super)));
        Assert.False(binding.Matches(new(ConsoleKey.C, KeyModifiers.Control)));
    }

    /// <summary>
    /// A leader with alternatives is opened by any of them, which is what lets a chord be reached from a
    /// keyboard that sends one modifier and from one that sends another.
    /// </summary>
    [Fact]
    public void AnAlternativeOpensTheChordToo()
    {
        var binding = KeyBinding.AltOrSuper(ConsoleKey.X).ThenKey(ConsoleKey.T);

        Assert.True(binding.Opens(new(ConsoleKey.X, KeyModifiers.Alt)));
        Assert.True(binding.Opens(new(ConsoleKey.X, KeyModifiers.Super)));
    }

    [Fact]
    public void BindingsBuiltTheSameWayAreTheSameBinding()
    {
        var one = new KeyBinding(ConsoleKey.X, KeyModifiers.Control)
            .AddAlternative(ConsoleKey.X, KeyModifiers.Alt)
            .ThenKey(ConsoleKey.T);

        var other = new KeyBinding(ConsoleKey.X, KeyModifiers.Control)
            .AddAlternative(ConsoleKey.X, KeyModifiers.Alt)
            .ThenKey(ConsoleKey.T);

        Assert.Equal(one, other);
        Assert.Equal(one.GetHashCode(), other.GetHashCode());

        Assert.NotEqual(one, other.ThenKey(ConsoleKey.D));
        Assert.NotEqual(one, new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T));
    }

    [Fact]
    public void ReplacingAModifierRewritesBothHalvesOfAChord()
    {
        var moved = new KeyBinding(ConsoleKey.X, KeyModifiers.Alt)
            .AddAlternative(ConsoleKey.Y, KeyModifiers.Alt)
            .ThenKey(ConsoleKey.T, KeyModifiers.Alt)
            .Replacing(KeyModifiers.Alt, KeyModifiers.Super);

        Assert.Equal(KeyModifiers.Super, moved.Modifiers);
        Assert.Equal(KeyModifiers.Super, Assert.Single(moved.Alternatives).Modifiers);
        Assert.Equal(KeyModifiers.Super, moved.Second?.Modifiers);
    }

    [Fact]
    public void AChordReadsAsItsTwoKeystrokes()
    {
        var binding = new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T);

        Assert.Equal("Ctrl+X T", binding.ToString());
    }

    [Fact]
    public void AChordRunsOnTheKeyThatFinishesIt()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<TagCommand>());

        app.Press(ConsoleKey.X, KeyModifiers.Control);
        Assert.Equal("", app.State.Output);

        app.Press(ConsoleKey.T);
        Assert.Equal("tagged", app.State.Output);
    }

    /// <summary>
    /// The second key belongs to the chord and to nothing else. Were it let through, a leader followed
    /// by a key somebody meant as itself would run two things, and the one they wanted second.
    /// </summary>
    [Fact]
    public void TheKeyAfterALeaderReachesNothingElse()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.AddCommand<TagCommand>().AddCommand<SaveCommand>());

        app.Press(ConsoleKey.X, KeyModifiers.Control);
        app.Press(ConsoleKey.S, KeyModifiers.Control);

        Assert.Equal("", app.State.Output);

        app.Press(ConsoleKey.S, KeyModifiers.Control);
        Assert.Equal("saved", app.State.Output);
    }

    /// <summary>
    /// What the leader is worth: the box stops listing the keys that are out of reach and lists the ones
    /// that finish the chord instead, so the second key is read rather than remembered.
    /// </summary>
    [Fact]
    public void TheHintsBoxListsWhatTheLeaderHasBehindIt()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.AddCommand<TagCommand>().AddCommand<TrashCommand>());

        app.Press(ConsoleKey.X, KeyModifiers.Control);

        var frame = app.Frame();

        Assert.Contains("Tag", frame, StringComparison.Ordinal);
        Assert.Contains("Trash", frame, StringComparison.Ordinal);
    }
}

public sealed class TagCommand : IArlecchinoCommand
{
    private readonly ArlecchinoState _state;

    public TagCommand(ArlecchinoState state)
    {
        _state = state;
    }

    public KeyBinding Binding => new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T);

    public string Icon => "▪";

    public string Label => "Tag";

    public ViewRoute Execute()
    {
        _state.Output = "tagged";

        return ViewRoute.None;
    }
}

public sealed class TrashCommand : IArlecchinoCommand
{
    private readonly ArlecchinoState _state;

    public TrashCommand(ArlecchinoState state)
    {
        _state = state;
    }

    public KeyBinding Binding => new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.D);

    public string Icon => "▪";

    public string Label => "Trash";

    public ViewRoute Execute()
    {
        _state.Output = "trashed";

        return ViewRoute.None;
    }
}
