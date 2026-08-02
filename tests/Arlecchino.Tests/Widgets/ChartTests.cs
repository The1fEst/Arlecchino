using System;
using System.Linq;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Testing;
using Arlecchino.Widgets.Readouts;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

public sealed class ChartTests
{
    [Fact]
    public void ASparklineSpreadsTheSeriesOverEveryBlock()
    {
        var terminal = new FakeTerminal(8, 2);
        var surface = Frame(terminal);

        new Sparkline { Values = [0m, 1m, 2m, 3m, 4m, 5m, 6m, 7m] }.Draw(surface.Frame);

        surface.Build();

        Assert.Equal("▁▂▃▄▅▆▇█", FrameText.Lines(terminal.Written)[0].TrimEnd());
    }

    [Fact]
    public void ASeriesThatDoesNotMoveDrawsAsTheLowestBlock()
    {
        var terminal = new FakeTerminal(8, 2);
        var surface = Frame(terminal);

        new Sparkline { Values = [5m, 5m, 5m] }.Draw(surface.Frame);

        surface.Build();

        Assert.Equal("▁▁▁", FrameText.Lines(terminal.Written)[0].TrimEnd());
    }

    [Fact]
    public void OnlyTheNewestValuesFitTheRow()
    {
        var terminal = new FakeTerminal(4, 2);
        var surface = Frame(terminal);

        new Sparkline { Values = [1m, 2m, 3m, 4m, 5m, 6m, 7m, 8m] }.Draw(surface.Frame);

        surface.Build();

        Assert.Equal("▁▃▆█", FrameText.Lines(terminal.Written)[0].TrimEnd());
    }

    [Fact]
    public void APinnedRangeIsWhatTheBlocksAreMeasuredAgainst()
    {
        var terminal = new FakeTerminal(4, 2);
        var surface = Frame(terminal);

        new Sparkline { Values = [50m], Minimum = 0, Maximum = 100 }.Draw(surface.Frame);

        surface.Build();

        Assert.Equal("▅", FrameText.Lines(terminal.Written)[0].TrimEnd());
    }

    [Fact]
    public void TheCaptionReadsTheNewestValueAndTakesRoomFromTheLine()
    {
        var terminal = new FakeTerminal(20, 2);
        var surface = Frame(terminal);

        var rest = new Sparkline
        {
            Values = [1m, 2m, 62m],
            Caption = static value => $"{value:0}%",
        }.Draw(surface.Frame);

        surface.Build();

        var line = FrameText.Lines(terminal.Written)[0];

        Assert.EndsWith("62%", line.TrimEnd(), StringComparison.Ordinal);
        Assert.StartsWith("▁▁█", line, StringComparison.Ordinal);
        Assert.Equal(1, rest.Top);
    }

    [Fact]
    public void BarsAreMeasuredAgainstTheLargestItem()
    {
        var terminal = new FakeTerminal(22, 4);
        var surface = Frame(terminal);

        var rest = Chart([("a", 10m), ("b", 5m)]).Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.Equal("a " + new string('█', 20), lines[0]);
        Assert.Equal("b " + new string('█', 10) + new string('░', 10), lines[1]);
        Assert.Equal(2, rest.Top);
        Assert.Equal(2, rest.Height);
    }

    [Fact]
    public void APinnedMaximumKeepsTheBarsComparableBetweenFrames()
    {
        var terminal = new FakeTerminal(22, 4);
        var surface = Frame(terminal);
        var chart = Chart([("a", 10m)]);

        new BarChart<(string Name, decimal Count)>
        {
            Render = chart.Render,
            Value = chart.Value,
            Items = chart.Items,
            Maximum = 20m,
        }.Draw(surface.Frame);

        surface.Build();

        Assert.Equal("a " + new string('█', 10) + new string('░', 10), FrameText.Lines(terminal.Written)[0]);
    }

    [Fact]
    public void TheReadoutsShareAColumnOfTheirOwn()
    {
        var terminal = new FakeTerminal(22, 4);
        var surface = Frame(terminal);

        var chart = Chart([("a", 100m), ("b", 5m)]);

        new BarChart<(string Name, decimal Count)>
        {
            Render = chart.Render,
            Value = chart.Value,
            Items = chart.Items,
            Caption = static value => $"{value:0}",
        }.Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.EndsWith(" 100", lines[0], StringComparison.Ordinal);
        Assert.EndsWith("   5", lines[1], StringComparison.Ordinal);
        Assert.Equal(new('█', 16), lines[0][2..18]);
    }

