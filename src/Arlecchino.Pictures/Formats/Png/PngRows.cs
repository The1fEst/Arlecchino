using System;
using System.IO;
using System.IO.Compression;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Png;

/// <summary>
/// Unpacks the rows. Each one says how it was written down against the one above it, and an interlaced
/// picture says it seven times over, in passes that fill the grid a few pixels at a time.
/// </summary>
internal static class PngRows
{
    private static readonly int[] ColumnStart = [0, 4, 0, 2, 0, 1, 0];
    private static readonly int[] RowStart = [0, 0, 4, 0, 2, 0, 1];
    private static readonly int[] ColumnStep = [8, 8, 4, 4, 2, 2, 1];
    private static readonly int[] RowStep = [8, 8, 8, 4, 4, 2, 2];

    /// <summary>Reads every row of the picture.</summary>
    /// <param name="header">What the picture is.</param>
    /// <param name="stream">The joined <c>IDAT</c> chunks.</param>
    /// <param name="palette">The palette, when the color type wants one.</param>
    /// <returns>The pixels, or <c>null</c> when the rows ran out or named a color that is not there.</returns>
    internal static Raster? Read(PngHeader header, Stream stream, byte[]? palette)
    {
        using var inflate = new ZLibStream(stream, CompressionMode.Decompress);

        var pixels = new Rgb[header.Width * header.Height];

        if (!header.Interlaced)
        {
            return Pass(inflate, header, palette, pixels, 0, 0, 1, 1)
                ? new(pixels, header.Width, header.Height)
                : null;
        }

        for (var pass = 0; pass < 7; pass++)
        {
            if (!Pass(
                    inflate,
                    header,
                    palette,
                    pixels,
                    ColumnStart[pass],
                    RowStart[pass],
                    ColumnStep[pass],
                    RowStep[pass]))
            {
                return null;
            }
        }

        return new(pixels, header.Width, header.Height);
    }

    /// <summary>
    /// Reads one pass over the picture. Without interlacing there is a single pass, stepping one pixel
    /// at a time.
    /// </summary>
    /// <param name="inflate">The bytes, uncompressed.</param>
    /// <param name="header">What the picture is.</param>
    /// <param name="palette">The palette, when the color type wants one.</param>
    /// <param name="pixels">Where the pass writes what it reads.</param>
    /// <param name="columnStart">The first column of the grid this pass fills.</param>
    /// <param name="rowStart">The first row of the grid this pass fills.</param>
    /// <param name="columnStep">How far apart its columns stand.</param>
    /// <param name="rowStep">How far apart its rows stand.</param>
    /// <returns><c>false</c> when the bytes ran out before the pass was filled.</returns>
    private static bool Pass(
        Stream inflate,
        in PngHeader header,
        byte[]? palette,
        Rgb[] pixels,
        int columnStart,
        int rowStart,
        int columnStep,
        int rowStep)
    {
        var width = (header.Width - columnStart + columnStep - 1) / columnStep;
        var height = (header.Height - rowStart + rowStep - 1) / rowStep;

        if (width <= 0 || height <= 0)
        {
            return true;
        }

        var stride = ((width * header.Channels * header.Depth) + 7) / 8;
        var step = Math.Max(1, ((header.Channels * header.Depth) + 7) / 8);
        var line = new byte[stride];
        var previousRow = new byte[stride];

        for (var row = 0; row < height; row++)
        {
            var filter = inflate.ReadByte();

            if (filter < 0 || !Filled(inflate, line))
            {
                return false;
            }

            Unfilter(filter, line, previousRow, step);

            var offset = ((rowStart + (row * rowStep)) * header.Width) + columnStart;

            if (!PngSamples.Row(line, header, palette, pixels, offset, columnStep, width))
            {
                return false;
            }

            (previousRow, line) = (line, previousRow);
        }

        return true;
    }

    private static bool Filled(Stream inflate, byte[] line)
    {
        var at = 0;

        while (at < line.Length)
        {
            var count = inflate.Read(line, at, line.Length - at);

            if (count == 0)
            {
                return false;
            }

            at += count;
        }

        return true;
    }

    /// <summary>
    /// Undoes one row's filter. Which filter it is decided once for the row rather than once for every
    /// byte of it: a picture is millions of bytes, and a branch inside that loop is paid for every one.
    /// </summary>
    /// <param name="filter">Which filter the row was written with.</param>
    /// <param name="line">The row, undone in place.</param>
    /// <param name="previousRow">The row above, already undone.</param>
    /// <param name="step">How many bytes back the pixel to the left stands.</param>
    private static void Unfilter(int filter, byte[] line, byte[] previousRow, int step)
    {
        switch (filter)
        {
            case 1:
                for (var index = step; index < line.Length; index++)
                {
                    line[index] += line[index - step];
                }

                break;

            case 2:
                for (var index = 0; index < line.Length; index++)
                {
                    line[index] += previousRow[index];
                }

                break;

            case 3:
                for (var index = 0; index < step && index < line.Length; index++)
                {
                    line[index] += (byte)(previousRow[index] / 2);
                }

                for (var index = step; index < line.Length; index++)
                {
                    line[index] += (byte)((line[index - step] + previousRow[index]) / 2);
                }

                break;

            case 4:
                for (var index = 0; index < step && index < line.Length; index++)
                {
                    line[index] += previousRow[index];
                }

                for (var index = step; index < line.Length; index++)
                {
                    line[index] += Paeth(line[index - step], previousRow[index], previousRow[index - step]);
                }

                break;
        }
    }

    private static byte Paeth(int left, int upper, int corner)
    {
        var guess = left + upper - corner;
        var toLeft = Math.Abs(guess - left);
        var toUpper = Math.Abs(guess - upper);
        var toCorner = Math.Abs(guess - corner);

        return (byte)(toLeft <= toUpper && toLeft <= toCorner ? left : toUpper <= toCorner ? upper : corner);
    }
}
