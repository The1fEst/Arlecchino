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
    /// <param name="newWidth">How wide it should be.</param>
    /// <param name="newHeight">How tall it should be.</param>
    /// <returns>The pixels to hand over, and how wide and tall they are.</returns>
    internal static (Rgb[] Pixels, int Width, int Height) To(
        Rgb[] pixels,
        int width,
        int height,
        int newWidth,
        int newHeight)
    {
        newWidth = Math.Max(1, newWidth);
        newHeight = Math.Max(1, newHeight);

        if (newWidth == width && newHeight == height)
        {
            return (pixels, width, height);
        }

        var result = new Rgb[newWidth * newHeight];

        for (var row = 0; row < newHeight; row++)
        {
            var top = row * height / newHeight;
            var bottom = Math.Max(top + 1, (row + 1) * height / newHeight);

            for (var column = 0; column < newWidth; column++)
            {
                var left = column * width / newWidth;
                var right = Math.Max(left + 1, (column + 1) * width / newWidth);
                var red = 0;
                var green = 0;
                var blue = 0;
                var samples = 0;

                for (var y = top; y < bottom; y++)
                {
                    for (var x = left; x < right; x++)
                    {
                        var pixel = pixels[(y * width) + x];

                        red += pixel.Red;
                        green += pixel.Green;
                        blue += pixel.Blue;
                        samples++;
                    }
                }

                result[(row * newWidth) + column] =
                    new((byte)(red / samples), (byte)(green / samples), (byte)(blue / samples));
            }
        }

        return (result, newWidth, newHeight);
    }
}
