using System;
using Arlecchino.Rendering.Colors;
using Xunit;

namespace Arlecchino.Tests.Rendering;

/// <summary>
/// The colors a palette is worked out with. What is asserted is the arithmetic rather than the taste:
/// that a hue survives being darkened, and that a ladder written for one background survives another.
/// </summary>
public sealed class DerivedColorTests
{
    private static readonly Rgb Ink = new(0x14, 0x13, 0x17);
    private static readonly Rgb Paper = new(0xFF, 0xFF, 0xFF);
    private static readonly Rgb Middle = new(0x80, 0x80, 0x80);

    [Theory]
    [InlineData(0x14, 0x13, 0x17)]
    [InlineData(0xED, 0xE6, 0xD9)]
    [InlineData(0xC9, 0x38, 0x2B)]
    [InlineData(0x00, 0x00, 0x00)]
    [InlineData(0xFF, 0xFF, 0xFF)]
    [InlineData(0x4E, 0x7A, 0x63)]
    public void AColorComesBackFromTheOtherSpaceAsItself(byte red, byte green, byte blue)
    {
        var color = new Rgb(red, green, blue);

        Assert.Equal(color, Oklch.Of(color).ToRgb());
    }

    /// <summary>The two ends of the scale, which every other ratio sits between.</summary>
    [Fact]
    public void BlackOnWhiteIsTheMostContrastThereIs()
    {
        Assert.Equal(21d, Contrast.Between(new(0x00, 0x00, 0x00), Paper), 2);
        Assert.Equal(1d, Contrast.Between(Ink, Ink), 6);
    }

    /// <summary>
    /// The point the light and dark themes turn on is where white and black are equally far off, which is
    /// a good deal darker than the halfway gray a reader would guess at.
    /// </summary>
    [Fact]
    public void ThePivotIsWhereWhiteAndBlackAreEquallyFarOff()
    {
        Assert.Equal(0.1791d, Contrast.Pivot, 4);

        Assert.True(Contrast.IsDark(Ink));
        Assert.False(Contrast.IsDark(Paper));
    }

    /// <summary>
    /// The ladder this application draws with, asked for by hue, chroma and contrast, and landing back on
    /// the colors it was drawn with. Nothing else says the arithmetic describes the design rather than
    /// replacing it.
    /// </summary>
    [Theory]
    [InlineData(83.1, 0.019, 14.91, 0xED, 0xE6, 0xD9)]
    [InlineData(84.6, 0.006, 10.51, 0xC5, 0xC3, 0xBF)]
    [InlineData(91.5, 0.009, 8.63, 0xB3, 0xB1, 0xAB)]
    [InlineData(87.5, 0.011, 7.00, 0xA2, 0x9F, 0x98)]
    [InlineData(84.6, 0.013, 5.82, 0x94, 0x90, 0x88)]
    [InlineData(84.6, 0.016, 4.91, 0x88, 0x83, 0x79)]
    [InlineData(82.4, 0.015, 4.22, 0x7D, 0x78, 0x6F)]
    [InlineData(76.5, 0.013, 3.30, 0x6C, 0x67, 0x60)]
    public void TheLadderComesBackFromWhatItIsMadeOf(
        double hue,
        double chroma,
        double contrast,
        byte red,
        byte green,
        byte blue)
    {
        var design = new Rgb(red, green, blue);
        var answer = Shade.Against(Ink, hue, chroma, contrast);

        Assert.InRange(Math.Abs(answer.Red - design.Red), 0, 2);
        Assert.InRange(Math.Abs(answer.Green - design.Green), 0, 2);
        Assert.InRange(Math.Abs(answer.Blue - design.Blue), 0, 2);
    }

