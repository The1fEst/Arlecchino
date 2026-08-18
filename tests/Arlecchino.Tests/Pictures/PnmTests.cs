using System.Collections.Generic;
using System.IO;
using System.Text;
using Arlecchino.Pictures;
using Arlecchino.Pictures.Formats.Pnm;
using Xunit;

namespace Arlecchino.Tests.Pictures;

public sealed class PnmTests
{
    [Fact]
    public void ColoursAreReadAsBytesOrAsNumbers()
    {
        var raw = Read(Written("P6\n2 1\n255\n", [255, 0, 0, 0, 255, 0]));
        var output = Read(Written("P3\n2 1\n255\n255 0 0 0 255 0\n"));

        Assert.NotNull(raw);
        Assert.NotNull(output);
        Assert.Equal(new(255, 0, 0), raw.Pixels[0]);
        Assert.Equal(new(0, 255, 0), raw.Pixels[1]);
        Assert.Equal(raw.Pixels, output.Pixels);
    }

    [Fact]
    public void GreyIsTheSameValueInAllThree()
    {
        var raster = Read(Written("P5\n2 1\n255\n", [10, 200]));

        Assert.NotNull(raster);
        Assert.Equal(new(10, 10, 10), raster.Pixels[0]);
        Assert.Equal(new(200, 200, 200), raster.Pixels[1]);
    }

    /// <summary>A bitmap is the odd one of the family: a set bit is black rather than white.</summary>
    [Fact]
    public void ASetBitIsBlack()
    {
        var body = Read(Written("P4\n2 1\n", [0b0100_0000]));
        var output = Read(Written("P1\n2 1\n0 1\n"));

        Assert.NotNull(body);
        Assert.NotNull(output);
        Assert.Equal(new(255, 255, 255), body.Pixels[0]);
        Assert.Equal(new(0, 0, 0), body.Pixels[1]);
        Assert.Equal(body.Pixels, output.Pixels);
    }

    /// <summary>A comment runs to the end of its line and may stand anywhere a space may.</summary>
    [Fact]
    public void CommentsInTheHeaderAreSteppedOver()
    {
        var raster = Read(Written("P5\n# what this is\n1 # and how wide\n1\n255\n", [42]));

        Assert.NotNull(raster);
        Assert.Equal(new(42, 42, 42), raster.Pixels[0]);
    }

    /// <summary>Above a depth of 255 a sample takes two bytes, and is brought back down to one.</summary>
    [Fact]
    public void DeepSamplesAreScaledDownToBytes()
    {
        var raster = Read(Written("P5\n1 1\n65535\n", [0x80, 0x00]));

        Assert.NotNull(raster);
        Assert.Equal(128, raster.Pixels[0].Red);
    }

    [Fact]
    public void AFileThatStopsBeforeItsPixelsDoComesBackAsNothing()
    {
        Assert.Null(Read(Written("P6\n4 4\n255\n", [1, 2, 3])));
    }

    [Theory]
    [InlineData("P7\n1 1\n255\n")]
    [InlineData("P6")]
    [InlineData("not a picture at all")]
    public void WhatIsNotANetpbmComesBackAsNothing(string text)
    {
        Assert.Null(Read(Written(text)));
    }

    private static Raster? Read(byte[] bytes) => new Pnm().Read(bytes, PictureLimits.Default);

    private static byte[] Written(string header, IEnumerable<byte>? body = null)
    {
        var file = new MemoryStream();

        file.Write(Encoding.ASCII.GetBytes(header));

        if (body is null)
        {
            return file.ToArray();
        }

        foreach (var value in body)
        {
            file.WriteByte(value);
        }

        return file.ToArray();
    }
}
