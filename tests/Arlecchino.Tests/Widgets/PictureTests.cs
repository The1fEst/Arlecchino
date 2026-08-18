using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Rendering.Terminals;
using Arlecchino.Testing;
using Arlecchino.Widgets.Pictures;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

public sealed class PictureTests
{
    private static readonly Rgb Red = new(255, 0, 0);
    private static readonly Rgb Blue = new(0, 0, 255);

    [Fact]
    public void OneCellCarriesTwoPixels()
    {
        var picture = new Picture();

        picture.Show([Red, Blue], 1, 2);

        var output = Draw(picture, 1, 1);

        Assert.Contains("38;2;255;0;0", output, StringComparison.Ordinal);
        Assert.Contains("48;2;0;0;255", output, StringComparison.Ordinal);
        Assert.Contains('▀', output);
    }

    [Fact]
    public void APictureIsNotStretchedToFillTheRegion()
    {
        var picture = new Picture();

        picture.Show(new Rgb[100 * 50], 100, 50);

        var lines = FrameText.Lines(Draw(picture, 40, 40));
        var frame = 0;

        foreach (var line in lines)
        {
            if (!line.Contains('▀', StringComparison.Ordinal))
            {
                continue;
            }

            frame++;
            Assert.Equal(40, line.TrimEnd().Length);
        }

        Assert.Equal(10, frame);
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

        var output = Draw(picture, 9, 4);

        Assert.Contains(Theme.Info.Ansi, output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheKittyProtocolSendsThePixelsThemselves()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show([Red, Blue], 1, 2);

        var output = Draw(picture, 4, 2);

        Assert.Contains("\e_Ga=T,q=2,f=24,i=", output, StringComparison.Ordinal);
        Assert.Contains(",s=1,v=2,c=2,r=2,m=0;", output, StringComparison.Ordinal);
        Assert.Contains(Convert.ToBase64String([255, 0, 0, 0, 0, 255]), output, StringComparison.Ordinal);
        Assert.EndsWith("\e\\", output.TrimEnd(), StringComparison.Ordinal);
        Assert.DoesNotContain('▀', output);
    }

    /// <summary>
    /// A terminal cannot show more than the cells it was given, so a picture larger than them is
    /// brought down before it goes out.
    /// </summary>
    [Fact]
    public void APictureLargerThanItsCellsIsBroughtDownBeforeItIsSent()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show(new Rgb[2000 * 1000], 2000, 1000);

        var output = Draw(picture, 40, 10);

        Assert.Contains($",s={40 * Glyphs.CellWidth},v={10 * Glyphs.CellHeight},c=40,r=10,", output, StringComparison.Ordinal);
        Assert.True(output.Length < 2000 * 1000, $"the payload is {output.Length} bytes");
    }

