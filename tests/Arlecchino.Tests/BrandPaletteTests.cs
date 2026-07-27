using System;
using System.Linq;
using Arlecchino.Rendering;
using Xunit;

namespace Arlecchino.Tests;

public sealed class BrandPaletteTests
{
    private const string Crimson = "201;56;43";
    private const string Bone = "237;230;217";

    [Fact]
    public void ExactColoursAreDrawnWhereTheTerminalCanShowThem()
    {
        using var colours = new ColorSupportScope(ColorSupport.TrueColor);

        Assert.Contains($"38;2;{Crimson}", ThemePalette.Arlecchino.Header.Ansi, StringComparison.Ordinal);
        Assert.Contains($"38;2;{Bone}", ThemePalette.Arlecchino.Accent.Ansi, StringComparison.Ordinal);
    }

    [Fact]
    public void APaletteTerminalGetsTheColourTheAuthorChose()
    {
        using var colours = new ColorSupportScope(ColorSupport.Palette);

        var header = ThemePalette.Arlecchino.Header.Ansi;

        Assert.DoesNotContain("38;2;", header, StringComparison.Ordinal);
        Assert.Contains($";{TermColor.ForegroundCode(TerminalColor.BrightRed)}", header, StringComparison.Ordinal);
    }

    [Fact]
    public void NoColourAtAllLeavesNothingBehind()
    {
        using var colours = new ColorSupportScope(ColorSupport.None);

        Assert.Equal("", ThemePalette.Arlecchino.Header.Ansi);
        Assert.Equal("", ThemePalette.Arlecchino.ActiveSelected.Ansi);
    }

    [Fact]
    public void TheBackgroundIsLeftToTheTerminalEverywhereButTheCursorRows()
    {
        var palette = ThemePalette.Arlecchino;

        TermColor[] overText =
            [palette.Default, palette.Header, palette.TableHeader, palette.Accent, palette.Info, palette.Muted,
             palette.Active];

        Assert.All(overText, colour => Assert.Equal(TerminalColor.Default, colour.Background));
        Assert.All(overText, colour => Assert.Null(colour.ExactBackground));

        Assert.NotEqual(TerminalColor.Default, palette.Selected.Background);
        Assert.NotEqual(TerminalColor.Default, palette.ActiveSelected.Background);
    }

    [Fact]
    public void EveryExactColourCarriesAPaletteColourBehindIt()
    {
        var palette = ThemePalette.Arlecchino;

        TermColor[] entries =
            [palette.Header, palette.TableHeader, palette.Accent, palette.Info, palette.Muted, palette.Input,
             palette.Selected, palette.Active, palette.ActiveSelected, palette.Warning, palette.Error];

        Assert.All(entries.Where(static colour => colour.ExactForeground is not null),
            colour => Assert.NotEqual(TerminalColor.Default, colour.Foreground));

        Assert.All(entries.Where(static colour => colour.ExactBackground is not null),
            colour => Assert.NotEqual(TerminalColor.Default, colour.Background));
    }

    [Fact]
    public void AnApplicationCanTakeItWithoutTouchingAnythingElse()
    {
        using var colours = new ColorSupportScope(ColorSupport.TrueColor);
        using var app = new TestApplication(80, 24, static builder => builder.UseTheme(ThemePalette.Arlecchino));

        app.Navigator.Apply(Navigation.Routes.Help);

        Assert.Contains(app.Styles(), style => style.Contains($"38;2;{Crimson}", StringComparison.Ordinal));
    }

    [Fact]
    public void AnApplicationThatAsksForNoThemeIsAlreadyDrawnInIt()
    {
        using var colours = new ColorSupportScope(ColorSupport.TrueColor);
        using var app = new TestApplication();

        app.Navigator.Apply(Navigation.Routes.Help);

        Assert.Contains(app.Styles(), style => style.Contains($"38;2;{Crimson}", StringComparison.Ordinal));
    }

    [Fact]
    public void TheSixteenColoursOfTheOldDefaultAreStillThereUnderTheirOwnName()
    {
        using var colours = new ColorSupportScope(ColorSupport.TrueColor);

        Assert.DoesNotContain("38;2;", ThemePalette.Basic.Header.Ansi, StringComparison.Ordinal);
        Assert.Equal(TerminalColor.BrightMagenta, ThemePalette.Basic.Header.Foreground);
        Assert.Equal(TerminalColor.Green, ThemePalette.Basic.Active.Foreground);
    }
}
