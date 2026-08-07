using System;
using Arlecchino.Input;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class KeyBindingFallbackTests
{
    private static KeyPress CharacterOnly(char character) => new(character);

    [Fact]
    public void LettersMatchWhenTheTerminalReportsNoVirtualKey()
    {
        var binding = new KeyBinding(ConsoleKey.N);

        Assert.True(binding.Matches(CharacterOnly('n')));
        Assert.True(binding.Matches(CharacterOnly('N')));
        Assert.False(binding.Matches(CharacterOnly('m')));
    }

    [Fact]
    public void DigitsAndControlKeysMatchByCharacter()
    {
        Assert.True(new KeyBinding(ConsoleKey.D5).Matches(CharacterOnly('5')));
        Assert.True(new KeyBinding(ConsoleKey.Enter).Matches(CharacterOnly('\r')));
        Assert.True(new KeyBinding(ConsoleKey.Escape).Matches(CharacterOnly('\e')));
        Assert.True(new KeyBinding(ConsoleKey.Tab).Matches(CharacterOnly('\t')));
        Assert.True(new KeyBinding(ConsoleKey.Spacebar).Matches(CharacterOnly(' ')));
    }

    [Fact]
    public void ModifiersAreStillCompared()
    {
        var binding = new KeyBinding(ConsoleKey.S, KeyModifiers.Control);

        Assert.False(binding.Matches(CharacterOnly('s')));
        Assert.True(binding.Matches(new(default, KeyModifiers.Control, 's')));
    }

    [Fact]
    public void ArrowsAreNotGuessedFromCharacters()
    {
        Assert.False(new KeyBinding(ConsoleKey.UpArrow).Matches(CharacterOnly('A')));
    }

    [Fact]
    public void CommandsRunWhenOnlyTheCharacterArrives()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<ProbeCommand>());

        app.Press(ConsoleKey.Oem1, KeyModifiers.Shift);
        app.Type("p");

        Assert.Equal("probe command", app.State.Output);
    }
}
