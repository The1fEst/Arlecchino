using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Tests.Navigation;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class KeysByPositionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AFieldTypesWhatTheLayoutSaysEitherWay(bool byPosition)
    {
        using var app = new TestApplication(configure: Configured(byPosition));

        app.State.RequestText("Filter", "", null, static _ => { });
        app.Frame();

        Press(app, 'ф', ConsoleKey.A);
        Press(app, 'ы', ConsoleKey.S);
        Press(app, 'в', ConsoleKey.D);

        Assert.Contains("фыв", app.Frame(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ThePaletteKeyIsTheOneItSitsOn(bool byPosition, bool opens)
    {
        using var app = new TestApplication(
            configure: Configured(byPosition, static builder => builder.AddCommand<ProbeCommand>()));

        app.Frame();

        Press(app, 'Ж', ConsoleKey.Oem1, KeyModifiers.Shift);

        Assert.Equal(opens, app.State.Modal is not null);
    }

    [Theory]
    [InlineData(false, "handled Oem2")]
    [InlineData(true, "find")]
    public void ACommandNamedAfterACharacterIsReachedFromItsKey(bool byPosition, string ran)
    {
        using var app = new TestApplication(configure: Configured(byPosition));

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        Press(app, '.', ConsoleKey.Oem2);

        Assert.Equal([ran], CommandingView.Ran);
    }

    /// <summary>
    /// A key named by a terminal that was asked to name them: the Cyrillic character it typed, the key
    /// it is on the base layout behind it, and the text after the modifiers. The command is on the key.
    /// </summary>
    [Fact]
    public void ACommandOnAKeyIsReachedFromTheKeyTheTerminalNames()
    {
        using var app = new TestApplication(configure: Configured(byPosition: true));

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        app.ReadFromTerminal(CyrillicKey(character: 'и', capital: 'И', key: 'b'));

        Assert.Equal(["build"], CommandingView.Ran);
    }

    [Fact]
    public void ThePaletteOpensFromTheKeyItSitsOnWhereverTheColonIs()
    {
        using var app = new TestApplication(
            configure: Configured(true, static builder => builder.AddCommand<ProbeCommand>()));

        app.Frame();

        app.ReadFromTerminal(CyrillicKey(character: 'ж', capital: 'Ж', key: ';', shifted: true));

        Assert.NotNull(app.State.Modal);
    }

    [Fact]
    public void AFieldStillTypesTheTextTheTerminalReports()
    {
        using var app = new TestApplication(configure: Configured(byPosition: true));

        app.State.RequestText("Filter", "", null, static _ => { });
        app.Frame();

        app.ReadFromTerminal(CyrillicKey(character: 'ф', capital: 'Ф', key: 'a'));
        app.ReadFromTerminal(CyrillicKey(character: 'ы', capital: 'Ы', key: 's'));
        app.ReadFromTerminal(CyrillicKey(character: 'в', capital: 'В', key: 'd'));

        Assert.Contains("фыв", app.Frame(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Whether the terminal is asked to name its keys at all, which is the whole of what the option
    /// does to it. A terminal that is never asked goes on sending what it always sent.
    /// </summary>
    /// <param name="byPosition">Whether the application asked to read its keys by position.</param>
    /// <param name="asked">Whether the request should have reached the terminal.</param>
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void TheTerminalIsAskedForKeyNamesOnlyWhenTheyAreWanted(bool byPosition, bool asked)
    {
        using var app = new TestApplication(configure: Configured(byPosition));

        ((TerminalModes)app.Services.GetService(typeof(TerminalModes))!).Enter();

        Assert.Equal(asked, app.Terminal.AreKeyNamesAsked);
    }

    [Fact]
    public void ThePaletteKeyStillTypesItselfIntoAField()
    {
        using var app = new TestApplication(configure: Configured(byPosition: true));

        app.State.RequestText("Filter", "", null, static _ => { });
        app.Frame();

        Press(app, 'Ж', ConsoleKey.Oem1, KeyModifiers.Shift);

        Assert.Contains("Ж", app.Frame(), StringComparison.Ordinal);
    }

    /// <summary>
    /// What a terminal asked to name its keys sends for one of them: the character the layout typed,
    /// the one it types with Shift, the key of the base layout, then the modifiers and the text.
    /// </summary>
    /// <param name="character">The character this key types on the layout in force.</param>
    /// <param name="capital">The character it types with Shift held.</param>
    /// <param name="key">The key it is on the base layout.</param>
    /// <param name="shifted">Whether Shift was held.</param>
    /// <returns>The sequence, escape and all.</returns>
    private static string CyrillicKey(char character, char capital, char key, bool shifted = false) =>
        FormattableString.Invariant(
            $"\e[{(int)character}:{(int)capital}:{(int)key};{(shifted ? 2 : 1)};{(int)(shifted ? capital : character)}u");

    private static Action<ArlecchinoBuilder> Configured(
        bool byPosition,
        Action<ArlecchinoBuilder>? rest = null) => builder =>
    {
        if (byPosition)
        {
            builder.TryUseKeysByPosition();
        }

        rest?.Invoke(builder);
    };

    private static void Press(TestApplication app, char character, ConsoleKey key, KeyModifiers modifiers = default)
    {
        app.Terminal.Enqueue(new(key, modifiers, character));

        ((TerminalInputReader)app.Services.GetService(typeof(TerminalInputReader))!).ReadPending();

        app.DrainInput();
    }
}
