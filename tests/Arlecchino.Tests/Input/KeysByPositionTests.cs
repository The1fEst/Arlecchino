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

    [Fact]
    public void ThePaletteKeyStillTypesItselfIntoAField()
    {
        using var app = new TestApplication(configure: Configured(byPosition: true));

        app.State.RequestText("Filter", "", null, static _ => { });
        app.Frame();

        Press(app, 'Ж', ConsoleKey.Oem1, KeyModifiers.Shift);

        Assert.Contains("Ж", app.Frame(), StringComparison.Ordinal);
    }

    private static Action<ArlecchinoBuilder> Configured(
        bool byPosition,
        Action<ArlecchinoBuilder>? rest = null) => builder =>
    {
        if (byPosition)
        {
            builder.UseKeysByPosition();
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
