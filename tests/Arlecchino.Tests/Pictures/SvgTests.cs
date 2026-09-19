using System.Text;
using Arlecchino.Pictures;
using Arlecchino.Pictures.Formats.Svg;
using Arlecchino.Rendering.Colors;
using Xunit;

namespace Arlecchino.Tests.Pictures;

/// <summary>
///     The flat documents a chart is written as, read into pixels. A picture drawn from shapes has no one
///     size, so every fact here asks for it at the size it names and reads the pixels back at that size.
/// </summary>
public sealed class SvgTests
{
    private const string Head = """<svg xmlns="http://www.w3.org/2000/svg" width="10" height="10">""";

    [Fact]
    public void AFileIsClaimedByTheElementItOpensWith()
    {
        Assert.Equal("svg", PictureFormats.For(Bytes($"{Head}<rect width='10' height='10'/></svg>"))?.Name);
    }

    [Fact]
    public void AFileThatOnlyLooksLikeMarkupIsNotClaimed()
    {
        Assert.Null(PictureFormats.For(Bytes("<html><body>no picture here</body></html>")));
    }

    [Fact]
    public void ADocumentDeclarationBeforeTheOpeningElementIsStillClaimed()
    {
        var format = PictureFormats.For(
            Bytes($"<?xml version='1.0'?>\n<!-- a picture -->\n{Head}<rect width='10' height='10'/></svg>"));

        Assert.Equal("svg", format?.Name);
    }

    [Fact]
    public void ARectangleFillsWhatItCovers()
    {
        var raster = Read($"{Head}<rect x='0' y='0' width='5' height='10' fill='#ff0000'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(255, 0, 0), At(raster, 2, 2));
        Assert.Equal(new(255, 255, 255), At(raster, 2, 7));
    }

    [Fact]
    public void AShapeFilledWithNothingLeavesTheGroundShowing()
    {
        var raster = Read($"{Head}<rect x='0' y='0' width='10' height='10' fill='none'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(255, 255, 255), At(raster, 5, 5));
    }

    [Fact]
    public void AColorCanBeNamedRatherThanWritten()
    {
        var raster = Read($"{Head}<rect x='0' y='0' width='10' height='10' fill='blue'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 255), At(raster, 5, 5));
    }

    [Fact]
    public void AColorThisCannotReadRefusesTheWholeFile()
    {
        Assert.Null(Read($"{Head}<rect width='10' height='10' fill='rgb(1,2,3)'/></svg>"));
    }

    [Fact]
    public void ALineIsDrawnWhereItWasPut()
    {
        var raster = Read($"{Head}<line x1='5' y1='0' x2='5' y2='10' stroke='#000000' stroke-width='2'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 0), At(raster, 5, 5));
        Assert.Equal(new(255, 255, 255), At(raster, 5, 1));
    }

    /// <summary>
    ///     A pattern is a list of lengths the document is free to write a nought into, and one walked by
    ///     how far the line has come rather than by where the pattern is would be walked forever.
    /// </summary>
    [Fact]
    public void ADashedLineLeavesGapsAndIsDrawnAtAll()
    {
        var raster = Read(
            $"{Head}<line x1='5' y1='0' x2='5' y2='10' stroke='#000000' stroke-width='2' " +
            "stroke-dasharray='2 2'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 0), At(raster, 1, 5));
        Assert.Equal(new(255, 255, 255), At(raster, 3, 5));
    }

    [Fact]
    public void ADashPatternOfNothingButNoughtsDrawsASolidLine()
    {
        var raster = Read(
            $"{Head}<line x1='5' y1='0' x2='5' y2='10' stroke='#000000' stroke-width='2' " +
            "stroke-dasharray='0 0'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 0), At(raster, 1, 5));
        Assert.Equal(new(0, 0, 0), At(raster, 3, 5));
    }

    [Fact]
    public void ACircleIsRound()
    {
        var raster = Read($"{Head}<circle cx='5' cy='5' r='4' fill='#000000'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 0), At(raster, 5, 5));
        Assert.Equal(new(255, 255, 255), At(raster, 0, 0));
    }

    /// <summary>A run of lines is a line rather than a shape, so nothing is filled in behind it.</summary>
    [Fact]
    public void APolylineIsDrawnAlongRatherThanFilledIn()
    {
        var raster = Read($"{Head}<polyline points='1,1 9,1 9,9' stroke='#000000' stroke-width='2'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 0), At(raster, 1, 5));
        Assert.Equal(new(255, 255, 255), At(raster, 5, 4));
    }

    [Fact]
    public void APolygonIsFilledIn()
    {
        var raster = Read($"{Head}<polygon points='0,0 10,0 10,10 0,10' fill='#000000'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 0), At(raster, 5, 5));
    }

    [Fact]
    public void AFileCarryingSomethingThisCannotDrawIsRefusedWhole()
    {
        Assert.Null(Read($"{Head}<rect width='10' height='10' fill='#000000'/><path d='M0 0 L5 5'/></svg>"));
        Assert.Null(Read($"{Head}<text x='1' y='1'>said</text></svg>"));
        Assert.Null(Read($"{Head}<rect width='10' height='10' fill='#000000' transform='rotate(45)'/></svg>"));
    }

    [Fact]
    public void ShapesWrittenWithoutQuotesAreStillRead()
    {
        var raster = Read("<svg xmlns='x' width=10 height=10><rect width=10 height=10 fill=#000000/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 0), At(raster, 5, 5));
    }

    /// <summary>A comment may hold an element of its own, and that is no element the document drew.</summary>
    [Fact]
    public void WhatIsCommentedOutIsNotDrawn()
    {
        var raster = Read(
            $"{Head}<!-- <rect width='10' height='10' fill='#000000'/> -->" +
            "<rect width='10' height='10' fill='blue'/></svg>");

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 255), At(raster, 5, 5));
    }

    [Fact]
    public void AFileWithNothingToDrawIsRefused()
    {
        Assert.Null(Read($"{Head}</svg>"));
    }

    /// <summary>A size given as a share of the page says nothing here, so the box it holds is taken.</summary>
    [Fact]
    public void ASizeGivenAsAShareFallsBackToTheBox()
    {
        var raster = Read(
            "<svg xmlns='http://www.w3.org/2000/svg' width='100%' height='100%' viewBox='0 0 8 4'>" +
            "<rect width='8' height='4' fill='#000000'/></svg>",
            32);

        Assert.NotNull(raster);
        Assert.Equal(2, raster.Width / raster.Height);
    }

    /// <summary>
    ///     Being drawn from shapes rather than from pixels, a picture asked for larger comes out sharper
    ///     rather than enlarged.
    /// </summary>
    [Fact]
    public void APictureAskedForLargerIsDrawnLarger()
    {
        var small = Read($"{Head}<rect width='10' height='10' fill='#000000'/></svg>");
        var large = Read($"{Head}<rect width='10' height='10' fill='#000000'/></svg>", 10000);

        Assert.NotNull(small);
        Assert.NotNull(large);
        Assert.Equal(10, small.Width);
        Assert.Equal(80, large.Width);
    }

    private static RGB At(Raster raster, int row, int column) => raster.Pixels[(row * raster.Width) + column];

    private static Raster? Read(string text, int pixels = 100) =>
        new Svg().Read(Bytes(text), PictureLimits.For(pixels));

    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
}
