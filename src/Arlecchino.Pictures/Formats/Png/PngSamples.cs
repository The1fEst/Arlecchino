using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Png;

/// <summary>
/// Takes pixels out of an unfiltered row. A sample is one, two, four, eight or sixteen bits wide, and
/// stands for a level of gray, a third of a color, or an entry to look up.
/// </summary>
internal static class PngSamples
{
    /// <summary>
    /// Reads a whole row of pixels, deciding what kind of picture it is once for the row. A sample of
    /// a whole byte is read without the bit arithmetic the narrower depths need.
    /// </summary>
    /// <param name="line">The row, with the filtering already undone.</param>
    /// <param name="header">What the picture is.</param>
    /// <param name="palette">The palette, when the color type wants one.</param>
    /// <param name="pixels">Where the row is read into.</param>
    /// <param name="offset">Where the row begins among them.</param>
    /// <param name="step">How far apart the pixels of this pass stand.</param>
    /// <param name="width">How many pixels the row holds.</param>
    /// <returns><c>false</c> when a palette entry is named that the palette does not hold.</returns>
    internal static bool Row(
        byte[] line,
        in PngHeader header,
        byte[]? palette,
        Rgb[] pixels,
        int offset,
        int step,
        int width)
    {
        var channels = header.Channels;

        if (header.Depth == 16)
        {
            for (var column = 0; column < width; column++)
            {
                var at = column * channels * 2;

                pixels[offset + (column * step)] = header.Color is 0 or 4
                    ? new(line[at], line[at], line[at])
                    : new(line[at], line[at + 2], line[at + 4]);
            }

            return true;
        }

        if (header.Depth != 8)
        {
            for (var column = 0; column < width; column++)
            {
                if (!Pixel(line, column, header, palette, out var pixel))
                {
                    return false;
                }

                pixels[offset + (column * step)] = pixel;
            }

            return true;
        }

        if (header.Color is 2 or 6)
        {
            for (var column = 0; column < width; column++)
            {
                var at = column * channels;

                pixels[offset + (column * step)] = new(line[at], line[at + 1], line[at + 2]);
            }

            return true;
        }

        if (header.Color is 0 or 4)
        {
            for (var column = 0; column < width; column++)
            {
                var gray = line[column * channels];

                pixels[offset + (column * step)] = new(gray, gray, gray);
            }

            return true;
        }

        for (var column = 0; column < width; column++)
        {
            var entry = line[column] * 3;

            if (palette is null || entry + 2 >= palette.Length)
            {
                return false;
            }

            pixels[offset + (column * step)] = new(palette[entry], palette[entry + 1], palette[entry + 2]);
        }

        return true;
    }

    /// <summary>Reads the pixel at a column.</summary>
    /// <param name="line">The row, with the filtering already undone.</param>
    /// <param name="column">Which pixel of the row.</param>
    /// <param name="header">What the picture is.</param>
    /// <param name="palette">The palette, when the color type wants one.</param>
    /// <param name="pixel">The color, once it is known.</param>
    /// <returns><c>false</c> when a palette entry is named that the palette does not hold.</returns>
    internal static bool Pixel(byte[] line, int column, in PngHeader header, byte[]? palette, out Rgb pixel)
    {
        var depth = header.Depth;
        var at = column * header.Channels;

        if (header.Color == 3)
        {
            var entry = Raw(line, at, depth) * 3;

            if (palette is null || entry + 2 >= palette.Length)
            {
                pixel = default;

                return false;
            }

            pixel = new(palette[entry], palette[entry + 1], palette[entry + 2]);

            return true;
        }

        if (header.Color is 0 or 4)
        {
            var gray = Scaled(Raw(line, at, depth), depth);

            pixel = new(gray, gray, gray);

            return true;
        }

        pixel = new(
            Scaled(Raw(line, at, depth), depth),
            Scaled(Raw(line, at + 1, depth), depth),
            Scaled(Raw(line, at + 2, depth), depth));

        return true;
    }

    /// <summary>The sample as it was written down, before it is stretched to a byte.</summary>
    /// <param name="line">The row.</param>
    /// <param name="index">Which sample of the row.</param>
    /// <param name="depth">Bits a sample.</param>
    /// <returns>The value; of a sixteen-bit sample, the half that is worth keeping.</returns>
    private static int Raw(byte[] line, int index, int depth)
    {
        if (depth == 8)
        {
            return line[index];
        }

        if (depth == 16)
        {
            return line[index * 2];
        }

        var bit = index * depth;

        return (line[bit / 8] >> (8 - depth - (bit % 8))) & ((1 << depth) - 1);
    }

    /// <summary>
    /// Stretches a sample to the whole of a byte, so that the deepest value of a one-bit picture is white
    /// rather than one.
    /// </summary>
    /// <param name="raw">The value as written down.</param>
    /// <param name="depth">Bits a sample.</param>
    /// <returns>The value over the full range.</returns>
    private static byte Scaled(int raw, int depth) => depth switch
    {
        1 => (byte)(raw * 255),
        2 => (byte)(raw * 85),
        4 => (byte)(raw * 17),
        _ => (byte)raw,
    };
}
