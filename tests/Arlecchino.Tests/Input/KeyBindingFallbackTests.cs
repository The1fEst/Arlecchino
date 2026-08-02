using System;
using Arlecchino.Input;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class KeyBindingFallbackTests
{
    private static ConsoleKeyInfo CharacterOnly(char character) => new(character, default, false, false, false);

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
        var binding = new KeyBinding(ConsoleKey.S, ConsoleModifiers.Control);

        Assert.False(binding.Matches(CharacterOnly('s')));
        Assert.True(binding.Matches(new('s', default, false, false, true)));
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

        app.Press(ConsoleKey.Oem1, shift: true);
        app.Type("p");

        Assert.Equal("probe command", app.State.Output);
    }
}