    /// <summary>
    /// A wide pane on a fine screen is still megabytes of escape sequence, so a ceiling stands above
    /// the cells: past it a little sharpness is given up for a picture that appears at once.
    /// </summary>
    [Fact]
    public void APictureIsNotSentInMoreDetailThanTheCeilingAllows()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty, Detail = 8 * 1024 };

        picture.Show(new Rgb[2000 * 1000], 2000, 1000);

        var size = Regex.Match(Draw(picture, 200, 50), @",s=(\d+),v=(\d+),");

        Assert.True(size.Success, "the payload states no size");

        var columns = int.Parse(size.Groups[1].Value, CultureInfo.InvariantCulture);
        var down = int.Parse(size.Groups[2].Value, CultureInfo.InvariantCulture);

        Assert.True(columns * down <= 8 * 1024, $"{columns}×{down} is past the ceiling");
        Assert.True(columns > down, "the shape of the picture is kept");
    }

    /// <summary>A picture smaller than its cells is sent as it is, for the terminal to enlarge.</summary>
    [Fact]
    public void APictureSmallerThanItsCellsIsSentAsItIs()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show(new Rgb[4 * 4], 4, 4);

        Assert.Contains(",s=4,v=4,", Draw(picture, 40, 10), StringComparison.Ordinal);
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
        var output = Undrawn(picture, 4, 2, picture.Clear);

        Assert.Contains($"\e_Ga=d,d=i,i={image},q=2\e\\", output, StringComparison.Ordinal);
    }

    [Fact]
    public void SwitchingAwayFromKittyTellsTheTerminalToLetGoOfThePicture()
    {
        var picture = new Picture { Protocol = ImageProtocol.Kitty };

        picture.Show([Red, Blue], 1, 2);

        var image = Image(Draw(picture, 4, 2));
        var output = Undrawn(picture, 4, 2, () => picture.Protocol = ImageProtocol.Blocks);

        Assert.Contains($"\e_Ga=d,d=i,i={image},q=2\e\\", output, StringComparison.Ordinal);
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

        var frame = terminal.WrittenText.Length;

        surface.ForgetPreviousFrame();
        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        Assert.Contains("\e_G", terminal.WrittenText[frame..], StringComparison.Ordinal);
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

        var frame = terminal.WrittenText.Length;

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        Assert.Contains("\e_G", terminal.WrittenText[frame..], StringComparison.Ordinal);
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

        var frame = terminal.WrittenText.Length;

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        Assert.Equal(frame, terminal.WrittenText.Length);
    }

    [Fact]
    public void TheLastBandOfAnUndrawPaintsOnlyTheRowsThePictureHad()
    {
        var original = (TerminalCapabilities.Background, Glyphs.CellWidth, Glyphs.CellHeight);

        try
        {
            TerminalCapabilities.Background = new(0, 0, 0);
            Glyphs.CellWidth = 1;
            Glyphs.CellHeight = 8;

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red], 1, 1);

            var output = Undrawn(picture, 1, 1, picture.Clear);

            Assert.Contains("!1~-", output, StringComparison.Ordinal);
            Assert.Contains("!1B-", output, StringComparison.Ordinal);
        }
        finally
        {
            (TerminalCapabilities.Background, Glyphs.CellWidth, Glyphs.CellHeight) = original;
        }
    }

    [Fact]
    public void WhatIsUndrawnIsPaintedOverBeforeTheFrameIsDrawnOverIt()
    {
        var original = TerminalCapabilities.Background;

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

            var frame = terminal.WrittenText.Length;

            surface.StartFrame();
            surface.Frame.Write(0, 0, "kept", Theme.Default);
            surface.Build();

            var output = terminal.WrittenText[frame..];

            Assert.True(
                output.IndexOf("\eP", StringComparison.Ordinal) <
                output.IndexOf("kept", StringComparison.Ordinal),
                "the undraw has to go out before the cells it paints over");
        }
        finally
        {
            TerminalCapabilities.Background = original;
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

        var frame = terminal.WrittenText.Length;
        var image = Image(terminal.WrittenText);

        surface.StartFrame();
        surface.Build();

        Assert.Contains(
            $"\e_Ga=d,d=i,i={image},q=2\e\\",
            terminal.WrittenText[frame..],
            StringComparison.Ordinal);
    }

    [Fact]
    public void ClearingASixelPicturePaintsOverItInTheColourTheTerminalNamed()
    {
        var original = TerminalCapabilities.Background;

        try
        {
            TerminalCapabilities.Background = new(255, 255, 255);

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red, Blue], 1, 2);

            var output = Undrawn(picture, 4, 2, picture.Clear);

            Assert.Contains("#0;2;100;100;100", output, StringComparison.Ordinal);
            Assert.Contains('~', output);
        }
        finally
        {
            TerminalCapabilities.Background = original;
        }
    }

    [Fact]
    public void ASixelPictureIsLeftWhereItIsWhenTheTerminalNeverSaidItsColour()
    {
        var original = TerminalCapabilities.Background;

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
            TerminalCapabilities.Background = original;
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

        var output = Draw(picture, 20, 10);
        var chunks = output.Split("\e_G").Length - 1;

        Assert.Equal(4, chunks);
        Assert.Equal(3, Occurrences(output, "m=1;"));
        Assert.Equal(1, Occurrences(output, "m=0;"));
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
        var original = Glyphs.Picture;

        try
        {
            Glyphs.Picture = ImageProtocol.Kitty;

            var picture = new Picture();

            picture.Show([Red, Blue], 1, 2);

            Assert.Contains("\e_G", Draw(picture, 4, 2), StringComparison.Ordinal);
        }
        finally
        {
            Glyphs.Picture = original;
        }
    }

    [Fact]
    public void TheSixelProtocolStatesItsSizeAndItsColours()
    {
        var picture = new Picture { Protocol = ImageProtocol.Sixel };

        picture.Show([Red, Blue], 1, 2);

        var output = Draw(picture, 2, 2);

        Assert.Contains($"\ePq\"1;1;{2 * Glyphs.CellWidth};{2 * Glyphs.CellHeight}", output, StringComparison.Ordinal);
        Assert.Contains(";2;100;0;0", output, StringComparison.Ordinal);
        Assert.Contains(";2;0;0;100", output, StringComparison.Ordinal);
        Assert.EndsWith("\e\\", output.TrimEnd(), StringComparison.Ordinal);
        Assert.DoesNotContain('▀', output);
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

        var output = Draw(picture, 32, 16);

        Assert.Contains("#255;2;", output, StringComparison.Ordinal);
        Assert.DoesNotContain("#256", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ShrinkingAPictureAveragesThePixelsCoveredRatherThanDroppingThem()
    {
        var original = (Glyphs.CellWidth, Glyphs.CellHeight);

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
            (Glyphs.CellWidth, Glyphs.CellHeight) = original;
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

        var output = Draw(picture, 1, 1);

        Assert.Equal((Glyphs.CellHeight + 5) / 6, Occurrences(output, "-"));
        Assert.Contains('!', output);
    }

    [Fact]
    public void ACellIsAsManyPixelsAsTheApplicationSays()
    {
        var original = Glyphs.CellWidth;

        try
        {
            Glyphs.CellWidth = 4;

            var picture = new Picture { Protocol = ImageProtocol.Sixel };

            picture.Show([Red], 1, 1);

            Assert.Contains("\ePq\"1;1;4;", Draw(picture, 1, 1), StringComparison.Ordinal);
        }
        finally
        {
            Glyphs.CellWidth = original;
        }
    }

    private static string Image(string output)
    {
        var at = output.IndexOf("i=", StringComparison.Ordinal) + 2;
        var end = output.IndexOf(',', at);

        return output[at..end];
    }

    private static int Occurrences(string text, string label)
    {
        var match = 0;
        var at = text.IndexOf(label, StringComparison.Ordinal);

        while (at >= 0)
        {
            match++;
            at = text.IndexOf(label, at + label.Length, StringComparison.Ordinal);
        }

        return match;
    }

    private static string Undrawn(Picture picture, int width, int height, Action act)
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        var frame = terminal.WrittenText.Length;

        act();

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        return terminal.WrittenText[frame..];
    }

    private static string Draw(Picture picture, int width, int height)
    {
        using var truecolor = new ColorSupportScope(ColorSupport.TrueColor);
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        picture.Draw(surface.Frame);
        surface.Build();

        return terminal.WrittenText;
    }
}
