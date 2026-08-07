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
    /// An alternative is one press even where the binding is a chord. That is how the same command reaches
    /// a keyboard with the key on it and one without: a laptop spells the chord out, a full keyboard holds
    /// the combination the chord stands in for.
    /// </summary>
    [Fact]
    public void AnAlternativeFiresAChordWithoutTheLeader()
    {
        var binding = new KeyBinding(ConsoleKey.G, KeyModifiers.Control)
            .ThenKey(ConsoleKey.U)
            .AddAlternative(ConsoleKey.PageUp, KeyModifiers.Control);

        Assert.True(binding.Opens(new(ConsoleKey.G, KeyModifiers.Control)));
        Assert.True(binding.Closes(new(ConsoleKey.U)));

        Assert.True(binding.Matches(new(ConsoleKey.PageUp, KeyModifiers.Control)));
        Assert.False(binding.Matches(new(ConsoleKey.G, KeyModifiers.Control)));
        Assert.False(binding.Opens(new(ConsoleKey.PageUp, KeyModifiers.Control)));

        Assert.Equal("Ctrl+G U", binding.ToString());
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

    /// <summary>
    /// An application that draws its own keys along the bottom has the box turned off, and turning it off
    /// is a statement about the keys of the screen rather than about a key half pressed. A leader with
    /// nothing on screen to answer it is a key nobody can press twice.
    /// </summary>
    [Fact]
    public void ALeaderIsAnsweredEvenWhereTheBoxIsTurnedOff()
    {
        using var app = new TestApplication(configure: static builder =>
        {
            builder.Options.ShowHints = false;
            builder.AddCommand<TagCommand>();
        });

        Assert.DoesNotContain("Tag", app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.X, KeyModifiers.Control);

        Assert.Contains("Tag", app.Frame(), StringComparison.Ordinal);
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
