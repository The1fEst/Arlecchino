using System;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// Puts the planes together, a component sampled at half the width being read twice across. Where each
/// pixel reads from is worked out once a row and once a column rather than once a pixel.
/// </summary>
internal static class JpegColors
{
    private const int Fraction = 16;
    private const int Half = 1 << (Fraction - 1);
    private const int Red = (int)((1.402 * (1 << Fraction)) + 0.5);
    private const int GreenBlue = (int)((0.344136 * (1 << Fraction)) + 0.5);
    private const int GreenRed = (int)((0.714136 * (1 << Fraction)) + 0.5);
    private const int Blue = (int)((1.772 * (1 << Fraction)) + 0.5);

    /// <summary>Reads the picture out of the decoded planes.</summary>
    /// <param name="frame">The components, with their planes filled in.</param>
    /// <returns>The pixels.</returns>
    internal static Raster Read(JpegFrame frame)
    {
        var pixels = new Rgb[frame.Shown * frame.Deep];
        var gray = frame.Parts.Count == 1;
        var plain = frame.Transform == 0;
        var columns = Columns(frame);
        var rows = new int[frame.Parts.Count];
        var planes = new byte[frame.Parts.Count][];

        for (var index = 0; index < planes.Length; index++)
        {
            planes[index] = frame.Parts[index].Plane;
        }

        for (var row = 0; row < frame.Deep; row++)
        {
            var into = row * frame.Shown;

            for (var index = 0; index < planes.Length; index++)
            {
                var part = frame.Parts[index];

                rows[index] = row * part.Tall / frame.Tall * part.PlaneWidth;
            }

            for (var column = 0; column < frame.Shown; column++)
            {
                var first = At(planes[0], rows[0] + columns[0][column]);

                if (gray)
                {
                    pixels[into + column] = new(first, first, first);

                    continue;
                }

                var second = At(planes[1], rows[1] + columns[1][column]);
                var third = At(planes[2], rows[2] + columns[2][column]);

                pixels[into + column] = plain
                    ? new(first, second, third)
                    : Color(first, second, third);
            }
        }

        return new(pixels, frame.Shown, frame.Deep);
    }

    /// <summary>
    /// Which sample of each component every column of the picture reads from. A component sampled less
    /// often than the brightness is stretched by several columns reading the same sample.
    /// </summary>
    /// <param name="frame">The components.</param>
    /// <returns>One row of offsets for each component.</returns>
    private static int[][] Columns(JpegFrame frame)
    {
        var columns = new int[frame.Parts.Count][];

        for (var index = 0; index < columns.Length; index++)
        {
            var part = frame.Parts[index];
            var row = new int[frame.Shown];

            for (var column = 0; column < row.Length; column++)
            {
                row[column] = column * part.Wide / frame.Wide;
            }

            columns[index] = row;
        }

        return columns;
    }

    private static byte At(byte[] plane, int index) => index >= 0 && index < plane.Length ? plane[index] : (byte)0;

    /// <summary>
    /// Turns a brightness and two colors into the three a terminal draws. The coefficients are held as
    /// whole numbers over sixteen bits, which is exact enough for a byte.
    /// </summary>
    /// <param name="brightness">How light the pixel is.</param>
    /// <param name="blueness">How far towards blue it stands.</param>
    /// <param name="redness">How far towards red it stands.</param>
    /// <returns>The color.</returns>
    private static Rgb Color(byte brightness, byte blueness, byte redness)
    {
        var blue = blueness - 128;
        var red = redness - 128;

        return new(
            Clamped(brightness + (((Red * red) + Half) >> Fraction)),
            Clamped(brightness + ((-(GreenBlue * blue) - (GreenRed * red) + Half) >> Fraction)),
            Clamped(brightness + (((Blue * blue) + Half) >> Fraction)));
    }

    private static byte Clamped(int value) => (byte)Math.Clamp(value, 0, 255);
}
