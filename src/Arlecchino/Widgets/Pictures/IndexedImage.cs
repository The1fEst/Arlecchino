using System;
using System.Collections.Generic;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Widgets.Pictures;

/// <summary>
/// A picture resampled to a given size and brought down to a palette of its own, which is what a format with
/// color registers needs. Shrinking averages the source pixels a destination pixel covers.
/// </summary>
internal sealed class IndexedImage
{
    private const int Buckets = 32 * 32 * 32;

    private IndexedImage(Rgb[] palette, byte[] indexes, int width, int height)
    {
        Palette = palette;
        Indexes = indexes;
        Width = width;
        Height = height;
    }

    public Rgb[] Palette { get; }

    public int Width { get; }

    public int Height { get; }

    private byte[] Indexes { get; }

    public byte At(int column, int row) => Indexes[(row * Width) + column];

    /// <summary>
    /// Brings a picture down to a palette. The colors are counted in five bits a channel rather than
    /// eight, since everything after the counting is paid for once per distinct color.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="across">How wide the result should be.</param>
    /// <param name="down">How tall the result should be.</param>
    /// <param name="colors">How many colors the palette may hold.</param>
    /// <returns>The picture as entries in a palette of its own.</returns>
    public static IndexedImage From(Rgb[] pixels, int width, int height, int across, int down, int colors)
    {
        var (scaled, wide, tall) = PictureScale.To(pixels, width, height, across, down);
        var counted = new int[Buckets];
        var reds = new int[Buckets];
        var greens = new int[Buckets];
        var blues = new int[Buckets];
        var distinct = 0;

        foreach (var pixel in scaled)
        {
            var bucket = Bucket(pixel);

            if (counted[bucket] == 0)
            {
                distinct++;
            }

            counted[bucket]++;
            reds[bucket] += pixel.Red;
            greens[bucket] += pixel.Green;
            blues[bucket] += pixel.Blue;
        }

        var keys = new int[distinct];
        var counts = new int[distinct];
        var at = 0;

        for (var bucket = 0; bucket < counted.Length; bucket++)
        {
            if (counted[bucket] == 0)
            {
                continue;
            }

            keys[at] = ((reds[bucket] / counted[bucket]) << 16) | ((greens[bucket] / counted[bucket]) << 8) | (blues[bucket] / counted[bucket]);

            counts[at] = counted[bucket];
            at++;
        }

        var palette = MedianCut(keys, counts, colors);
        var nearest = new byte[Buckets];
        var known = new bool[Buckets];
        var indexes = new byte[scaled.Length];

        for (var index = 0; index < scaled.Length; index++)
        {
            var pixel = scaled[index];
            var bucket = Bucket(pixel);

            if (!known[bucket])
            {
                nearest[bucket] = Nearest(palette, Key(pixel));
                known[bucket] = true;
            }

            indexes[index] = nearest[bucket];
        }

        return new(palette, indexes, wide, tall);
    }

    private static Rgb[] MedianCut(int[] keys, int[] counts, int colors)
    {
        var boxes = new List<(int Start, int Length)> { (0, keys.Length) };

        while (boxes.Count < colors)
        {
            var pick = -1;
            var widest = 0;
            var shift = 0;

            for (var index = 0; index < boxes.Count; index++)
            {
                if (boxes[index].Length < 2)
                {
                    continue;
                }

                var (span, along) = Longest(keys, boxes[index]);

                if (span <= widest)
                {
                    continue;
                }

                widest = span;
                shift = along;
                pick = index;
            }

            if (pick < 0)
            {
                break;
            }

            var box = boxes[pick];

            Array.Sort(keys, counts, box.Start, box.Length, Channel.At(shift));

            var half = Half(counts, box);

            boxes[pick] = (box.Start, half - box.Start);
            boxes.Insert(pick + 1, (half, box.Start + box.Length - half));
        }

        var palette = new Rgb[boxes.Count];

        for (var index = 0; index < boxes.Count; index++)
        {
            palette[index] = Average(keys, counts, boxes[index]);
        }

        return palette;
    }

    private static (int Span, int Shift) Longest(int[] keys, (int Start, int Length) box)
    {
        var span = 0;
        var shift = 0;

        for (var channel = 0; channel < 3; channel++)
        {
            var low = 255;
            var high = 0;

            for (var index = box.Start; index < box.Start + box.Length; index++)
            {
                var value = (keys[index] >> (channel * 8)) & 0xFF;

                low = Math.Min(low, value);
                high = Math.Max(high, value);
            }

            if (high - low <= span)
            {
                continue;
            }

            span = high - low;
            shift = channel * 8;
        }

        return (span, shift);
    }

    private static int Half(int[] counts, (int Start, int Length) box)
    {
        var total = 0L;

        for (var index = box.Start; index < box.Start + box.Length; index++)
        {
            total += counts[index];
        }

        var seen = 0L;

        for (var index = box.Start; index < box.Start + box.Length - 1; index++)
        {
            seen += counts[index];

            if (seen * 2 >= total)
            {
                return index + 1;
            }
        }

        return box.Start + box.Length - 1;
    }

    private static Rgb Average(int[] keys, int[] counts, (int Start, int Length) box)
    {
        var red = 0L;
        var green = 0L;
        var blue = 0L;
        var total = 0L;

        for (var index = box.Start; index < box.Start + box.Length; index++)
        {
            var weight = counts[index];

            red += ((keys[index] >> 16) & 0xFF) * (long)weight;
            green += ((keys[index] >> 8) & 0xFF) * (long)weight;
            blue += (keys[index] & 0xFF) * (long)weight;
            total += weight;
        }

        if (total == 0)
        {
            return default;
        }

        return new((byte)(red / total), (byte)(green / total), (byte)(blue / total));
    }

    private static byte Nearest(Rgb[] palette, int key)
    {
        var red = (key >> 16) & 0xFF;
        var green = (key >> 8) & 0xFF;
        var blue = key & 0xFF;
        var best = 0;
        var closest = int.MaxValue;

        for (var index = 0; index < palette.Length; index++)
        {
            var color = palette[index];
            var apart = Square(red - color.Red) + Square(green - color.Green) + Square(blue - color.Blue);

            if (apart >= closest)
            {
                continue;
            }

            closest = apart;
            best = index;

            if (apart == 0)
            {
                break;
            }
        }

        return (byte)best;
    }

    private static int Square(int value) => value * value;

    /// <summary>Which of the buckets a color falls into, at five bits for each of the three.</summary>
    /// <param name="pixel">The color.</param>
    /// <returns>The bucket.</returns>
    private static int Bucket(Rgb pixel) =>
        ((pixel.Red >> 3) << 10) | ((pixel.Green >> 3) << 5) | (pixel.Blue >> 3);

    private static int Key(Rgb pixel) => (pixel.Red << 16) | (pixel.Green << 8) | pixel.Blue;

    private sealed class Channel(int shift) : IComparer<int>
    {
        private static readonly Channel[] Channels = [new(0), new(8), new(16)];

        public static Channel At(int shift) => Channels[shift / 8];

        public int Compare(int x, int y) => ((x >> shift) & 0xFF).CompareTo((y >> shift) & 0xFF);
    }
}
