using System.IO;
using Arlecchino.Pictures;
using Arlecchino.Pictures.Formats.Qoi;
using Xunit;

namespace Arlecchino.Tests.Pictures;

public sealed class QoiTests
{
    /// <summary>
    /// The four ways a <c>QOI</c> writes a pixel that is not written out in full: the same one again, a small
    /// step from the last, a step named against the green, and one of the sixty-four seen recently.
    /// </summary>
    [Fact]
    public void EachKindOfChunkIsUnderstood()
    {
        var raster = Read(Written(4, 1, [0xFE, 10, 20, 30, 0xC0, 0x79, 0xA5, 0x96]));

        Assert.NotNull(raster);
        Assert.Equal(new(10, 20, 30), raster.Pixels[0]);
        Assert.Equal(new(10, 20, 30), raster.Pixels[1]);
        Assert.Equal(new(11, 20, 29), raster.Pixels[2]);
        Assert.Equal(new(17, 25, 32), raster.Pixels[3]);
    }

    /// <summary>
    /// A color is remembered in the slot its own values hash to, and named again by that slot alone.
    /// </summary>
    [Fact]
    public void AColourIsNamedAgainByTheSlotItHashedTo()
    {
        var raster = Read(Written(3, 1, [0xFE, 10, 20, 30, 0xFE, 200, 200, 200, 0x09]));

        Assert.NotNull(raster);
        Assert.Equal(new(200, 200, 200), raster.Pixels[1]);
        Assert.Equal(new(10, 20, 30), raster.Pixels[2]);
    }

    /// <summary>The alpha of a four-channel picture is read and then dropped, as everywhere else.</summary>
    [Fact]
    public void APixelWithAlphaKeepsItsColours()
    {
        var raster = Read(Written(1, 1, [0xFF, 1, 2, 3, 128]));

        Assert.NotNull(raster);
        Assert.Equal(new(1, 2, 3), raster.Pixels[0]);
    }

    [Fact]
    public void AFileThatStopsBeforeItsPixelsDoComesBackAsNothing()
    {
        Assert.Null(Read(Written(4, 4, [0xFE, 1, 2, 3])));
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new[] { (byte)'q', (byte)'o', (byte)'i', (byte)'f' })]
    [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 })]
    public void WhatIsNotAQoiComesBackAsNothing(byte[] bytes)
    {
        Assert.Null(Read(bytes));
    }

    private static Raster? Read(byte[] bytes) => new Qoi().Read(bytes, PictureLimits.Default);

    /// <summary>Writes a <c>QOI</c> by hand: the header, and then the chunks as they are given.</summary>
    /// <param name="width">How wide.</param>
    /// <param name="height">How tall.</param>
    /// <param name="chunks">The body of the file.</param>
    /// <returns>The file.</returns>
    private static byte[] Written(int width, int height, byte[] chunks)
    {
        var file = new MemoryStream();

        file.Write("qoif"u8);
        Big(file, width);
        Big(file, height);
        file.Write([4, 0]);
        file.Write(chunks);

        return file.ToArray();
    }

    private static void Big(Stream file, int value) =>
        file.Write([(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value]);
}
