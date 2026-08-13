using System.IO;
using Arlecchino.Pictures;
using Arlecchino.Pictures.Formats.Tga;
using Xunit;

namespace Arlecchino.Tests.Pictures;

public sealed class TgaTests
{
    /// <summary>The rows stand bottom to top, and each color is written blue first.</summary>
    [Fact]
    public void TheLastRowWrittenIsTheTopOfThePicture()
    {
        var raster = Read(Written(2, 1, 2, 24, [1, 2, 3, 4, 5, 6]));

        Assert.NotNull(raster);
        Assert.Equal(new(6, 5, 4), raster.Pixels[0]);
        Assert.Equal(new(3, 2, 1), raster.Pixels[1]);
    }

    /// <summary>One bit of the last header byte asks for the first row to be the top one.</summary>
    [Fact]
    public void ABitOfTheDescriptorTurnsThePictureTheRightWayUp()
    {
        var raster = Read(Written(2, 1, 2, 24, [1, 2, 3, 4, 5, 6], descriptor: 0x20));

        Assert.NotNull(raster);
        Assert.Equal(new(3, 2, 1), raster.Pixels[0]);
        Assert.Equal(new(6, 5, 4), raster.Pixels[1]);
    }

    /// <summary>
    /// A packet is either one pixel to repeat or a count of pixels that follow, and the high bit says
    /// which.
    /// </summary>
    [Fact]
    public void RunLengthEncodedPixelsAreUnpacked()
    {
        var raster = Read(Written(10, 4, 1, 24, [0x81, 7, 8, 9, 0x01, 1, 2, 3, 4, 5, 6]));

        Assert.NotNull(raster);
        Assert.Equal(new(9, 8, 7), raster.Pixels[0]);
        Assert.Equal(new(9, 8, 7), raster.Pixels[1]);
        Assert.Equal(new(3, 2, 1), raster.Pixels[2]);
        Assert.Equal(new(6, 5, 4), raster.Pixels[3]);
    }

    /// <summary>Fifteen and sixteen bits hold five for each color, packed into two bytes.</summary>
    [Fact]
    public void FiveBitsOfColourAreStretchedToWholeBytes()
    {
        var raster = Read(Written(2, 1, 1, 16, [0x1F, 0x7C]));

        Assert.NotNull(raster);
        Assert.Equal(new(255, 0, 255), raster.Pixels[0]);
    }

    [Fact]
    public void APaletteIsLookedUpRatherThanRead()
    {
        var raster = Read(Written(1, 2, 1, 8, [1, 0], palette: [0, 0, 0, 9, 8, 7]));

        Assert.NotNull(raster);
        Assert.Equal(new(7, 8, 9), raster.Pixels[0]);
        Assert.Equal(new(0, 0, 0), raster.Pixels[1]);
    }

    /// <summary>
    /// The format begins with no signature, so the header is claimed only when every field of it is one
    /// of those the format allows.
    /// </summary>
    /// <param name="bytes">The head of a file.</param>
    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 24, 0 })]
    [InlineData(new byte[] { 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 24, 0 })]
    [InlineData(new byte[] { 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 7, 0 })]
    public void AHeaderThatIsNotATargasIsNotClaimed(byte[] bytes)
    {
        Assert.False(new Tga().Starts(bytes));
    }

    [Fact]
    public void AFileThatStopsBeforeItsPixelsDoComesBackAsNothing()
    {
        Assert.Null(Read(Written(2, 4, 4, 24, [1, 2, 3])));
    }

    private static Raster? Read(byte[] bytes) => new Tga().Read(bytes, PictureLimits.Default);

    /// <summary>Writes a <c>Targa</c> by hand.</summary>
    /// <param name="kind">Which sort of picture: mapped, true color, gray, or the packed ones.</param>
    /// <param name="width">How wide.</param>
    /// <param name="height">How tall.</param>
    /// <param name="depth">Bits a pixel.</param>
    /// <param name="body">The pixels, packed or not as the kind says.</param>
    /// <param name="palette">The color map, three bytes an entry, blue first.</param>
    /// <param name="descriptor">The last byte of the header.</param>
    /// <returns>The file.</returns>
    private static byte[] Written(
        int kind,
        int width,
        int height,
        int depth,
        byte[] body,
        byte[]? palette = null,
        byte descriptor = 0)
    {
        var file = new MemoryStream();
        var entries = palette is null ? 0 : palette.Length / 3;

        file.WriteByte(0);
        file.WriteByte((byte)(palette is null ? 0 : 1));
        file.WriteByte((byte)kind);
        Little(file, 0);
        Little(file, entries);
        file.WriteByte((byte)(palette is null ? 0 : 24));
        Little(file, 0);
        Little(file, 0);
        Little(file, width);
        Little(file, height);
        file.WriteByte((byte)depth);
        file.WriteByte(descriptor);

        if (palette is not null)
        {
            file.Write(palette);
        }

        file.Write(body);

        return file.ToArray();
    }

    private static void Little(Stream file, int value) => file.Write([(byte)value, (byte)(value >> 8)]);
}
