using System;
using System.Collections.Generic;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// A picture resampled to a given size and brought down to a palette of at most so many colours —
/// what a format with colour registers, <see cref="ImageProtocol.Sixel"/> among them, needs before it
/// can be written out.
///
/// The palette is the picture's own rather than a fixed cube: colours are gathered into boxes and the
/// widest box is split at its weighted median until there are as many boxes as colours allowed, which
/// spends the registers where the picture actually has detail. Shrinking averages every source pixel a
/// destination pixel covers instead of picking one of them, so a picture reduced to a pane keeps its
/// edges.
/// </summary>
internal sealed class IndexedImage
{
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

    public static IndexedImage From(Rgb[] pixels, int width, int height, int across, int down, int colors)
    {
        var scaled = Resample(pixels, width, height, across, down);
        var counted = new Dictionary<int, int>();

        foreach (var pixel in scaled)
        {
            var key = Key(pixel);

            counted[key] = counted.TryGetValue(key, out var seen) ? seen + 1 : 1;
        }

        var keys = new int[counted.Count];
        var counts = new int[counted.Count];
        var at = 0;

        foreach (var (key, count) in counted)
        {
            keys[at] = key;
            counts[at] = count;
            at++;
        }

        var palette = MedianCut(keys, counts, colors);
        var nearest = new Dictionary<int, byte>(keys.Length);

        foreach (var key in keys)
        {
            nearest[key] = Nearest(palette, key);
        }

        var indexes = new byte[scaled.Length];

        for (var index = 0; index < scaled.Length; index++)
        {
            indexes[index] = nearest[Key(scaled[index])];
        }

        return new(palette, indexes, across, down);
    }

    private static Rgb[] Resample(Rgb[] pixels, int width, int height, int across, int down)
    {
        var scaled = new Rgb[across * down];

        for (var row = 0; row < down; row++)
        {
            var top = row * height / down;
            var bottom = Math.Max(top + 1, (row + 1) * height / down);

            for (var column = 0; column < across; column++)
            {
                var left = column * width / across;
                var right = Math.Max(left + 1, (column + 1) * width / across);
                var red = 0;
                var green = 0;
                var blue = 0;
                var taken = 0;

                for (var y = top; y < bottom; y++)
                {
                    for (var x = left; x < right; x++)
                    {
                        var pixel = pixels[(y * width) + x];

                        red += pixel.Red;
                        green += pixel.Green;
                        blue += pixel.Blue;
                        taken++;
                    }
                }

                scaled[(row * across) + column] =
                    new((byte)(red / taken), (byte)(green / taken), (byte)(blue / taken));
            }
        }

        return scaled;
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

    private static int Key(Rgb pixel) => (pixel.Red << 16) | (pixel.Green << 8) | pixel.Blue;

    private sealed class Channel(int shift) : IComparer<int>
    {
        private static readonly Channel[] Channels = [new(0), new(8), new(16)];

        public static Channel At(int shift) => Channels[shift / 8];

        public int Compare(int x, int y) => ((x >> shift) & 0xFF).CompareTo((y >> shift) & 0xFF);
    }
}
