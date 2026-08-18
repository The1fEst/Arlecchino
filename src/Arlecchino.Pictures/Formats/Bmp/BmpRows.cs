using System;
using System.Buffers.Binary;
using System.Numerics;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Bmp;

/// <summary>
/// The rows of a bitmap. Each one is padded out to four bytes, and the last one written is the top of
/// the picture unless the header asked for the other way round.
/// </summary>
internal static class BmpRows
{
    /// <summary>Reads every row.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="header">What the header said.</param>
    /// <param name="palette">The colors to look up, when the depth looks anything up.</param>
    /// <param name="offset">Where the rows begin.</param>
    /// <returns>The pixels, or <c>null</c> when the depth is not one that is read or the rows are short.</returns>
    internal static Raster? Read(ReadOnlySpan<byte> bytes, in BmpInfo header, byte[]? palette, int offset)
    {
        if (header.Bits is not (1 or 4 or 8 or 16 or 24 or 32))
        {
            return null;
        }

        var stride = ((header.Width * header.Bits) + 31) / 32 * 4;

        if (offset + ((long)stride * header.Height) > bytes.Length)
        {
            return null;
        }

        var red = header.Masks ? header.RedMask : Default(header.Bits, 0);
        var green = header.Masks ? header.GreenMask : Default(header.Bits, 1);
        var blue = header.Masks ? header.BlueMask : Default(header.Bits, 2);
        var pixels = new Rgb[header.Width * header.Height];

        for (var row = 0; row < header.Height; row++)
        {
            var from = offset + (row * stride);
            var target = (header.TopDown ? row : header.Height - 1 - row) * header.Width;

            for (var column = 0; column < header.Width; column++)
            {
                if (!Pixel(bytes, from, column, header, palette, red, green, blue, out var pixel))
                {
                    return null;
                }

                pixels[target + column] = pixel;
            }
        }

        return new(pixels, header.Width, header.Height);
    }

    private static bool Pixel(
        ReadOnlySpan<byte> bytes,
        int from,
        int column,
        in BmpInfo header,
        byte[]? palette,
        uint red,
        uint green,
        uint blue,
        out Rgb pixel)
    {
        pixel = default;

        if (header.Bits <= 8)
        {
            var bit = column * header.Bits;
            var entry = ((bytes[from + (bit / 8)] >> (8 - header.Bits - (bit % 8))) & ((1 << header.Bits) - 1)) * 3;

            if (palette is null || entry + 2 >= palette.Length)
            {
                return false;
            }

            pixel = new(palette[entry], palette[entry + 1], palette[entry + 2]);

            return true;
        }

        if (header.Bits == 24)
        {
            var at = from + (column * 3);

            pixel = new(bytes[at + 2], bytes[at + 1], bytes[at]);

            return true;
        }

        var value = header.Bits == 16
            ? BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(from + (column * 2), 2))
            : BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(from + (column * 4), 4));

        pixel = new(Channel(value, red), Channel(value, green), Channel(value, blue));

        return true;
    }

    /// <summary>
    /// Which bits a color takes when the header does not say. Sixteen bits are five each with one left
    /// over, and thirty-two are a byte each with a byte left over.
    /// </summary>
    /// <param name="bits">Bits a pixel.</param>
    /// <param name="channel">Nought for red, one for green, two for blue.</param>
    /// <returns>The mask.</returns>
    private static uint Default(int bits, int channel) => bits switch
    {
        16 => channel switch
        {
            0 => 0x7C00,
            1 => 0x03E0,
            _ => 0x001F,
        },
        _ => channel switch
        {
            0 => 0x00FF0000,
            1 => 0x0000FF00,
            _ => 0x000000FF,
        },
    };

    private static byte Channel(uint value, uint mask)
    {
        if (mask == 0)
        {
            return 0;
        }

        var bits = (value & mask) >> BitOperations.TrailingZeroCount(mask);
        var deepest = (1u << BitOperations.PopCount(mask)) - 1;

        return (byte)(bits * 255 / deepest);
    }
}
