using System.IO;
using Arlecchino.Pictures;
using Arlecchino.Pictures.Formats.Bmp;
using Xunit;

namespace Arlecchino.Tests.Pictures;

public sealed class BmpTests
{
    /// <summary>The rows are written from the bottom up, and each color is written blue first.</summary>
    [Fact]
    public void TheLastRowWrittenIsTheTopOfThePicture()
    {
        var raster = Read(Written(
            2,
            2,
            24,
            [
                0, 0, 255, 0, 255, 0, 0, 0,
                255, 0, 0, 255, 255, 255, 0, 0,
            ]));

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 255), raster.Pixels[0]);
        Assert.Equal(new(255, 255, 255), raster.Pixels[1]);
        Assert.Equal(new(255, 0, 0), raster.Pixels[2]);
        Assert.Equal(new(0, 255, 0), raster.Pixels[3]);
    }

    /// <summary>A negative height asks for the rows the other way round.</summary>
    [Fact]
    public void ANegativeHeightPutsTheFirstRowOnTop()
    {
        var raster = Read(Written(
            1,
            2,
            24,
            [
                10, 20, 30, 0,
                40, 50, 60, 0,
            ],
            topDown: true));

        Assert.NotNull(raster);
        Assert.Equal(new(30, 20, 10), raster.Pixels[0]);
        Assert.Equal(new(60, 50, 40), raster.Pixels[1]);
    }

    [Fact]
    public void APaletteIsLookedUpRatherThanRead()
    {
        var raster = Read(Written(
            2,
            1,
            8,
            [1, 0, 0, 0],
            palette: [7, 8, 9, 0, 1, 2, 3, 0]));

        Assert.NotNull(raster);
        Assert.Equal(new(3, 2, 1), raster.Pixels[0]);
        Assert.Equal(new(9, 8, 7), raster.Pixels[1]);
    }

    /// <summary>Sixteen bits hold five for each color, and five bits deep is white rather than thirty-one.</summary>
    [Fact]
    public void SixteenBitsAreStretchedToWholeBytes()
    {
        var raster = Read(Written(1, 1, 16, [0x1F, 0x7C, 0, 0]));

        Assert.NotNull(raster);
        Assert.Equal(new(255, 0, 255), raster.Pixels[0]);
    }

    /// <summary>
    /// A run says how many times to repeat one entry; a nought and a three or more says that many entries
    /// follow, padded out to a pair of bytes; a nought and a nought ends the row.
    /// </summary>
    [Fact]
    public void RunLengthEncodedRowsAreUnpacked()
    {
        var raster = Read(Written(
            5,
            1,
            8,
            [2, 1, 0, 3, 2, 0, 2, 0, 0, 0, 0, 1],
            palette: [0, 0, 0, 0, 10, 0, 0, 0, 0, 20, 0, 0],
            compression: 1));

        Assert.NotNull(raster);
        Assert.Equal(new(0, 0, 10), raster.Pixels[0]);
        Assert.Equal(new(0, 0, 10), raster.Pixels[1]);
        Assert.Equal(new(0, 20, 0), raster.Pixels[2]);
        Assert.Equal(new(0, 0, 0), raster.Pixels[3]);
        Assert.Equal(new(0, 20, 0), raster.Pixels[4]);
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new[] { (byte)'B', (byte)'M' })]
    [InlineData(new byte[] { (byte)'M', (byte)'Z', 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 })]
    public void WhatIsNotABitmapComesBackAsNothing(byte[] bytes)
    {
        Assert.Null(Read(bytes));
    }

    /// <summary>The rows are as long as the header says, and a file that stops short is not half drawn.</summary>
    [Fact]
    public void ATruncatedBitmapComesBackAsNothing()
    {
        var whole = Written(2, 2, 24, [0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0]);

        Assert.Null(Read(whole[..^4]));
    }

    private static Raster? Read(byte[] bytes) => new Bmp().Read(bytes, PictureLimits.Default);

    /// <summary>Writes a bitmap by hand.</summary>
    /// <param name="width">How wide.</param>
    /// <param name="height">How tall.</param>
    /// <param name="bits">Bits a pixel.</param>
    /// <param name="rows">The rows, already padded out to four bytes each.</param>
    /// <param name="palette">The palette, four bytes an entry, blue first.</param>
    /// <param name="compression">Nought for plain rows, one for run-length encoded ones.</param>
    /// <param name="topDown">Whether to ask for the first row to be the top one.</param>
    /// <returns>The file.</returns>
    private static byte[] Written(
        int width,
        int height,
        int bits,
        byte[] rows,
        byte[]? palette = null,
        int compression = 0,
        bool topDown = false)
    {
        var file = new MemoryStream();
        var offset = 14 + 40 + (palette?.Length ?? 0);

        file.Write("BM"u8);
        Little(file, offset + rows.Length);
        Little(file, 0);
        Little(file, offset);
        Little(file, 40);
        Little(file, width);
        Little(file, topDown ? -height : height);
        file.Write([1, 0]);
        file.Write([(byte)bits, 0]);
        Little(file, compression);
        Little(file, rows.Length);
        Little(file, 0);
        Little(file, 0);
        Little(file, palette is null ? 0 : palette.Length / 4);
        Little(file, 0);

        if (palette is not null)
        {
            file.Write(palette);
        }

        file.Write(rows);

        return file.ToArray();
    }

    private static void Little(Stream file, int value) =>
        file.Write([(byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24)]);
}
