using System.Collections.Generic;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// How the coefficients of a block are written down: in a diagonal order that puts the coarse ones
/// first, and with the bits of a value standing for its distance from the smallest value of its size.
/// </summary>
internal static class JpegCoefficients
{
    /// <summary>Where each coefficient of a block belongs once it is laid back out in rows.</summary>
    internal static readonly int[] Diagonal =
    [
        0, 1, 8, 16, 9, 2, 3, 10,
        17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34,
        27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36,
        29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46,
        53, 60, 61, 54, 47, 55, 62, 63,
    ];

    /// <summary>
    /// Which coefficients matter at each of the four sizes a block is read at, in the order they are
    /// read. The rest are neither cleared nor laid back out.
    /// </summary>
    internal static readonly int[][] Wanted = Needed();

    /// <summary>
    /// Reads a coefficient the way a JPEG writes one: the bits are the distance from the smallest value
    /// of its size, so the lower half of the range is negative.
    /// </summary>
    /// <param name="value">The bits as they were read.</param>
    /// <param name="size">How many of them there were.</param>
    /// <returns>The coefficient.</returns>
    internal static int Signed(int value, int size) =>
        value < 1 << (size - 1) ? value - (1 << size) + 1 : value;

    /// <summary>Works out which coefficients each size needs.</summary>
    /// <returns>The lists, by how many samples a side of the block comes out as.</returns>
    private static int[][] Needed()
    {
        var wanted = new int[9][];

        foreach (var side in new[] { 1, 2, 4, 8 })
        {
            var kept = new List<int>(side * side);

            for (var index = 0; index < Diagonal.Length; index++)
            {
                if (Diagonal[index] % 8 < side && Diagonal[index] / 8 < side)
                {
                    kept.Add(index);
                }
            }

            wanted[side] = [.. kept];
        }

        return wanted;
    }
}