    /// <summary>
    /// A color asked for against a background it was not drawn for keeps its hue. A red that turns brown on
    /// a white terminal is a different brand, not a darker one.
    /// </summary>
    [Fact]
    public void AnAccentKeepsItsHueOnEitherBackground()
    {
        var crimson = new Rgb(0xC9, 0x38, 0x2B);
        var hue = Oklch.Of(crimson).Hue;

        var onInk = Oklch.Of(Shade.Against(Ink, crimson, 3.6d)).Hue;
        var onPaper = Oklch.Of(Shade.Against(Paper, crimson, 4.5d)).Hue;

        Assert.InRange(Math.Abs(onInk - hue), 0d, 2d);
        Assert.InRange(Math.Abs(onPaper - hue), 0d, 2d);
    }

    /// <summary>
    /// What is asked for is what is reached, to within a tenth. The answer is an 8-bit color, and one step
    /// of a channel is worth about that much of the ratio.
    /// </summary>
    /// <param name="contrast">The ratio to ask for.</param>
    [Theory]
    [InlineData(3.5)]
    [InlineData(7.0)]
    [InlineData(12.0)]
    public void TheContrastAskedForIsTheContrastReached(double contrast)
    {
        var onInk = Contrast.Between(Shade.Against(Ink, 83d, 0.014d, contrast), Ink);
        var onPaper = Contrast.Between(Shade.Against(Paper, 83d, 0.014d, contrast), Paper);

        Assert.InRange(onInk, contrast - 0.1d, contrast + 0.1d);
        Assert.InRange(onPaper, contrast - 0.1d, contrast + 0.1d);
    }

    /// <summary>A surface is lifted off the terminal, whichever way off it happens to be.</summary>
    [Fact]
    public void ASurfaceIsLiftedAwayFromTheBackgroundItSitsOn()
    {
        Assert.True(Oklch.Of(Shade.Lifted(Ink, 0.07d)).Lightness > Oklch.Of(Ink).Lightness);
        Assert.True(Oklch.Of(Shade.Lifted(Paper, 0.07d)).Lightness < Oklch.Of(Paper).Lightness);
    }

    /// <summary>
    /// A background near the middle has no room for a ladder written for black, so the ladder is brought
    /// down to what there is. Without it the top steps land on the same color and the design goes flat.
    /// </summary>
    [Fact]
    public void ALadderIsBroughtDownToWhatTheBackgroundCanGive()
    {
        Assert.True(Contrast.Reach(Middle) < 6d);

        var top = Shade.Against(Middle, 83d, 0.014d, Shade.Scaled(14.91d, 3.30d, 14.91d, Middle));
        var next = Shade.Against(Middle, 83d, 0.014d, Shade.Scaled(10.51d, 3.30d, 14.91d, Middle));
        var last = Shade.Against(Middle, 83d, 0.014d, Shade.Scaled(3.30d, 3.30d, 14.91d, Middle));

        Assert.NotEqual(top, next);
        Assert.NotEqual(next, last);
        Assert.True(Contrast.Between(top, Middle) > Contrast.Between(next, Middle));
        Assert.True(Contrast.Between(next, Middle) > Contrast.Between(last, Middle));
    }

    /// <summary>A background with room to spare is left alone, so nothing is paid where nothing is needed.</summary>
    [Fact]
    public void ALadderWithRoomToSpareIsNotBroughtDown()
    {
        Assert.Equal(14.91d, Shade.Scaled(14.91d, 3.30d, 14.91d, Paper), 6);
        Assert.Equal(14.91d, Shade.Scaled(14.91d, 3.30d, 14.91d, Ink), 6);
    }

    /// <summary>A color too vivid for the screen is cut back in chroma, which is what keeps its hue.</summary>
    [Fact]
    public void AColorTooVividForTheScreenLosesChromaRatherThanHue()
    {
        var vivid = new Oklch(0.55d, 0.40d, 29d);

        Assert.False(vivid.FitsScreen);

        var answer = vivid.Trimmed();

        Assert.True(answer.FitsScreen);
        Assert.True(answer.Chroma < vivid.Chroma);
        Assert.Equal(vivid.Hue, answer.Hue, 6);
    }
}
