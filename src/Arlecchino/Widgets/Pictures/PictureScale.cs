using System;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Widgets.Pictures;

/// <summary>
/// Brings a picture to the size it will be drawn at, averaging the source pixels each one covers.
/// </summary>
internal static class PictureScale
{
    /// <summary>
    /// Averages the picture to the given size, which may be smaller or larger. A picture already that
    /// size is handed back as it is.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="across">How wide it should be.</param>
    /// <param name="down">How tall it should be.</param>
    /// <returns>The pixels to hand over, and how wide and tall they are.</returns>
    internal static (Rgb[] Pixels, int Width, int Height) To(
        Rgb[] pixels,
        int width,
        int height,
        int across,
        int down)
    {
        across = Math.Max(1, across);
        down = Math.Max(1, down);

        if (across == width && down == height)
        {
            return (pixels, width, height);
        }

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

        return (scaled, across, down);
    }
}
