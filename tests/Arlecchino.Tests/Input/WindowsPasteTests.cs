using System;
using System.Linq;
using Arlecchino.Input;
using Xunit;

namespace Arlecchino.Tests.Input;

public sealed class WindowsPasteTests
{
    [Fact]
    public void ARunThatEndsInANewlineReadsAsAPaste()
    {
        Assert.True(WindowsPaste.Reads([Typed('l'), Typed('s'), Typed('\r')]));
    }

    [Fact]
    public void ALongRunReadsAsAPasteWithoutOne()
    {
        Assert.True(WindowsPaste.Reads([Typed('n'), Typed('o'), Typed('t'), Typed('e')]));
    }

    [Fact]
    public void TwoKeysThatLandedTogetherAreStillTyping()
    {
        Assert.False(WindowsPaste.Reads([Typed('j'), Typed('k')]));
    }

    [Fact]
    public void ASingleKeyIsTyping()
    {
        Assert.False(WindowsPaste.Reads([Typed('l')]));
    }

    [Fact]
    public void ARunOfOneCharacterIsAHeldKey()
    {
        Assert.False(WindowsPaste.Reads([Typed('j'), Typed('j'), Typed('j')]));
    }

    [Fact]
    public void ARunWithAKeyThatTypesNothingIsNotAPaste()
    {
        Assert.False(WindowsPaste.Reads([Typed('l'), new(ConsoleKey.F5)]));
    }

    [Fact]
    public void ANewlineIsPartOfWhatWasPasted()
    {
        Assert.True(WindowsPaste.Types(Typed('\r')));
    }

    [Fact]
    public void AKeyHeldWithControlTypesNothingToPaste()
    {
        Assert.False(WindowsPaste.Types(new(ConsoleKey.V, KeyModifiers.Control, 'v')));
    }

    [Fact]
    public void AWrappedRunCarriesTheMarkersAReaderLooksFor()
    {
        var lines = string.Concat(WindowsPaste.Wrapped([Typed('l'), Typed('s'), Typed('\r')])
            .Select(static key => key.Character));

        Assert.Equal("\e[200~ls\r\e[201~", lines);
    }

    [Fact]
    public void TheEscapeOfAMarkerIsNamedAsTheKeyItIs()
    {
        var first = WindowsPaste.Wrapped([Typed('l'), Typed('s')]).First();

        Assert.Equal(ConsoleKey.Escape, first.Key);
        Assert.Equal('\e', first.Character);
    }

    private static KeyPress Typed(char character) => new(default, KeyModifiers.None, character);
}
