using System;
using Arlecchino.Input;
using Arlecchino.Testing;
using Xunit;

namespace Arlecchino.Tests.Hosting;

/// <summary>
/// What the fake terminal hands over has to be the shape a real console hands over, or every test that
/// feeds it agrees with something no terminal produces. These are the shapes, measured in a real pane:
/// run <c>keys</c> from the tools to take the measurement again.
/// </summary>
public sealed class FakeTerminalTests
{
    private static KeyPress First(string text)
    {
        var terminal = new FakeTerminal(10, 2);
        terminal.EnqueueText(text);

        return terminal.ReadKey();
    }

    [Theory]
    [InlineData("\r", ConsoleKey.Enter)]
    [InlineData("\n", ConsoleKey.Enter)]
    [InlineData("\t", ConsoleKey.Tab)]
    [InlineData("", ConsoleKey.Backspace)]
    [InlineData("\b", ConsoleKey.Backspace)]
    [InlineData("\e", ConsoleKey.Escape)]
    [InlineData(" ", ConsoleKey.Spacebar)]
    public void AControlCharacterCarriesTheKeyAConsoleNamesItBy(string text, ConsoleKey expected)
    {
        var key = First(text);

        Assert.Equal(expected, key.Key);
        Assert.Equal(text[0], key.Character);
        Assert.Equal(default, key.Modifiers);
    }

    [Fact]
    public void ALetterCarriesItsKeyAndSaysWhenItWasShifted()
    {
        var lower = First("a");
        var upper = First("Z");

        Assert.Equal(ConsoleKey.A, lower.Key);
        Assert.Equal(default, lower.Modifiers);
        Assert.Equal(ConsoleKey.Z, upper.Key);
        Assert.Equal(KeyModifiers.Shift, upper.Modifiers);
    }

    [Fact]
    public void ADigitCarriesItsKey()
    {
        Assert.Equal(ConsoleKey.D7, First("7").Key);
    }

    [Fact]
    public void AControlChordCarriesTheLetterAndTheModifier()
    {
        var key = First("");

        Assert.Equal(ConsoleKey.A, key.Key);
        Assert.Equal(KeyModifiers.Control, key.Modifiers);
    }

    [Fact]
    public void SomethingAConsoleHasNoNameForKeepsTheCharacterAlone()
    {
        var key = First("[");

        Assert.Equal(default, key.Key);
        Assert.Equal('[', key.Character);
    }

    /// <summary>
    /// A console may hand Alt and a letter over as one press. A terminal sends two, and the reader is
    /// built to tell that from someone pressing Escape and then typing, so the fake sends two.
    /// </summary>
    [Fact]
    public void AnEscapeAndALetterStayTwoPresses()
    {
        var terminal = new FakeTerminal(10, 2);
        terminal.EnqueueText("\ea");

        Assert.Equal(ConsoleKey.Escape, terminal.ReadKey().Key);
        Assert.Equal(ConsoleKey.A, terminal.ReadKey().Key);
    }

    [Fact]
    public void AnEscapeSequenceStillArrivesCharacterByCharacter()
    {
        var terminal = new FakeTerminal(10, 2);
        terminal.EnqueueText("\e[A");

        Assert.Equal(ConsoleKey.Escape, terminal.ReadKey().Key);
        Assert.Equal('[', terminal.ReadKey().Character);
        Assert.Equal('A', terminal.ReadKey().Character);
    }
}
