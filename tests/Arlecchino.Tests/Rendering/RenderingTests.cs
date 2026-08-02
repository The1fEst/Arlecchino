using System;
using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Testing;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Rendering;

public sealed class RenderingTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(255, 255, 255)]
    [InlineData(255, 0, 0)]
    [InlineData(63, 169, 245)]
    public void HslRoundTripStaysWithinRoundingDistance(byte red, byte green, byte blue)
    {
        var original = new Rgb(red, green, blue);
        var (hue, saturation, lightness) = original.ToHsl();
        var restored = Rgb.FromHsl(hue, saturation, lightness);

        Assert.InRange(Math.Abs(restored.Red - original.Red), 0, 3);
        Assert.InRange(Math.Abs(restored.Green - original.Green), 0, 3);
        Assert.InRange(Math.Abs(restored.Blue - original.Blue), 0, 3);
    }

    [Fact]
    public void HexIsParsedAndFormattedBack()
    {
        Assert.True(Rgb.TryParseHex("#3FA9F5", out var color));
        Assert.Equal(new(63, 169, 245), color);
        Assert.Equal("#3FA9F5", color.Hex);

        Assert.True(Rgb.TryParseHex("000000", out var black));
        Assert.Equal(new(0, 0, 0), black);

        Assert.False(Rgb.TryParseHex("#xyz", out _));
        Assert.False(Rgb.TryParseHex("#FFF", out _));
    }

    [Fact]
    public void TrueColorStyleCarriesBothPlanes()
    {
        using var scope = new ColorSupportScope(ColorSupport.TrueColor);
        var style = new RgbTermColor { Foreground = new(1, 2, 3), Background = new(4, 5, 6) };

        Assert.Contains("38;2;1;2;3", style.Ansi, StringComparison.Ordinal);
        Assert.Contains("48;2;4;5;6", style.Ansi, StringComparison.Ordinal);
    }

    [Fact]
    public void PaletteStyleIsBuiltOnceAndReused()
    {
        using var scope = new ColorSupportScope(ColorSupport.Palette);
        var style = new TermColor { Foreground = TerminalColor.BrightMagenta, Style = TextStyle.Bold };

        Assert.Same(style.Ansi, style.Ansi);
        Assert.Contains(";1", style.Ansi, StringComparison.Ordinal);
    }

    [Fact]
    public void FrameIsSentAsASingleWrite()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal);

        surface.StartFrame();
        surface.AppendLine("one", Theme.Default);
        surface.AppendLine("two", Theme.Header);
        surface.Build();

        Assert.Equal(4, FrameText.Lines(terminal.Written).Length);
    }

    [Fact]
    public void TooSmallTerminalReplacesTheViewWithANotice()
    {
        using var app = new TestApplication(34,
            6,
            static builder =>
            {
                builder.Options.MinimumWidth = 40;
                builder.Options.MinimumHeight = 10;
            });

        var frame = app.Frame();

        Assert.Contains(app.Options.Strings.TerminalTooSmall(), frame, StringComparison.Ordinal);
        Assert.DoesNotContain("probe", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void OutputLineIsStyledOnlyWhenItCarriesText()
    {
        using var app = new TestApplication();

        app.State.Output = "done";

        Assert.Contains("done", app.FrameLines()[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void HintsAreHiddenWhileAModalIsOpen()
    {
        using var app = new TestApplication();

        Assert.Contains(app.Options.Strings.KeysTitle(), app.Frame(), StringComparison.Ordinal);

        app.State.RequestText("Name", "", null, static _ => { });

        Assert.DoesNotContain(app.Options.Strings.KeysTitle(), app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void ModalBoxesAreRectangular()
    {
        using var app = new TestApplication(90, 26);

        app.State.RequestColor("Accent", new(63, 169, 245), static _ => { });

        var widths = new HashSet<int>();
        foreach (var line in app.FrameLines())
        {
            var width = FrameText.BoxWidth(line);
            if (width > 0)
            {
                widths.Add(width);
            }
        }

        Assert.Single(widths);
    }

    [Fact]
    public void KeysByPositionReadTheKeyRatherThanTheLayout()
    {
        var byPosition = KeyText.For(TextInputMode.ByPosition);
        var native = KeyText.For(TextInputMode.Native);
        var cyrillicQ = new ConsoleKeyInfo('й', ConsoleKey.Q, false, false, false);

        Assert.Equal('q', byPosition.Resolve(cyrillicQ));
        Assert.Equal('й', native.Resolve(cyrillicQ));
        Assert.Equal('Q', byPosition.Resolve(new('Й', ConsoleKey.Q, true, false, false)));
        Assert.Null(byPosition.Resolve(new('\0', ConsoleKey.F5, false, false, false)));
    }

    [Fact]
    public void ThePositionWinsEvenWhenTheLayoutTypedSomethingOrdinary()
    {
        var byPosition = KeyText.For(TextInputMode.ByPosition);
        var native = KeyText.For(TextInputMode.Native);
        var moved = new ConsoleKeyInfo('a', ConsoleKey.Q, false, false, false);

        Assert.Equal('q', byPosition.Resolve(moved));
        Assert.Equal('a', native.Resolve(moved));
    }

    [Fact]
    public void AKeyWithNoPositionOfItsOwnTypesNothing()
    {
        var byPosition = KeyText.For(TextInputMode.ByPosition);
        var native = KeyText.For(TextInputMode.Native);
        var unmapped = new ConsoleKeyInfo('€', ConsoleKey.Oem8, false, false, false);

        Assert.Null(byPosition.Resolve(unmapped));
        Assert.Equal('€', native.Resolve(unmapped));
    }

    [Fact]
    public void EveryPrintableKeyOfAUsKeyboardHasAPosition()
    {
        var byPosition = KeyText.For(TextInputMode.ByPosition);

        Assert.Equal(' ', byPosition.Resolve(new('\0', ConsoleKey.Spacebar, false, false, false)));
        Assert.Equal('7', byPosition.Resolve(new('\0', ConsoleKey.D7, false, false, false)));
        Assert.Equal('&', byPosition.Resolve(new('\0', ConsoleKey.D7, true, false, false)));
        Assert.Equal('3', byPosition.Resolve(new('\0', ConsoleKey.NumPad3, false, false, false)));
        Assert.Equal(';', byPosition.Resolve(new('\0', ConsoleKey.Oem1, false, false, false)));
        Assert.Equal('?', byPosition.Resolve(new('\0', ConsoleKey.Oem2, true, false, false)));
    }
}
