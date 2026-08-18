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
        Assert.True(new KeyBinding(ConsoleKey.Oem2).Matches(CharacterOnly('/')));
        Assert.True(new KeyBinding(ConsoleKey.Oem5).Matches(CharacterOnly('\\')));
        Assert.False(new KeyBinding(ConsoleKey.Oem2).Matches(CharacterOnly('\\')));
    }

    /// <summary>
    /// What a terminal really hands back for the punctuation: a slash, a minus and a full stop arrive under
    /// the keypad's names, so a binding on either name answers to both.
    /// </summary>
    /// <param name="output">The key the binding was written on.</param>
    /// <param name="press">The key the terminal reported.</param>
    /// <param name="types">The character both of them type.</param>
    [Theory]
    [InlineData(ConsoleKey.Oem2, ConsoleKey.Divide, '/')]
    [InlineData(ConsoleKey.OemMinus, ConsoleKey.Subtract, '-')]
    [InlineData(ConsoleKey.OemPeriod, ConsoleKey.Decimal, '.')]
    public void AKeyWithTwoNamesAnswersToEither(ConsoleKey output, ConsoleKey press, char types)
    {
        Assert.True(new KeyBinding(output).Matches(new(press, default, types)));
        Assert.True(new KeyBinding(press).Matches(new(output, default, types)));

        Assert.Equal(types.ToString(), new KeyBinding(output).ToString());
        Assert.Equal(types.ToString(), new KeyBinding(press).ToString());
    }

    /// <summary>The pair that types two different characters stays two keys.</summary>
    [Fact]
    public void ThePlusAndTheEqualsAreNotTheSameKey()
    {
        Assert.False(new KeyBinding(ConsoleKey.OemPlus).Matches(new(ConsoleKey.Add, default, '+')));

        Assert.Equal("=", new KeyBinding(ConsoleKey.OemPlus).ToString());
        Assert.Equal("+", new KeyBinding(ConsoleKey.Add).ToString());
        Assert.Equal("*", new KeyBinding(ConsoleKey.Multiply).ToString());
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

    /// <summary>
    /// A binding on a character answers to that character however the keyboard produced it, and the key
    /// screen writes the character itself.
    /// </summary>
    [Fact]
    public void ACharacterBindingAnswersToTheCharacter()
    {
        var binding = new KeyBinding('!');

        Assert.True(binding.Matches(CharacterOnly('!')));
        Assert.True(binding.Matches(new(ConsoleKey.D1, KeyModifiers.Shift, '!')));
        Assert.False(binding.Matches(CharacterOnly('1')));
        Assert.False(binding.Matches(new(default, KeyModifiers.Control, '!')));
        Assert.Equal("!", binding.ToString());
        Assert.False(binding.IsNone);
    }

    /// <summary>
    /// A press that names a key and carries no character is still answered, where that key is the one
    /// that types the character. A console reporting keys rather than text is what that press is.
    /// </summary>
    [Fact]
    public void ACharacterBindingAnswersToTheKeyThatTypesIt()
    {
        Assert.True(new KeyBinding('/').Matches(new(ConsoleKey.Oem2)));
        Assert.True(new KeyBinding(':').Matches(new(ConsoleKey.Oem1)));
        Assert.False(new KeyBinding('/').Matches(new(ConsoleKey.Oem5)));
    }

    /// <summary>Two characters are two bindings, which is what stops one of them shadowing the other.</summary>
    [Fact]
    public void CharacterBindingsAreToldApart()
    {
        var setting = new KeyBinding('!');

        Assert.NotEqual(setting, new(':'));
        Assert.Equal(setting, new('!'));
        Assert.NotEqual(setting, new(ConsoleKey.D1));
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
