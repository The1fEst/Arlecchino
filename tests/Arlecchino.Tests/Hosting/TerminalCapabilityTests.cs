using System;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Terminals;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Hosting;

public sealed class TerminalCapabilityTests
{
    [Fact]
    public void NoColorAndDumbTerminalsDisableStyling()
    {
        Assert.Equal(ColorSupport.None, TerminalCapabilities.DetectColor("1", null, "truecolor", null));
        Assert.Equal(ColorSupport.None, TerminalCapabilities.DetectColor(null, "dumb", "truecolor", null));
    }

    [Fact]
    public void TrueColorIsDetectedFromColorTermAndWindowsTerminal()
    {
        Assert.Equal(ColorSupport.TrueColor, TerminalCapabilities.DetectColor(null, "xterm", "truecolor", null));
        Assert.Equal(ColorSupport.TrueColor, TerminalCapabilities.DetectColor(null, "xterm", "24bit", null));
        Assert.Equal(ColorSupport.TrueColor, TerminalCapabilities.DetectColor(null, null, null, "session-id"));
    }

    [Fact]
    public void PaletteIsTheFallback()
    {
        Assert.Equal(ColorSupport.Palette, TerminalCapabilities.DetectColor(null, "xterm-256color", null, null));
        Assert.Equal(ColorSupport.Palette, TerminalCapabilities.DetectColor(null, null, null, null));
    }

    [Fact]
    public void TrueColorStyleDegradesToTheNearestPaletteEntry()
    {
        var style = new RgbTermColor { Foreground = new(250, 10, 10) };

        using (new ColorSupportScope(ColorSupport.TrueColor))
        {
            Assert.Contains("38;2;250;10;10", style.Ansi, StringComparison.Ordinal);
        }

        using (new ColorSupportScope(ColorSupport.Palette))
        {
            Assert.Contains("91", style.Ansi, StringComparison.Ordinal);
            Assert.DoesNotContain("38;2", style.Ansi, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void NoColorStripsEveryEscapeSequence()
    {
        var palette = new TermColor { Foreground = TerminalColor.BrightMagenta };
        var truecolor = new RgbTermColor { Background = new(1, 2, 3) };

        using var scope = new ColorSupportScope(ColorSupport.None);

        Assert.Equal("", palette.Ansi);
        Assert.Equal("", truecolor.Ansi);
    }

    [Fact]
    public void NearestPaletteColorPicksTheObviousMatches()
    {
        Assert.Equal(TerminalColor.BrightRed, TerminalCapabilities.NearestPaletteColor(new(255, 0, 0)));
        Assert.Equal(TerminalColor.Black, TerminalCapabilities.NearestPaletteColor(new(0, 0, 0)));
        Assert.Equal(TerminalColor.BrightWhite, TerminalCapabilities.NearestPaletteColor(new(255, 255, 255)));
        Assert.Equal(TerminalColor.BrightBlue, TerminalCapabilities.NearestPaletteColor(new(100, 100, 250)));
    }

    [Fact]
    public void FrameCarriesNoStylesWhenColorIsOff()
    {
        using var app = new TestApplication();
        using var scope = new ColorSupportScope(ColorSupport.None);

        app.State.RequestColor("Accent", new(63, 169, 245), static _ => { });

        Assert.Empty(app.RawStyles());
    }
}
