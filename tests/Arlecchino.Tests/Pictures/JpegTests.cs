using System.IO;
using Arlecchino.Pictures;
using Arlecchino.Pictures.Formats.Jpeg;
using Xunit;

namespace Arlecchino.Tests.Pictures;

public sealed class JpegTests
{
    /// <summary>
    /// A block whose only coefficient is the first one is a flat square. Eight, through a table of
    /// ones, is one step above the middle gray a sample is counted from.
    /// </summary>
    [Fact]
    public void ABlockOfOneCoefficientIsFlat()
    {
        var raster = Read(Written(0xC0, [0x43]));

        Assert.NotNull(raster);
        Assert.Equal(8, raster.Width);
        Assert.Equal(8, raster.Height);
        Assert.All(raster.Pixels, pixel => Assert.Equal(new(129, 129, 129), pixel));
    }

    /// <summary>
    /// A caller that will draw the picture small says so, and the file is read at a size to match. The
    /// value it lands on is the one the whole block averages to.
    /// </summary>
    /// <param name="enoughPixels">How many pixels the caller says it has a use for.</param>
    /// <param name="side">How wide and tall the picture should come out.</param>
    [Theory]
    [InlineData(64, 8)]
    [InlineData(16, 4)]
    [InlineData(4, 2)]
    [InlineData(1, 1)]
    public void APictureIsReadNoLargerThanTheCallerAsksFor(int enoughPixels, int side)
    {
        var raster = new Jpeg().Read(Written(0xC0, [0x43]), PictureLimits.For(enoughPixels));

        Assert.NotNull(raster);
        Assert.Equal(side, raster.Width);
        Assert.Equal(side, raster.Height);
        Assert.All(raster.Pixels, pixel => Assert.Equal(new(129, 129, 129), pixel));
    }

    /// <summary>
    /// The bits of a coefficient are the distance from the smallest value of its size, so the lower half
    /// of the range is negative: four bits of nought are fifteen below zero rather than zero.
    /// </summary>
    [Fact]
    public void TheLowerHalfOfACoefficientIsNegative()
    {
        var raster = Read(Written(0xC0, [0x03]));

        Assert.NotNull(raster);
        Assert.Equal(new(126, 126, 126), raster.Pixels[0]);
    }

    /// <summary>
    /// A progressive file says the same block several times over. Here the first scan carries the one
    /// coefficient the block has, one bit above where it will end up, and the second says the rest of
    /// them are nothing.
    /// </summary>
    [Fact]
    public void APictureWrittenInSeveralScansIsGathered()
    {
        var raster = Read(Progressive());

        Assert.NotNull(raster);
        Assert.Equal(8, raster.Width);
        Assert.All(raster.Pixels, pixel => Assert.Equal(new(130, 130, 130), pixel));
    }

    /// <summary>
    /// What is neither of the two ways a JPEG is read — lossless, hierarchical, or coded arithmetically
    /// — is refused rather than half drawn.
    /// </summary>
    /// <param name="frame">Which frame marker the file carries.</param>
    [Theory]
    [InlineData(0xC3)]
    [InlineData(0xC9)]
    [InlineData(0xCB)]
    public void AFrameThatIsNeitherIsRefused(int frame)
    {
        Assert.Null(Read(Written((byte)frame, [0x43])));
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF })]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 })]
    public void WhatIsNotAJpegComesBackAsNothing(byte[] bytes)
    {
        Assert.Null(Read(bytes));
    }

    [Fact]
    public void TheSignatureIsRecognisedOnItsOwn()
    {
        var jpeg = new Jpeg();

        Assert.True(jpeg.Starts([0xFF, 0xD8, 0xFF, 0xDB]));
        Assert.False(jpeg.Starts([0xFF, 0xD8]));
        Assert.False(jpeg.Starts("not a picture at all"u8));
    }

    private static Raster? Read(byte[] bytes) => new Jpeg().Read(bytes, PictureLimits.Default);

    /// <summary>
    /// Writes the smallest JPEG that says anything: one block of one component, divided by a table of
    /// ones, with a table that reads a four-bit coefficient and one that ends the block at once.
    /// </summary>
    /// <param name="frame">Which frame marker to write, since a progressive one is refused.</param>
    /// <param name="scan">The bits of the scan.</param>
    /// <returns>The file.</returns>
    private static byte[] Written(byte frame, byte[] scan)
    {
        var file = new MemoryStream();
        var divisors = new byte[65];

        for (var index = 1; index < divisors.Length; index++)
        {
            divisors[index] = 1;
        }

        file.Write([0xFF, 0xD8]);

        Segment(file, 0xDB, divisors);
        Segment(file, frame, [8, 0, 8, 0, 8, 1, 1, 0x11, 0]);
        Segment(file, 0xC4, [0x00, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4]);
        Segment(file, 0xC4, [0x10, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        Segment(file, 0xDA, [1, 1, 0x00, 0, 63, 0]);

        file.Write(scan);
        file.Write([0xFF, 0xD9]);

        return file.ToArray();
    }

    /// <summary>
    /// Writes the smallest progressive JPEG: one scan carrying the first coefficient a bit above where
    /// it belongs, and one saying that the rest of the block is nothing.
    /// </summary>
    /// <returns>The file.</returns>
    private static byte[] Progressive()
    {
        var file = new MemoryStream();
        var divisors = new byte[65];

        for (var index = 1; index < divisors.Length; index++)
        {
            divisors[index] = 1;
        }

        file.Write([0xFF, 0xD8]);

        Segment(file, 0xDB, divisors);
        Segment(file, 0xC2, [8, 0, 8, 0, 8, 1, 1, 0x11, 0]);
        Segment(file, 0xC4, [0x00, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4]);
        Segment(file, 0xDA, [1, 1, 0x00, 0, 0, 0x01]);

        file.Write([0x47]);

        Segment(file, 0xC4, [0x10, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        Segment(file, 0xDA, [1, 1, 0x00, 1, 63, 0x00]);

        file.Write([0x7F]);
        file.Write([0xFF, 0xD9]);

        return file.ToArray();
    }

    private static void Segment(Stream file, byte marker, byte[] body)
    {
        file.Write([0xFF, marker]);
        file.Write([(byte)((body.Length + 2) >> 8), (byte)(body.Length + 2)]);
        file.Write(body);
    }
}