    [Fact]
    public void BarsPastTheBottomOfTheRegionAreNotDrawn()
    {
        var terminal = new FakeTerminal(22, 2);
        var surface = Frame(terminal);

        var rest = Chart([("a", 1m), ("b", 1m), ("c", 1m), ("d", 1m)]).Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.StartsWith("a", lines[0], StringComparison.Ordinal);
        Assert.StartsWith("b", lines[1], StringComparison.Ordinal);
        Assert.True(rest.IsEmpty);
    }

    [Fact]
    public void ALongLabelIsTruncatedRatherThanSqueezingTheBarsOut()
    {
        var terminal = new FakeTerminal(20, 2);
        var surface = Frame(terminal);

        Chart([("Storefront.Workers", 1m)]).Draw(surface.Frame);

        surface.Build();

        var line = FrameText.Lines(terminal.Written)[0];

        Assert.StartsWith("Storef ", line, StringComparison.Ordinal);
        Assert.Equal(13, line.Length - line.IndexOf('█'));
    }

    [Fact]
    public void AGaugeFillsAgainstARangeThatNeedNotStartAtZero()
    {
        var terminal = new FakeTerminal(20, 2);
        var surface = Frame(terminal);
        var gauge = new Gauge { Minimum = 20, Maximum = 40, Value = 30 };

        var rest = gauge.Draw(surface.Frame);

        surface.Build();

        Assert.Equal(0.5m, gauge.Fraction);
        Assert.Equal(new string('█', 10) + new string('░', 10), FrameText.Lines(terminal.Written)[0]);
        Assert.Equal(1, rest.Top);
    }

    [Fact]
    public void AValueOutsideTheRangeReadsAsEmptyOrFull()
    {
        var gauge = new Gauge { Minimum = 0, Maximum = 100, Value = 140 };

        Assert.Equal(1m, gauge.Fraction);

        gauge.Value = -5;

        Assert.Equal(0m, gauge.Fraction);
    }

    [Fact]
    public void TheBandAValueLandsInIsWhatColoursIt()
    {
        var gauge = new Gauge
        {
            Bands =
            [
                new(0m, Theme.Active),
                new(70m, Theme.Warning),
                new(90m, Theme.Error),
            ],
        };

        Assert.Same(Theme.Active, gauge.StyleAt(0m));
        Assert.Same(Theme.Active, gauge.StyleAt(69m));
        Assert.Same(Theme.Warning, gauge.StyleAt(70m));
        Assert.Same(Theme.Error, gauge.StyleAt(200m));
    }

    [Fact]
    public void BelowEveryBandTheGaugeFallsBackToItsOwnStyle()
    {
        var gauge = new Gauge { Style = Theme.Info, Bands = [new(50m, Theme.Error)] };

        Assert.Same(Theme.Info, gauge.StyleAt(10m));
        Assert.Same(Theme.Error, gauge.StyleAt(50m));
    }

    [Fact]
    public void TheFillChangesColourWhereItCrossesABand()
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);

        var oneBand = new Gauge { Value = 100, Bands = [new(0m, Theme.Active)] };
        var crossing = new Gauge { Value = 100, Bands = [new(0m, Theme.Active), new(50m, Theme.Error)] };

        Assert.Equal(ColoursOf(oneBand) + 1, ColoursOf(crossing));
    }

    [Fact]
    public void AnEmptyChartDrawsNothingAndKeepsItsRegion()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = Frame(terminal);

        var rest = Chart([]).Draw(surface.Frame);

        surface.Build();

        Assert.Equal("", FrameText.Lines(terminal.Written)[0].Trim());
        Assert.Equal(4, rest.Height);
    }

    private static int ColoursOf(Gauge gauge)
    {
        var terminal = new FakeTerminal(10, 2);
        var surface = Frame(terminal);

        gauge.Draw(surface.Frame);
        surface.Build();

        Assert.Equal(new('█', 10), FrameText.Lines(terminal.Written)[0]);

        return FrameText.StylesIn(terminal.Written).Distinct().Count();
    }


    private static BarChart<(string Name, decimal Count)> Chart((string Name, decimal Count)[] items) =>
        new()
        {
            Render = static item => item.Name,
            Value = static item => item.Count,
            Items = items,
        };

    private static Surface Frame(FakeTerminal terminal)
    {
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();

        return surface;
    }
}
