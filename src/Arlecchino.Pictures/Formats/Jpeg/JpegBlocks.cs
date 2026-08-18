using System;
using System.Runtime.Intrinsics;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// Turns blocks of coefficients back into rows of samples, summing the waves they stand for. One of
/// these is kept for a whole picture, since it holds the two arrays the sums are worked in.
/// </summary>
internal sealed class JpegBlocks
{
    private readonly float[] _block = new float[64];
    private readonly float[] _partial = new float[64];

    private static readonly float[][] Waves = Built();

    /// <summary>Writes one block into the plane of a component.</summary>
    /// <param name="coefficients">The blocks, in the order they were read.</param>
    /// <param name="from">Where this block begins among them.</param>
    /// <param name="divisors">The table it was divided by.</param>
    /// <param name="plane">The samples of the component.</param>
    /// <param name="stride">How wide the plane is.</param>
    /// <param name="left">Which column of the plane the block begins at.</param>
    /// <param name="top">Which row of the plane the block begins at.</param>
    /// <param name="side">How many samples a side of the block comes out as: eight, four, two or one.</param>
    internal void Restore(
        int[] coefficients,
        int from,
        int[] divisors,
        byte[] plane,
        int stride,
        int left,
        int top,
        int side)
    {
        var block = _block;
        var partial = _partial;
        var waves = Waves[side];

        foreach (var index in JpegCoefficients.Places[side])
        {
            block[JpegCoefficients.Diagonal[index]] = coefficients[from + index] * divisors[index];
        }

        if (side == 8 && Vector128.IsHardwareAccelerated)
        {
            Whole(block, partial, waves, plane, stride, left, top);

            return;
        }

        for (var row = 0; row < side; row++)
        {
            if (Flat(block, row * 8, side))
            {
                var level = block[row * 8] * waves[0];

                for (var column = 0; column < side; column++)
                {
                    partial[(row * side) + column] = level;
                }

                continue;
            }

            for (var column = 0; column < side; column++)
            {
                var sum = 0f;

                for (var wave = 0; wave < side; wave++)
                {
                    sum += waves[(wave * side) + column] * block[(row * 8) + wave];
                }

                partial[(row * side) + column] = sum;
            }
        }

        for (var column = 0; column < side; column++)
        {
            for (var row = 0; row < side; row++)
            {
                var sum = 0f;

                for (var wave = 0; wave < side; wave++)
                {
                    sum += waves[(wave * side) + row] * partial[(wave * side) + column];
                }

                var offset = ((top + row) * stride) + left + column;

                if (offset >= 0 && offset < plane.Length)
                {
                    plane[offset] = (byte)Math.Clamp((int)MathF.Round(sum + 128f), 0, 255);
                }
            }
        }
    }

    /// <summary>
    /// The whole block, eight samples a side, summed four at a time. A row of the result is eight sums
    /// of the same shape, so four of them are added in one instruction.
    /// </summary>
    /// <param name="block">The coefficients, laid back out in rows.</param>
    /// <param name="partial">Where the first pass leaves its work.</param>
    /// <param name="waves">The waves, sampled at eight places.</param>
    /// <param name="plane">The samples of the component.</param>
    /// <param name="stride">How wide the plane is.</param>
    /// <param name="left">Which column of the plane the block begins at.</param>
    /// <param name="top">Which row of the plane the block begins at.</param>
    private static void Whole(
        float[] block,
        float[] partial,
        float[] waves,
        byte[] plane,
        int stride,
        int left,
        int top)
    {
        for (var row = 0; row < 8; row++)
        {
            var near = Vector128<float>.Zero;
            var far = Vector128<float>.Zero;

            for (var wave = 0; wave < 8; wave++)
            {
                var value = block[(row * 8) + wave];

                if (value == 0f)
                {
                    continue;
                }

                var scale = Vector128.Create(value);

                near += Vector128.LoadUnsafe(ref waves[wave * 8]) * scale;
                far += Vector128.LoadUnsafe(ref waves[(wave * 8) + 4]) * scale;
            }

            near.StoreUnsafe(ref partial[row * 8]);
            far.StoreUnsafe(ref partial[(row * 8) + 4]);
        }

        for (var row = 0; row < 8; row++)
        {
            var near = Vector128<float>.Zero;
            var far = Vector128<float>.Zero;

            for (var wave = 0; wave < 8; wave++)
            {
                var scale = Vector128.Create(waves[(wave * 8) + row]);

                near += Vector128.LoadUnsafe(ref partial[wave * 8]) * scale;
                far += Vector128.LoadUnsafe(ref partial[(wave * 8) + 4]) * scale;
            }

            var offset = ((top + row) * stride) + left;

            if (offset < 0 || offset + 8 > plane.Length)
            {
                continue;
            }

            for (var column = 0; column < 8; column++)
            {
                var sum = column < 4 ? near[column] : far[column - 4];

                plane[offset + column] = (byte)Math.Clamp((int)MathF.Round(sum + 128f), 0, 255);
            }
        }
    }

    /// <summary>
    /// Whether a row of the block holds nothing but its first coefficient, as most rows do. Such a row
    /// is one value across.
    /// </summary>
    /// <param name="block">The block, laid out in rows.</param>
    /// <param name="from">Where the row begins.</param>
    /// <param name="side">How many of its coefficients are being read.</param>
    /// <returns><c>true</c> when everything past the first coefficient is nought.</returns>
    private static bool Flat(float[] block, int from, int side)
    {
        for (var index = 1; index < side; index++)
        {
            if (block[from + index] != 0f)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The wave each coefficient stands for, sampled at as many places as the block comes out wide, for
    /// each of the four sizes. The half at the front is what the two passes together are divided by.
    /// </summary>
    /// <returns>The tables, by how many samples a side comes out as.</returns>
    private static float[][] Built()
    {
        var tables = new float[9][];

        foreach (var side in new[] { 1, 2, 4, 8 })
        {
            var waves = new float[side * side];

            for (var wave = 0; wave < side; wave++)
            {
                var scale = wave == 0 ? MathF.Sqrt(0.5f) : 1f;

                for (var at = 0; at < side; at++)
                {
                    waves[(wave * side) + at] =
                        0.5f * scale * MathF.Cos((((2 * at) + 1) * wave * MathF.PI) / (2f * side));
                }
            }

            tables[side] = waves;
        }

        return tables;
    }
}
