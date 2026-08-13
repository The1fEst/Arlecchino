using System;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Pnm;

/// <summary>
/// The body of a <c>Netpbm</c> picture, which comes in two shapes: the numbers written one after another,
/// and the samples as bytes. A bitmap is the odd one of the family, where a set bit means black.
/// </summary>
internal static class PnmBody
{
    /// <summary>Reads a body written as numbers, which is P1, P2 or P3.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where the body begins.</param>
    /// <param name="kind">Which of the six it is.</param>
    /// <param name="width">How wide.</param>
    /// <param name="height">How tall.</param>
    /// <param name="levels">The deepest value a sample takes.</param>
    /// <returns>The pixels, or <c>null</c> when the numbers run out.</returns>
    internal static Raster? Written(ReadOnlySpan<byte> bytes, int at, int kind, int width, int height, int levels)
    {
        var pixels = new Rgb[width * height];

        for (var index = 0; index < pixels.Length; index++)
        {
            if (kind == 1)
            {
                if (!Pnm.Number(bytes, ref at, out var bit))
                {
                    return null;
                }

                pixels[index] = Gray(bit == 0 ? (byte)255 : (byte)0);

                continue;
            }

            if (kind == 2)
            {
                if (!Pnm.Number(bytes, ref at, out var gray))
                {
                    return null;
                }

                pixels[index] = Gray(Scaled(gray, levels));

                continue;
            }

            if (!Pnm.Number(bytes, ref at, out var red) ||
                !Pnm.Number(bytes, ref at, out var green) ||
                !Pnm.Number(bytes, ref at, out var blue))
            {
                return null;
            }

            pixels[index] = new(Scaled(red, levels), Scaled(green, levels), Scaled(blue, levels));
        }

        return new(pixels, width, height);
    }

    /// <summary>Reads a body written as bytes, which is P4, P5 or P6.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where the body begins.</param>
    /// <param name="kind">Which of the six it is.</param>
    /// <param name="width">How wide.</param>
    /// <param name="height">How tall.</param>
    /// <param name="levels">The deepest value a sample takes.</param>
    /// <returns>The pixels, or <c>null</c> when the file is shorter than it says it is.</returns>
    internal static Raster? Raw(ReadOnlySpan<byte> bytes, int at, int kind, int width, int height, int levels)
    {
        var pixels = new Rgb[width * height];

        if (kind == 4)
        {
            return Bits(bytes, at, width, height, pixels);
        }

        var channels = kind == 5 ? 1 : 3;
        var wide = levels > 255;
        var size = channels * (wide ? 2 : 1);

        if (at + ((long)pixels.Length * size) > bytes.Length)
        {
            return null;
        }

        for (var index = 0; index < pixels.Length; index++)
        {
            var sample = at + (index * size);

            pixels[index] = kind == 5
                ? Gray(Scaled(Value(bytes, sample, wide), levels))
                : new(
                    Scaled(Value(bytes, sample, wide), levels),
                    Scaled(Value(bytes, sample + (wide ? 2 : 1), wide), levels),
                    Scaled(Value(bytes, sample + (wide ? 4 : 2), wide), levels));
        }

        return new(pixels, width, height);
    }

    private static Raster? Bits(ReadOnlySpan<byte> bytes, int at, int width, int height, Rgb[] pixels)
    {
        var stride = (width + 7) / 8;

        if (at + ((long)stride * height) > bytes.Length)
        {
            return null;
        }

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var bit = (bytes[at + (row * stride) + (column / 8)] >> (7 - (column % 8))) & 1;

                pixels[(row * width) + column] = Gray(bit == 0 ? (byte)255 : (byte)0);
            }
        }

        return new(pixels, width, height);
    }

    private static int Value(ReadOnlySpan<byte> bytes, int at, bool wide) => wide ? (bytes[at] << 8) | bytes[at + 1] : bytes[at];

    private static byte Scaled(int value, int levels) =>
        (byte)Math.Min((((value * 255 * 2) + levels) / (levels * 2)), 255);

    private static Rgb Gray(byte value) => new(value, value, value);
}
