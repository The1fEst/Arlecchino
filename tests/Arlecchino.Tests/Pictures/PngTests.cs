using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Arlecchino.Pictures;
using Arlecchino.Pictures.Formats.Png;
using Xunit;

namespace Arlecchino.Tests.Pictures;

public sealed class PngTests
{
    [Fact]
    public void ATruecolourPictureIsReadPixelForPixel()
    {
        var raster = Read(Written(2,
            2,
            colour: 2,
            [
                [0, 255, 0, 0, 0, 255, 0],
                [0, 0, 0, 255, 255, 255, 255],
            ]));

        Assert.NotNull(raster);
        Assert.Equal(2, raster.Width);
        Assert.Equal(2, raster.Height);
        Assert.Equal(new(255, 0, 0), raster.Pixels[0]);
        Assert.Equal(new(0, 255, 0), raster.Pixels[1]);
        Assert.Equal(new(0, 0, 255), raster.Pixels[2]);
        Assert.Equal(new(255, 255, 255), raster.Pixels[3]);
    }

    /// <summary>Alpha is dropped: a terminal has nothing to show it against.</summary>
    [Fact]
    public void APictureWithAlphaKeepsItsColoursAndLosesTheAlpha()
    {
        var raster = Read(Written(1, 1, colour: 6, [[0, 10, 20, 30, 128]]));

        Assert.NotNull(raster);
        Assert.Equal(new(10, 20, 30), raster.Pixels[0]);
    }

    [Fact]
    public void AGreyPictureIsTheSameValueInAllThree()
    {
        var raster = Read(Written(1, 1, colour: 0, ["\0Z"u8.ToArray()]));

        Assert.NotNull(raster);
        Assert.Equal(new(90, 90, 90), raster.Pixels[0]);
    }

    [Fact]
    public void APaletteIsLookedUpRatherThanRead()
    {
        var raster = Read(Written(
            2,
            1,
            colour: 3,
            [[0, 1, 0]],
            palette: [1, 2, 3, 4, 5, 6]));

        Assert.NotNull(raster);
        Assert.Equal(4, raster.Pixels[0].Red);
        Assert.Equal(1, raster.Pixels[1].Red);
    }

    /// <summary>
    /// The filters are what a PNG is: each row says how it was written down against the one above.
    /// </summary>
    [Fact]
    public void EachRowFilterIsUndone()
    {
        var raster = Read(Written(2,
            3,
            colour: 2,
            [
                [0, 10, 10, 10, 20, 20, 20],
                [1, 10, 10, 10, 5, 5, 5],
                [2, 0, 0, 0, 0, 0, 0],
            ]));

        Assert.NotNull(raster);
        Assert.Equal(10, raster.Pixels[0].Red);
        Assert.Equal(20, raster.Pixels[1].Red);
        Assert.Equal(10, raster.Pixels[2].Red);
        Assert.Equal(15, raster.Pixels[3].Red);
        Assert.Equal(10, raster.Pixels[4].Red);
        Assert.Equal(15, raster.Pixels[5].Red);
    }

    /// <summary>A sample of sixteen bits is kept to the half of it a terminal can show.</summary>
    [Fact]
    public void SixteenBitsAreReadDownToEight()
    {
        var raster = Read(Written(1, 1, colour: 2, [[0, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC]], depth: 16));

        Assert.NotNull(raster);
        Assert.Equal(new(0x12, 0x56, 0x9A), raster.Pixels[0]);
    }

    /// <summary>Below eight bits the samples share a byte, and the deepest of them is white.</summary>
    /// <param name="depth">Bits a sample.</param>
    /// <param name="row">The one byte the row is written as.</param>
    /// <param name="first">What the first pixel should come out as.</param>
    /// <param name="second">What the second pixel should come out as.</param>
    [Theory]
    [InlineData(1, 0b0100_0000, 0, 255)]
    [InlineData(2, 0b0011_0000, 0, 255)]
    [InlineData(4, 0b0000_1111, 0, 255)]
    public void SamplesNarrowerThanAByteAreStretchedToOne(byte depth, byte row, byte first, byte second)
    {
        var raster = Read(Written(2, 1, colour: 0, [[0, row]], depth: depth));

        Assert.NotNull(raster);
        Assert.Equal(first, raster.Pixels[0].Red);
        Assert.Equal(second, raster.Pixels[1].Red);
    }

