using System;
using Arlecchino.Input;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class KeysByPositionTests
{
    [Fact]
    public void ACyrillicLayoutTypesWhatTheKeysSay()
    {
        using var app = new TestApplication(configure: static builder => builder.UseKeysByPosition());

        app.State.RequestText("Filter", "", null, static _ => { });
        app.Frame();

        Press(app, 'ф', ConsoleKey.A);
        Press(app, 'ы', ConsoleKey.S);
        Press(app, 'в', ConsoleKey.D);

        Assert.Contains("asd", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheDefaultTypesWhatTheLayoutSays()
    {
        using var app = new TestApplication();

        app.State.RequestText("Filter", "", null, static _ => { });
        app.Frame();

        Press(app, 'ф', ConsoleKey.A);
        Press(app, 'ы', ConsoleKey.S);
        Press(app, 'в', ConsoleKey.D);

        Assert.Contains("фыв", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePaletteStillOpensOnALayoutThatHasNoColonThere()
    {
        using var app = new TestApplication(
            configure: static builder => builder.UseKeysByPosition().AddCommand<ProbeCommand>());

        app.Frame();

        Press(app, 'Ж', ConsoleKey.Oem1, KeyModifiers.Shift);

        Assert.NotNull(app.State.Modal);
    }

    [Fact]
    public void OnTheDefaultThatKeyOpensNothing()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<ProbeCommand>());

        app.Frame();

        Press(app, 'Ж', ConsoleKey.Oem1, KeyModifiers.Shift);

        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void WithoutItThatKeyTypesWhatTheLayoutSays()
    {
        using var app = new TestApplication();

        app.State.RequestText("Filter", "", null, static _ => { });
        app.Frame();

        Press(app, 'Ж', ConsoleKey.Oem1, KeyModifiers.Shift);

        Assert.Contains("Ж", app.Frame(), StringComparison.Ordinal);
    }

    private static void Press(TestApplication app, char typed, ConsoleKey key, KeyModifiers modifiers = default)
    {
        app.Terminal.Enqueue(new(key, modifiers, typed));

        ((TerminalInputReader)app.Services.GetService(typeof(TerminalInputReader))!).ReadPending();

        app.DrainInput();
    }
}
