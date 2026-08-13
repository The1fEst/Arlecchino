using System;

namespace Arlecchino.Pictures.Formats.Pnm;

/// <summary>
/// Reads the <c>Netpbm</c> family: a bitmap, a gray picture or a color one, written either as numbers
/// or as the bytes themselves. The header is text, and a comment may stand between any two of its words.
/// </summary>
public sealed class Pnm : IPictureFormat
{
    /// <inheritdoc />
    public string Name => "pnm";

    /// <inheritdoc />
    public bool Starts(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 3 && bytes[0] == (byte)'P' && bytes[1] is >= (byte)'1' and <= (byte)'6' && Space(bytes[2]);

    /// <inheritdoc />
    public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        if (!Starts(bytes))
        {
            return null;
        }

        var kind = bytes[1] - '0';
        var at = 2;

        if (!Number(bytes, ref at, out var width) || !Number(bytes, ref at, out var height))
        {
            return null;
        }

        var levels = 1;

        if (kind is not (1 or 4) && !Number(bytes, ref at, out levels))
        {
            return null;
        }

        if (width <= 0 || height <= 0 || levels is <= 0 or > 65535 || (long)width * height > limits.Most)
        {
            return null;
        }

        return kind <= 3
            ? PnmBody.Written(bytes, at, kind, width, height, levels)
            : PnmBody.Raw(bytes, at + 1, kind, width, height, levels);
    }

    /// <summary>
    /// Reads the next number of the header, stepping over the space and the comments before it. A comment
    /// runs to the end of its line and may stand anywhere a space may.
    /// </summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where to read from; left on the character that ended it.</param>
    /// <param name="value">The value, once it is read.</param>
    /// <returns><c>false</c> when the file ends before the digits do.</returns>
    internal static bool Number(ReadOnlySpan<byte> bytes, ref int at, out int value)
    {
        value = 0;

        while (at < bytes.Length && (Space(bytes[at]) || bytes[at] == (byte)'#'))
        {
            if (bytes[at] == (byte)'#')
            {
                while (at < bytes.Length && bytes[at] is not ((byte)'\n' or (byte)'\r'))
                {
                    at++;
                }

                continue;
            }

            at++;
        }

        if (at >= bytes.Length || bytes[at] is < (byte)'0' or > (byte)'9')
        {
            return false;
        }

        while (at < bytes.Length && bytes[at] is >= (byte)'0' and <= (byte)'9')
        {
            value = (value * 10) + (bytes[at] - '0');

            if (value > 0xFFFFFF)
            {
                return false;
            }

            at++;
        }

        return true;
    }

    private static bool Space(byte value) => value is (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f';
}