    /// <summary>
    /// An interlaced picture is written down in seven passes, each filling a grid with its own. Two by two
    /// is the smallest picture that lands in more than one of them: the first pass, the sixth and the
    /// seventh.
    /// </summary>
    [Fact]
    public void AnInterlacedPictureIsGatheredFromItsPasses()
    {
        var raster = Read(Written(2,
            2,
            colour: 0,
            [
                [0, 1],
                [0, 2],
                [0, 3, 4],
            ],
            depth: 8,
            interlacing: 1));

        Assert.NotNull(raster);
        Assert.Equal(1, raster.Pixels[0].Red);
        Assert.Equal(2, raster.Pixels[1].Red);
        Assert.Equal(3, raster.Pixels[2].Red);
        Assert.Equal(4, raster.Pixels[3].Red);
    }

    /// <summary>
    /// A file manager opens whatever it is pointed at, so nothing here may throw. What cannot be read
    /// comes back as nothing and is shown as bytes instead.
    /// </summary>
    /// <param name="bytes">What is being opened.</param>
    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 1, 2, 3 })]
    [InlineData(new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A })]
    public void WhatIsNotAPngComesBackAsNothing(byte[] bytes)
    {
        Assert.Null(Read(bytes));
    }

    [Fact]
    public void ATruncatedPngComesBackAsNothing()
    {
        var whole = Written(2,
            2,
            colour: 2,
            [
                [0, 1, 2, 3, 4, 5, 6],
                [0, 7, 8, 9, 10, 11, 12],
            ]);

        Assert.Null(Read(whole[..(whole.Length / 2)]));
    }

    /// <summary>
    /// A header states its own size, so a small file can ask for an enormous picture. The limit is read
    /// before anything is allocated against it.
    /// </summary>
    [Fact]
    public void APictureLargerThanTheLimitIsRefusedBeforeItIsRead()
    {
        var whole = Written(40_000, 40_000, colour: 2, [[0, 1, 2, 3]]);

        Assert.Null(new Png().Read(whole, PictureLimits.Default));
    }

    [Fact]
    public void TheSignatureIsRecognisedOnItsOwn()
    {
        var png = new Png();

        Assert.True(png.Starts([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A, 0, 0]));
        Assert.False(png.Starts([0x89, (byte)'P', (byte)'N', (byte)'G']));
        Assert.False(png.Starts("not a picture at all"u8));
    }

    private static Raster? Read(byte[] bytes) => new Png().Read(bytes, PictureLimits.Default);

    /// <summary>
    /// Writes a PNG by hand, which is the only way to hold a decoder to what it says it reads: the
    /// rows go in as the filter byte and then the bytes of the row.
    /// </summary>
    /// <param name="width">How wide.</param>
    /// <param name="height">How tall.</param>
    /// <param name="colour">The PNG color type.</param>
    /// <param name="rows">Each row, filter byte first.</param>
    /// <param name="palette">The palette, when the color type wants one.</param>
    /// <param name="depth">Bits a channel.</param>
    /// <param name="interlacing">Whether it claims to be interlaced.</param>
    /// <returns>The file.</returns>
    private static byte[] Written(
        int width,
        int height,
        int colour,
        byte[][] rows,
        byte[]? palette = null,
        byte depth = 8,
        byte interlacing = 0)
    {
        var file = new MemoryStream();

        file.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A]);

        var header = new byte[13];

        BigEndian(header, 0, width);
        BigEndian(header, 4, height);
        header[8] = depth;
        header[9] = (byte)colour;
        header[12] = interlacing;

        Chunk(file, "IHDR", header);

        if (palette is not null)
        {
            Chunk(file, "PLTE", palette);
        }

        var raw = new MemoryStream();

        foreach (var row in rows)
        {
            raw.Write(row);
        }

        var stream = new MemoryStream();

        using (var zip = new ZLibStream(stream, CompressionMode.Compress, leaveOpen: true))
        {
            zip.Write(raw.ToArray());
        }

        Chunk(file, "IDAT", stream.ToArray());
        Chunk(file, "IEND", []);

        return file.ToArray();
    }

    private static void Chunk(Stream file, string kind, byte[] body)
    {
        var length = new byte[4];

        BigEndian(length, 0, body.Length);

        file.Write(length);
        file.Write(Encoding.ASCII.GetBytes(kind));
        file.Write(body);
        file.Write("\0\0\0\0"u8);
    }

    private static void BigEndian(IList<byte> bytes, int at, int value)
    {
        bytes[at] = (byte)(value >> 24);
        bytes[at + 1] = (byte)(value >> 16);
        bytes[at + 2] = (byte)(value >> 8);
        bytes[at + 3] = (byte)value;
    }
}
