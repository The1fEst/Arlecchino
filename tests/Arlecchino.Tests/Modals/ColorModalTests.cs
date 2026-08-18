using System;
using Arlecchino.Rendering.Colors;
using Xunit;
using Arlecchino.Modals.Setting;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

public sealed class ColorModalTests
{
    [Fact]
    public void SwatchShowsTheHexOfTheCurrentColor()
    {
        using var app = new TestApplication();

        app.State.RequestColor("Accent", new(255, 0, 0), static _ => { });

        Assert.Contains("#FF0000", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void SwatchIsPaintedWithATrueColorSequence()
    {
        using var app = new TestApplication();

        app.State.RequestColor("Accent", new(63, 169, 245), static _ => { });

        Assert.Contains(app.RawStyles(), style => style.Contains("48;2;", StringComparison.Ordinal));
    }

    [Fact]
    public void VerticalArrowsPickTheChannel()
    {
        using var app = new TestApplication();

        app.State.RequestColor("Accent", new(255, 0, 0), static _ => { });
        var modal = (ColorModal)app.State.Modal!;

        Assert.Equal(ColorChannel.Hue, modal.Channel);

        app.Press(ConsoleKey.DownArrow);
        Assert.Equal(ColorChannel.Saturation, modal.Channel);

        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.DownArrow);
        Assert.Equal(ColorChannel.Lightness, modal.Channel);
    }

    [Fact]
    public void HorizontalArrowsAdjustTheActiveChannel()
    {
        using var app = new TestApplication();

        app.State.RequestColor("Accent", new(255, 0, 0), static _ => { });
        var modal = (ColorModal)app.State.Modal!;

        app.Press(ConsoleKey.RightArrow);
        Assert.Equal(1, modal.Hue);

        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.PageDown);
        Assert.Equal(90, modal.Saturation);
    }

    [Fact]
    public void HueWrapsAroundAndPercentagesClamp()
    {
        using var app = new TestApplication();

        app.State.RequestColor("Accent", new(255, 0, 0), static _ => { });
        var modal = (ColorModal)app.State.Modal!;

        app.Press(ConsoleKey.LeftArrow);
        Assert.Equal(359, modal.Hue);

        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.End);
        app.Press(ConsoleKey.RightArrow);
        Assert.Equal(100, modal.Saturation);

        app.Press(ConsoleKey.Home);
        Assert.Equal(0, modal.Saturation);
    }

    [Fact]
    public void EnterHandsBackTheEditedColor()
    {
        using var app = new TestApplication();
        var choice = default(Rgb);

        app.State.RequestColor("Accent", new(255, 0, 0), value => choice = value);

        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Home);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(0, 0, 0), choice);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void ChannelLabelsComeFromStrings()
    {
        using var app = new TestApplication();

        app.State.RequestColor("Accent", new(255, 0, 0), static _ => { });

        var frame = app.Frame();
        Assert.Contains(app.Options.Strings.ColorHue(), frame, StringComparison.Ordinal);
        Assert.Contains(app.Options.Strings.ColorSaturation(), frame, StringComparison.Ordinal);
        Assert.Contains(app.Options.Strings.ColorLightness(), frame, StringComparison.Ordinal);
    }
}
