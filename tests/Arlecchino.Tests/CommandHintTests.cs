using System;
using Xunit;

namespace Arlecchino.Tests;

public sealed class CommandHintTests
{
    [Fact]
    public void TheHintsBoxOffersThePaletteWhenThereIsACommand()
    {
        using var app = new TestApplication(120, 40, static builder =>
        {
            builder.Options.ShowHints = true;
            builder.AddCommand<ProbeCommand>();
        });

        var frame = app.Frame();

        Assert.Contains($"{app.Options.CommandPaletteKey} → {app.Options.Strings.HintCommands()}", frame,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ThereIsNoSuchHintWhenNothingIsRegistered()
    {
        using var app = new TestApplication(120, 40, static builder => builder.Options.ShowHints = true);

        Assert.DoesNotContain(app.Options.Strings.HintCommands(), app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheHintFollowsTheKeyThatOpensThePalette()
    {
        using var app = new TestApplication(120, 40, static builder =>
        {
            builder.Options.ShowHints = true;
            builder.Options.CommandPaletteKey = '!';
            builder.AddCommand<ProbeCommand>();
        });

        var frame = app.Frame();

        Assert.Contains($"! → {app.Options.Strings.HintCommands()}", frame, StringComparison.Ordinal);
        Assert.DoesNotContain($": → {app.Options.Strings.HintCommands()}", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePaletteLineIsTranslatedLikeEverythingElse()
    {
        using var app = new TestApplication(120, 40, static builder =>
        {
            builder.Options.ShowHints = true;
            builder.UseStrings(new() { HintCommands = static () => "«команды»" });
            builder.AddCommand<ProbeCommand>();
        });

        Assert.Contains("«команды»", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheViewKeepsItsOwnHintsAbove()
    {
        using var app = new TestApplication(120, 40, static builder =>
        {
            builder.Options.ShowHints = true;
            builder.AddCommand<ProbeCommand>();
        });

        var frame = app.Frame();
        var own = frame.IndexOf("other", StringComparison.Ordinal);
        var palette = frame.IndexOf(app.Options.Strings.HintCommands(), StringComparison.Ordinal);

        Assert.True(own >= 0, "the probe view draws a hint of its own");
        Assert.True(own < palette, "the view's own hints come first");
    }
}
