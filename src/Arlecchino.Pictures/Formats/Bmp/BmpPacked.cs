using System;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Bmp;

/// <summary>
/// The run-length encoded rows of a palette bitmap. A pair of bytes is a run of one entry, or an
/// escape: the end of a row, the end of the picture, a jump, or a count of entries.
/// </summary>
internal static class BmpPacked
{
    /// <summary>Reads the packed rows.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="header">What the header said.</param>
    /// <param name="palette">The colors to look up.</param>
    /// <param name="offset">Where the packets begin.</param>
    /// <returns>The pixels, or <c>null</c> when the depth and the packing disagree, or the packets run out.</returns>
    internal static Raster? Read(ReadOnlySpan<byte> bytes, in BmpInfo header, byte[]? palette, int offset)
    {
        var four = header.Compression == 2;

        if (palette is null || header.Bits != (four ? 4 : 8))
        {
            return null;
        }

        var pixels = new Rgb[header.Width * header.Height];
        var at = offset;
        var column = 0;
        var row = 0;

        while (at + 1 < bytes.Length)
        {
            var count = bytes[at++];
            var value = bytes[at++];

            if (count > 0)
            {
                for (var step = 0; step < count; step++, column++)
                {
                    Put(pixels, header, palette, row, column, four ? Nibble(value, step) : value);
                }

                continue;
            }

            if (value == 1)
            {
                break;
            }

            if (value == 0)
            {
                column = 0;
                row++;

                continue;
            }

            if (value == 2)
            {
                if (at + 1 >= bytes.Length)
                {
                    return null;
                }

                column += bytes[at++];
                row += bytes[at++];

                continue;
            }

            var length = four ? (value + 1) / 2 : value;

            if (at + length > bytes.Length)
            {
                return null;
            }

            for (var step = 0; step < value; step++, column++)
            {
                var entry = four ? Nibble(bytes[at + (step / 2)], step) : bytes[at + step];

                Put(pixels, header, palette, row, column, entry);
            }

            at += length + (length & 1);
        }

        return new(pixels, header.Width, header.Height);
    }

    private static void Put(Rgb[] pixels, in BmpInfo header, byte[] palette, int row, int column, int entry)
    {
        if (column < 0 || column >= header.Width || row < 0 || row >= header.Height)
        {
            return;
        }

        var at = entry * 3;

        if (at + 2 >= palette.Length)
        {
            return;
        }

        var into = ((header.TopDown ? row : header.Height - 1 - row) * header.Width) + column;

        pixels[into] = new(palette[at], palette[at + 1], palette[at + 2]);
    }

    private static int Nibble(byte packed, int step) => (step & 1) == 0 ? packed >> 4 : packed & 0x0F;
}
