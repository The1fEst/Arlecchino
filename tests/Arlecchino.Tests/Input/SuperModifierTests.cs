using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

/// <summary>
/// The key next to the space bar, held to what kitty on a Mac really sent. The sequences were taken off a
/// pty rather than written from the specification.
/// </summary>
public sealed class SuperModifierTests
{
    [Fact]
    public void ACursorKeyHeldWithCommandKeepsTheModifier()
    {
        Assert.True(EscapeSequenceParser.TryParseKey("1;9D", out var key));
        Assert.Equal(ConsoleKey.LeftArrow, key.Key);
        Assert.Equal(KeyModifiers.Super, key.Modifiers);
    }

    [Fact]
    public void ACursorKeyHeldWithAltStillReadsAsAlt()
    {
        Assert.True(EscapeSequenceParser.TryParseKey("1;3D", out var key));
        Assert.Equal(ConsoleKey.LeftArrow, key.Key);
        Assert.Equal(KeyModifiers.Alt, key.Modifiers);
    }

    [Fact]
    public void ALetterHeldWithCommandArrivesAsThatLetter()
    {
        Assert.True(EscapeSequenceParser.TryParseKey("106;9u", out var key));
        Assert.Equal(ConsoleKey.J, key.Key);
        Assert.Equal(KeyModifiers.Super, key.Modifiers);
        Assert.Equal('j', key.Character);
    }

    [Fact]
    public void CommandAndShiftAreBothRead()
    {
        Assert.True(EscapeSequenceParser.TryParseKey("99;10u", out var key));
        Assert.Equal(ConsoleKey.C, key.Key);
        Assert.Equal(KeyModifiers.Super | KeyModifiers.Shift, key.Modifiers);
    }

    /// <summary>
    /// A key coming back up is understood and then dropped. Nothing here asks a terminal for release
    /// events, but one that sent them anyway would otherwise make every key act twice.
    /// </summary>
    [Fact]
    public void AKeyComingBackUpDoesNothing()
    {
        Assert.True(EscapeSequenceParser.TryParseKey("106;9:3u", out var key));
        Assert.True(key.IsNothing);
    }

    /// <summary>
    /// A key with no name here is swallowed rather than replayed. The alternative is worse than losing
    /// the press: the bytes would arrive in whatever is being typed into, as text.
    /// </summary>
    [Fact]
    public void AKeyWithNoNameHereIsNotTypedAsText()
    {
        Assert.True(EscapeSequenceParser.TryParseKey("57399;1u", out var key));
        Assert.True(key.IsNothing);
    }

    [Fact]
    public void ABindingOnCommandMatchesOnlyCommand()
    {
        var binding = new KeyBinding(ConsoleKey.C, KeyModifiers.Super);

        Assert.True(binding.Matches(new(ConsoleKey.C, KeyModifiers.Super)));
        Assert.False(binding.Matches(new(ConsoleKey.C, KeyModifiers.Control)));
        Assert.False(binding.Matches(new(ConsoleKey.C)));
    }

    [Fact]
    public void ABindingReadsAsTheKeyCapTheMachineHas()
    {
        var expected = OperatingSystem.IsMacOS() ? "Cmd+←" : "Win+←";

        Assert.Equal(expected, new KeyBinding(ConsoleKey.LeftArrow, KeyModifiers.Super).ToString());
    }

    [Fact]
    public void ReplacingAModifierRewritesBothCombinationsOfABinding()
    {
        var copy = new KeyBinding(ConsoleKey.Insert, KeyModifiers.Control)
            .AddAlternative(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);

        var moved = copy.Replacing(KeyModifiers.Control, KeyModifiers.Super);

        Assert.Equal(KeyModifiers.Super, moved.Modifiers);
        Assert.Equal(KeyModifiers.Super | KeyModifiers.Shift, Assert.Single(moved.Alternatives).Modifiers);
    }

    [Fact]
    public void ReplacingLeavesBindingsThatDoNotHoldTheModifierAlone()
    {
        var confirm = new KeyBinding(ConsoleKey.Enter);

        Assert.Equal(confirm, confirm.Replacing(KeyModifiers.Alt, KeyModifiers.Super));
    }

    [Fact]
    public void ReplacingTheWholeMapMovesEveryBindingBuiltOnTheModifier()
    {
        var moved = new ArlecchinoKeymap
        {
            Back = new(ConsoleKey.LeftArrow, KeyModifiers.Alt),
            Forward = new(ConsoleKey.RightArrow, KeyModifiers.Alt),
        }.Replacing(KeyModifiers.Alt, KeyModifiers.Super);

        Assert.Equal(KeyModifiers.Super, moved.Back.Modifiers);
        Assert.Equal(KeyModifiers.Super, moved.Forward.Modifiers);
        Assert.Equal(KeyModifiers.Control, moved.ToggleLog.Modifiers);
    }

    [Fact]
    public void TheHistoryKeysAreOnAlt()
    {
        var keymap = new ArlecchinoKeymap();

        Assert.Equal(KeyModifiers.Alt, keymap.Back.Modifiers);
        Assert.Empty(keymap.Back.Alternatives);

        Assert.True(keymap.Back.Matches(new(ConsoleKey.LeftArrow, KeyModifiers.Alt)));
        Assert.False(keymap.Back.Matches(new(ConsoleKey.LeftArrow, KeyModifiers.Super)));
        Assert.False(keymap.Back.Matches(new(ConsoleKey.LeftArrow)));
    }

    [Fact]
    public void TheDefaultMapWalksTheHistoryOnAlt()
    {
        using var app = new TestApplication();

        app.Press(ConsoleKey.O);
        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);

        app.ReadFromTerminal("\e[1;3D");
        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
    }

    /// <summary>
    /// The whole point, end to end: a keymap moved onto Command, and the bytes a Mac terminal really
    /// sends for <c>Cmd+←</c> walking the history.
    /// </summary>
    [Fact]
    public void CommandArrowsWalkTheHistoryOnceTheMapIsMoved()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.UseKeymap(new ArlecchinoKeymap().Replacing(KeyModifiers.Alt, KeyModifiers.Super)));

        app.Press(ConsoleKey.O);
        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);

        app.ReadFromTerminal("\e[1;9D");
        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);

        app.ReadFromTerminal("\e[1;9C");
        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);
    }

    /// <summary>
    /// What the old reader did with a letter held with Command: the sequence was not understood, so it
    /// was replayed a character at a time and landed in the field as text.
    /// </summary>
    [Fact]
    public void ALetterHeldWithCommandIsNotTypedIntoAField()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.Frame();

        app.ReadFromTerminal("\e[106;9u");

        Assert.DoesNotContain("106", app.Frame(), StringComparison.Ordinal);
    }
}
