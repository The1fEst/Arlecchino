using System;
using System.Buffers.Binary;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Tga;

/// <summary>
/// Turns the unpacked bytes of a <c>Targa</c> into pixels. The colors are written blue first, fifteen and
/// sixteen bits hold five for each of them, and the rows stand bottom to top unless the header says
/// otherwise.
/// </summary>
internal static class TgaPixels
{
    /// <summary>Reads the color map, which is written in the same shapes a pixel is.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where the map begins.</param>
    /// <param name="first">Which entry the map starts at, since the ones below it are not written down.</param>
    /// <param name="entries">How many entries it holds.</param>
    /// <param name="size">Bits an entry.</param>
    /// <returns>The colors, three bytes each, indexed the way a pixel indexes them.</returns>
    internal static byte[] Palette(ReadOnlySpan<byte> bytes, int at, int first, int entries, int size)
    {
        var colors = new byte[(first + entries) * 3];
        var step = (size + 7) / 8;

        for (var index = 0; index < entries; index++)
        {
            var from = at + (index * step);

            if (from + step > bytes.Length)
            {
                break;
            }

            var color = Color(bytes[from..], size);
            var into = (first + index) * 3;

            colors[into] = color.Red;
            colors[into + 1] = color.Green;
            colors[into + 2] = color.Blue;
        }

        return colors;
    }

    /// <summary>Reads the pixels.</summary>
    /// <param name="raw">The pixels as bytes, with any packing already undone.</param>
    /// <param name="width">How wide.</param>
    /// <param name="height">How tall.</param>
    /// <param name="depth">Bits a pixel.</param>
    /// <param name="kind">Which sort of picture the header said it is.</param>
    /// <param name="palette">The color map, when the pixels are entries in one.</param>
    /// <param name="topDown">Whether the first row written is the top one.</param>
    /// <returns>The pixels, or <c>null</c> when an entry is named that the map does not hold.</returns>
    internal static Raster? Read(
        byte[] raw,
        int width,
        int height,
        int depth,
        int kind,
        byte[]? palette,
        bool topDown)
    {
        var size = depth == 8 ? 1 : depth <= 16 ? 2 : depth / 8;
        var pixels = new Rgb[width * height];

        for (var row = 0; row < height; row++)
        {
            var from = row * width * size;
            var into = (topDown ? row : height - 1 - row) * width;

            for (var column = 0; column < width; column++)
            {
                var at = from + (column * size);

                if (kind is 1 or 9)
                {
                    var entry = raw[at] * 3;

                    if (palette is null || entry + 2 >= palette.Length)
                    {
                        return null;
                    }

                    pixels[into + column] = new(palette[entry], palette[entry + 1], palette[entry + 2]);

                    continue;
                }

                pixels[into + column] = Color(raw.AsSpan(at), depth);
            }
        }

        return new(pixels, width, height);
    }

    private static Rgb Color(ReadOnlySpan<byte> bytes, int depth)
    {
        if (depth == 8)
        {
            return new(bytes[0], bytes[0], bytes[0]);
        }

        if (depth > 16)
        {
            return new(bytes[2], bytes[1], bytes[0]);
        }

        var packed = BinaryPrimitives.ReadUInt16LittleEndian(bytes);

        return new(Five((packed >> 10) & 0x1F), Five((packed >> 5) & 0x1F), Five(packed & 0x1F));
    }

    private static byte Five(int value) => (byte)((value << 3) | (value >> 2));
}
