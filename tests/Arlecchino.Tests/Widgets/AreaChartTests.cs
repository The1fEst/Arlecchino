using System;
using System.Collections.Generic;
using System.Linq;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Testing;
using Arlecchino.Widgets.Readouts;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

public sealed class AreaChartTests
{
    private const char Blank = '⠀';

    [Fact]
    public void TheFillClimbsWithTheValue()
    {
        var lines = Draw(new() { Values = [0m, 100m], Minimum = 0, Maximum = 100 }, 2, 1);

        Assert.Equal($"{Blank}⢸", lines[0]);
    }

    [Fact]
    public void ACellCarriesTwoSamples()
    {
        var lines = Draw(new() { Values = [0m, 100m, 0m, 100m], Minimum = 0, Maximum = 100 }, 4, 1);

        Assert.Equal($"{Blank}{Blank}⢸⢸", lines[0]);
    }

    [Fact]
    public void TheNewestValueIsAtTheRight()
    {
        var lines = Draw(new() { Values = [0m, 0m, 0m, 100m], Minimum = 0, Maximum = 100 }, 4, 2);

        Assert.Equal($"{Blank}{Blank}{Blank}⢸", lines[0]);
        Assert.Equal($"{Blank}{Blank}{Blank}⢸", lines[1]);
    }

    [Fact]
    public void OnlyWhatFitsIsDrawnAndTheOldestFallsOff()
    {
        var values = new List<decimal>();

        for (var step = 0; step < 100; step++)
        {
            values.Add(step < 90 ? 0m : 100m);
        }

        Assert.Equal("⣿⣿⣿⣿", Draw(new() { Values = values, Minimum = 0, Maximum = 100 }, 4, 1)[0]);

        var wider = Draw(new() { Values = values, Minimum = 0, Maximum = 100 }, 8, 1)[0];

        Assert.StartsWith($"{Blank}", wider, StringComparison.Ordinal);
        Assert.EndsWith("⣿⣿⣿⣿", wider, StringComparison.Ordinal);
    }

    [Fact]
    public void EachSetOfSymbolsDrawsItsOwn()
    {
        decimal[] values = [0m, 40m, 80m, 100m];

        Assert.All(
            Draw(new() { Values = values, Symbols = GraphSymbols.Blocks, Minimum = 0, Maximum = 100 }, 2, 2)[1],
            symbol => Assert.Contains(symbol, " ▗▐▖▄▟▌▙█"));

        Assert.All(
            Draw(new() { Values = values, Symbols = GraphSymbols.Tty, Minimum = 0, Maximum = 100 }, 4, 2)[1],
            symbol => Assert.Contains(symbol, " ░▒█"));
    }

    [Fact]
    public void AnInvertedChartHangsFromTheTop()
    {
        decimal[] half = [50m, 50m];

        var upright = Draw(new() { Values = half, Minimum = 0, Maximum = 100 }, 2, 2);
        var hanging = Draw(new() { Values = half, Minimum = 0, Maximum = 100, Invert = true }, 2, 2);

        Assert.Equal(Blank, upright[0][1]);
        Assert.NotEqual(Blank, upright[1][1]);

        Assert.NotEqual(Blank, hanging[0][1]);
        Assert.Equal(Blank, hanging[1][1]);
    }

    [Fact]
    public void AnInvertedChartOfBlocksHangsToo()
    {
        decimal[] half = [50m, 50m];

        var upright = Draw(new() { Values = half, Symbols = GraphSymbols.Blocks, Minimum = 0, Maximum = 100 }, 2, 2);
        var hanging = Draw(
            new() { Values = half, Symbols = GraphSymbols.Blocks, Minimum = 0, Maximum = 100, Invert = true },
            2,
            2);

        Assert.Equal(' ', upright[0][1]);
        Assert.Equal('█', upright[1][1]);

        Assert.Equal('█', hanging[0][1]);
        Assert.Equal(' ', hanging[1][1]);
    }

    [Fact]
    public void TheApplicationsOwnChoiceIsWhatItFallsBackTo()
    {
        var was = Glyphs.Graph;

        try
        {
            Glyphs.Graph = GraphSymbols.Tty;

            var line = Draw(new() { Values = [100m], Minimum = 0, Maximum = 100 }, 2, 1)[0];

            Assert.Contains('█', line);
            Assert.DoesNotContain(Blank, line);
        }
        finally
        {
            Glyphs.Graph = was;
        }
    }

    [Fact]
    public void ASeriesThatDoesNotMoveDrawsItsFloor()
    {
        var lines = Draw(new() { Values = [5m, 5m, 5m, 5m] }, 2, 2);

        Assert.Equal($"{Blank}{Blank}", lines[0]);
        Assert.Equal("⣀⣀", lines[1]);
    }

    [Fact]
    public void ItFillsWhatItIsGivenAndHandsBackNothing()
    {
        var terminal = new FakeTerminal(10, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();

        var rest = new AreaChart { Values = [1m, 2m] }.Draw(surface.Frame);

        surface.Build();

        Assert.True(rest.IsEmpty);
    }

    [Fact]
    public void NothingToDrawIsNotAFailure()
    {
        var lines = Draw(new() { Values = [] }, 4, 2);

        Assert.All(lines, line => Assert.Equal(new(Blank, 4), line));
    }

    [Fact]
    public void TheFillTakesItsColourFromHowHighItClimbed()
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);

        Assert.Equal(1, ChartColours(Plain()));
        Assert.Equal(6, ChartColours(Banded()));
    }

    [Fact]
    public void ASmallerPaletteQuantisesTheBlendRatherThanLosingIt()
    {
        int rich;
        int quantised;

        using (new ColorSupportScope(ColorSupport.TrueColor))
        {
            rich = ChartColours(Banded());
        }

        using (new ColorSupportScope(ColorSupport.Palette))
        {
            quantised = ChartColours(Banded());
        }

        Assert.InRange(quantised, Banded().Bands.Count - 1, rich - 1);
    }

    [Fact]
    public void ATerminalWithoutColourDrawsTheShapeAnyway()
    {
        using var none = new ColorSupportScope(ColorSupport.None);

        Assert.Equal(0, Colours(Banded()));
        Assert.Equal($"{Blank}⢸", Draw(new() { Values = [0m, 100m], Minimum = 0, Maximum = 100 }, 2, 1)[0]);
    }

    private static AreaChart Plain() =>
        new() { Values = [0m, 50m, 100m], Minimum = 0, Maximum = 100 };

    private static AreaChart Banded() =>
        new()
        {
            Values = [0m, 50m, 100m],
            Minimum = 0,
            Maximum = 100,
            Bands = [new(0m, Theme.Active), new(60m, Theme.Warning), new(85m, Theme.Error)],
        };

    private static int ChartColours(AreaChart chart) => Colours(chart) - 1;

    private static int Colours(AreaChart chart)
    {
        var terminal = new FakeTerminal(8, 6);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        chart.Draw(surface.Frame);
        surface.Build();

        return FrameText.StylesIn(terminal.Written).Distinct().Count();
    }

    private static string[] Draw(AreaChart chart, int width, int height)
    {
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        chart.Draw(surface.Frame);
        surface.Build();

        return FrameText.Lines(terminal.Written);
    }
}
