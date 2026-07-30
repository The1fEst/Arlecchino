using System;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class PictureTests
{
    private static readonly Rgb Red = new(255, 0, 0);
    private static readonly Rgb Blue = new(0, 0, 255);

    [Fact]
    public void OneCellCarriesTwoPixels()
    {
        var picture = new Picture();

        picture.Show([Red, Blue], 1, 2);

        var written = Draw(picture, 1, 1);

        Assert.Contains("38;2;255;0;0", written, StringComparison.Ordinal);
        Assert.Contains("48;2;0;0;255", written, StringComparison.Ordinal);
        Assert.Contains('▀', written);
    }

    [Fact]
    public void APictureIsNotStretchedToFillTheRegion()
    {
        var picture = new Picture();

        picture.Show(new Rgb[100 * 50], 100, 50);

        var lines = FrameText.Lines(Draw(picture, 40, 40));
        var drawn = 0;

        foreach (var line in lines)
        {
            if (!line.Contains('▀', StringComparison.Ordinal))
            {
                continue;
            }

            drawn++;
            Assert.Equal(40, line.TrimEnd().Length);
        }

        Assert.Equal(10, drawn);
    }

    [Fact]
    public void APictureNarrowerThanTheRegionSitsInTheMiddleOfIt()
    {
        var picture = new Picture();

        picture.Show(new Rgb[2 * 8], 2, 8);

        var lines = FrameText.Lines(Draw(picture, 9, 4));

        Assert.Equal(3, lines[0].IndexOf('▀'));
        Assert.Equal(2, lines[0].TrimEnd().Length - lines[0].IndexOf('▀'));
        Assert.Equal(3, lines[3].IndexOf('▀'));
    }

    [Fact]
    public void APictureSmallerThanTheRegionIsEnlargedToFillIt()
    {
        var picture = new Picture();

        picture.Show([Red, Red], 1, 2);

        var lines = FrameText.Lines(Draw(picture, 9, 5));

        Assert.Equal("▀▀▀▀▀", lines[0].Trim());
        Assert.Equal(5, lines[4].Trim().Length);
    }

    [Fact]
    public void ThePixelsAreCopiedOutOfWhatItWasGiven()
    {
        var picture = new Picture();
        var pixels = new[] { Red, Red };

        picture.Show(pixels, 1, 2);
        pixels[0] = Blue;

        Assert.Contains("38;2;255;0;0", Draw(picture, 1, 1), StringComparison.Ordinal);
    }

    [Fact]
    public void APictureThatIsNotThereDrawsNothingAndSaysSo()
    {
        var picture = new Picture();

        Assert.True(picture.IsEmpty);
        Assert.Equal("", FrameText.Lines(Draw(picture, 6, 3))[0].Trim());

        picture.Show([Red, Blue], 1, 2);

        Assert.False(picture.IsEmpty);
        Assert.Equal(1, picture.PixelWidth);
        Assert.Equal(2, picture.PixelHeight);

        picture.Clear();

        Assert.True(picture.IsEmpty);
        Assert.Equal("", FrameText.Lines(Draw(picture, 6, 3))[0].Trim());
    }

    [Fact]
    public void TooFewPixelsForTheSizeGivenIsRefused()
    {
        var picture = new Picture();

        Assert.Throws<ArgumentException>(() => picture.Show([Red], 2, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => picture.Show([Red], -1, 2));
        Assert.True(picture.IsEmpty);
    }

    [Fact]
    public void ItFillsWhatItIsGivenAndHandsBackNothing()
    {
        var terminal = new FakeTerminal(8, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var picture = new Picture();

        picture.Show([Red, Blue], 1, 2);
        surface.StartFrame();

        var rest = picture.Draw(surface.Frame);

        surface.Build();

        Assert.True(rest.IsEmpty);
    }

    [Fact]
    public void WhatIsBehindThePictureIsDrawnWhenItIsAskedFor()
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var picture = new Picture { Background = Theme.Info };

        picture.Show(new Rgb[2 * 8], 2, 8);

        var written = Draw(picture, 9, 4);

        Assert.Contains(Theme.Info.Ansi, written, StringComparison.Ordinal);
    }

    [Fact]
    public void TheKittyProtocolSendsThePixelsThemselves()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show([Red, Blue], 1, 2);

        var written = Draw(picture, 4, 2);

        Assert.Contains("\e_Ga=T,q=2,f=24,i=", written, StringComparison.Ordinal);
        Assert.Contains(",s=1,v=2,c=2,r=2,m=0;", written, StringComparison.Ordinal);
        Assert.Contains(Convert.ToBase64String([255, 0, 0, 0, 0, 255]), written, StringComparison.Ordinal);
        Assert.EndsWith("\e\\", written.TrimEnd(), StringComparison.Ordinal);
        Assert.DoesNotContain('▀', written);
    }

    [Fact]
    public void EachPictureOwnsAnImageNumberSoOneReplacesItselfRatherThanPileUp()
    {
        var first = new Picture { Protocol = ImageProtocol.Kitty };
        var second = new Picture { Protocol = ImageProtocol.Kitty };

        first.Show([Red, Blue], 1, 2);
        second.Show([Red, Blue], 1, 2);

        var mine = Image(Draw(first, 4, 2));

        Assert.Equal(mine, Image(Draw(first, 4, 2)));
        Assert.NotEqual(mine, Image(Draw(second, 4, 2)));
    }

    [Fact]
    public void ClearingAKittyPictureTellsTheTerminalToLetGoOfIt()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show([Red, Blue], 1, 2);

        var image = Image(Draw(picture, 4, 2));
        var written = Undrawn(picture, 4, 2, picture.Clear);

        Assert.Contains($"\e_Ga=d,d=i,i={image},q=2\e\\", written, StringComparison.Ordinal);
    }

    [Fact]
    public void ForgettingTheLastFrameSendsThePictureAgain()
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(6, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show([Red, Blue], 1, 2);

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        var drawn = terminal.Written.Length;

        surface.ForgetPreviousFrame();
        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        Assert.Contains("\e_G", terminal.Written[drawn..], StringComparison.Ordinal);
    }

    [Fact]
    public void AFixedSizeSurfaceSendsThePictureWithEveryFrameItWritesWhole()
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(6, 3);
        var surface = new Surface(terminal);
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        surface.SetFixedSize(6, 3);
        picture.Show([Red, Blue], 1, 2);

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        var drawn = terminal.Written.Length;

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        Assert.Contains("\e_G", terminal.Written[drawn..], StringComparison.Ordinal);
    }

    [Fact]
    public void AStillPictureInCellsIsNotWrittenOutAgain()
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(8, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var picture = new Picture();

        picture.Show([Red, Blue], 1, 2);

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        var drawn = terminal.Written.Length;

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        Assert.Equal(drawn, terminal.Written.Length);
    }

    [Fact]
    public void TheLastBandOfAnUndrawPaintsOnlyTheRowsThePictureHad()
    {
        var was = (TerminalCapabilities.Background, Glyphs.CellWidth, Glyphs.CellHeight);

        try
        {
            TerminalCapabilities.Background = new(0, 0, 0);
            Glyphs.CellWidth = 1;
            Glyphs.CellHeight = 8;

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red], 1, 1);

            var written = Undrawn(picture, 1, 1, picture.Clear);

            Assert.Contains("!1~-", written, StringComparison.Ordinal);
            Assert.Contains("!1B-", written, StringComparison.Ordinal);
        }
        finally
        {
            (TerminalCapabilities.Background, Glyphs.CellWidth, Glyphs.CellHeight) = was;
        }
    }

    [Fact]
    public void WhatIsUndrawnIsPaintedOverBeforeTheFrameIsDrawnOverIt()
    {
        var was = TerminalCapabilities.Background;

        try
        {
            TerminalCapabilities.Background = new(255, 255, 255);

            using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
            var terminal = new FakeTerminal(8, 3);
            var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red, Blue], 1, 2);
            surface.StartFrame();
            picture.Draw(surface.Frame);
            surface.Build();

            var drawn = terminal.Written.Length;

            surface.StartFrame();
            surface.Frame.Write(0, 0, "kept", Theme.Default);
            surface.Build();

            var written = terminal.Written[drawn..];

            Assert.True(
                written.IndexOf("\eP", StringComparison.Ordinal) <
                written.IndexOf("kept", StringComparison.Ordinal),
                "the undraw has to go out before the cells it paints over");
        }
        finally
        {
            TerminalCapabilities.Background = was;
        }
    }

    [Fact]
    public void APictureThatStopsBeingDrawnIsUndrawnAllTheSame()
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(6, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show([Red, Blue], 1, 2);
        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        var drawn = terminal.Written.Length;
        var image = Image(terminal.Written);

        surface.StartFrame();
        surface.Build();

        Assert.Contains(
            $"\e_Ga=d,d=i,i={image},q=2\e\\",
            terminal.Written[drawn..],
            StringComparison.Ordinal);
    }

    [Fact]
    public void ClearingASixelPicturePaintsOverItInTheColourTheTerminalNamed()
    {
        var was = TerminalCapabilities.Background;

        try
        {
            TerminalCapabilities.Background = new(255, 255, 255);

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red, Blue], 1, 2);

            var written = Undrawn(picture, 4, 2, picture.Clear);

            Assert.Contains("#0;2;100;100;100", written, StringComparison.Ordinal);
            Assert.Contains('~', written);
        }
        finally
        {
            TerminalCapabilities.Background = was;
        }
    }

    [Fact]
    public void ASixelPictureIsLeftWhereItIsWhenTheTerminalNeverSaidItsColour()
    {
        var was = TerminalCapabilities.Background;

        try
        {
            TerminalCapabilities.Background = null;

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red, Blue], 1, 2);
            Draw(picture, 4, 2);
            picture.Clear();

            Assert.DoesNotContain("\eP", Draw(picture, 4, 2), StringComparison.Ordinal);
        }
        finally
        {
            TerminalCapabilities.Background = was;
        }
    }

    [Fact]
    public void ClearingAPictureThatNeverReachedTheTerminalSaysNothing()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show([Red, Blue], 1, 2);
        picture.Clear();

        Assert.DoesNotContain("a=d", Draw(picture, 4, 2), StringComparison.Ordinal);
    }

    [Fact]
    public void APictureTooBigForOneChunkIsSentInSeveral()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show(new Rgb[64 * 64], 64, 64);

        var written = Draw(picture, 20, 10);
        var chunks = written.Split("\e_G").Length - 1;

        Assert.Equal(4, chunks);
        Assert.Equal(3, Occurrences(written, "m=1;"));
        Assert.Equal(1, Occurrences(written, "m=0;"));
    }

    [Fact]
    public void ThePayloadLandsWhereThePictureWasPlaced()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show(new Rgb[2 * 8], 2, 8);

        Assert.Contains("\e[1;4H\e_G", Draw(picture, 9, 4), StringComparison.Ordinal);
    }

    [Fact]
    public void TheApplicationsOwnChoiceIsWhatItFallsBackTo()
    {
        var was = Glyphs.Picture;

        try
        {
            Glyphs.Picture = ImageProtocol.Kitty;

            var picture = new Picture();

            picture.Show([Red, Blue], 1, 2);

            Assert.Contains("\e_G", Draw(picture, 4, 2), StringComparison.Ordinal);
        }
        finally
        {
            Glyphs.Picture = was;
        }
    }

    [Fact]
    public void TheSixelProtocolStatesItsSizeAndItsColours()
    {
        var picture = new Picture { Protocol = ImageProtocol.Sixel };

        picture.Show([Red, Blue], 1, 2);

        var written = Draw(picture, 2, 2);

        Assert.Contains($"\ePq\"1;1;{2 * Glyphs.CellWidth};{2 * Glyphs.CellHeight}", written, StringComparison.Ordinal);
        Assert.Contains(";2;100;0;0", written, StringComparison.Ordinal);
        Assert.Contains(";2;0;0;100", written, StringComparison.Ordinal);
        Assert.EndsWith("\e\\", written.TrimEnd(), StringComparison.Ordinal);
        Assert.DoesNotContain('▀', written);
    }

    [Fact]
    public void SixelColoursAreThePicturesOwnRatherThanAFixedCube()
    {
        var picture = new Picture { Protocol = ImageProtocol.Sixel };

        picture.Show([new(200, 100, 50)], 1, 1);

        Assert.Contains("#0;2;78;39;20", Draw(picture, 1, 1), StringComparison.Ordinal);
    }

    [Fact]
    public void SixelSpendsEveryRegisterOnAPictureWithMoreColoursThanThat()
    {
        var pixels = new Rgb[64 * 64];

        for (var index = 0; index < pixels.Length; index++)
        {
            pixels[index] = new((byte)(index / 16), (byte)(index % 251), (byte)(index % 199));
        }

        var picture = new Picture { Protocol = ImageProtocol.Sixel };

        picture.Show(pixels, 64, 64);

        var written = Draw(picture, 32, 16);

        Assert.Contains("#255;2;", written, StringComparison.Ordinal);
        Assert.DoesNotContain("#256", written, StringComparison.Ordinal);
    }

    [Fact]
    public void ShrinkingAPictureAveragesThePixelsCoveredRatherThanDroppingThem()
    {
        var was = (Glyphs.CellWidth, Glyphs.CellHeight);

        try
        {
            Glyphs.CellWidth = 1;
            Glyphs.CellHeight = 1;

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red, Blue, Blue, Red], 2, 2);

            Assert.Contains("#0;2;50;0;50", Draw(picture, 1, 1), StringComparison.Ordinal);
        }
        finally
        {
            (Glyphs.CellWidth, Glyphs.CellHeight) = was;
        }
    }

    [Fact]
    public void ANewPictureReplacesThePayloadBuiltForTheLastOne()
    {
        var picture = new Picture { Protocol = ImageProtocol.Sixel };

        picture.Show([Red], 1, 1);

        var first = Draw(picture, 4, 4);

        Assert.Equal(first, Draw(picture, 4, 4));

        picture.Show([Blue], 1, 1);

        Assert.NotEqual(first, Draw(picture, 4, 4));
    }

    [Fact]
    public void SixelPutsTheRowsOutInBandsOfSix()
    {
        var picture = new Picture { Protocol = ImageProtocol.Sixel };

        picture.Show([Red], 1, 1);

        var written = Draw(picture, 1, 1);

        Assert.Equal((Glyphs.CellHeight + 5) / 6, Occurrences(written, "-"));
        Assert.Contains('!', written);
    }

    [Fact]
    public void ACellIsAsManyPixelsAsTheApplicationSays()
    {
        var was = Glyphs.CellWidth;

        try
        {
            Glyphs.CellWidth = 4;

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red], 1, 1);

            Assert.Contains("\ePq\"1;1;4;", Draw(picture, 1, 1), StringComparison.Ordinal);
        }
        finally
        {
            Glyphs.CellWidth = was;
        }
    }

    private static string Image(string written)
    {
        var at = written.IndexOf("i=", StringComparison.Ordinal) + 2;
        var end = written.IndexOf(',', at);

        return written[at..end];
    }

    private static int Occurrences(string text, string what)
    {
        var found = 0;
        var at = text.IndexOf(what, StringComparison.Ordinal);

        while (at >= 0)
        {
            found++;
            at = text.IndexOf(what, at + what.Length, StringComparison.Ordinal);
        }

        return found;
    }

    private static string Undrawn(Picture picture, int width, int height, Action between)
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        var drawn = terminal.Written.Length;

        between();

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        return terminal.Written[drawn..];
    }

    private static string Draw(Picture picture, int width, int height)
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        return terminal.Written;
    }
}
