using System;
using System.Collections.Generic;
using Arlecchino.Pictures.Formats.Bmp;
using Arlecchino.Pictures.Formats.Jpeg;
using Arlecchino.Pictures.Formats.Png;
using Arlecchino.Pictures.Formats.Pnm;
using Arlecchino.Pictures.Formats.Qoi;
using Arlecchino.Pictures.Formats.Tga;

namespace Arlecchino.Pictures;

/// <summary>
/// The formats that can be read, and the two questions asked of them: which format a file is, and
/// what it holds. A file is recognized by what is in it rather than by what it is called.
/// </summary>
public static class PictureFormats
{
    /// <summary>
    /// How many pixels are read at once when a caller does not say. A header states its own size, so a
    /// small file can ask for an enormous picture.
    /// </summary>
    public const int DefaultPixels = 32 * 1024 * 1024;

    private static readonly IPictureFormat[] Known =
    [
        new Png(),
        new Jpeg(),
        new Bmp(),
        new Qoi(),
        new Pnm(),
        new Tga(),
    ];

    /// <summary>Every format that is read, in the order a file is offered to them.</summary>
    public static IReadOnlyList<IPictureFormat> All => Known;

    /// <summary>Which format the file is.</summary>
    /// <param name="head">The head of a file; the signatures are all short.</param>
    /// <returns>The format, or <c>null</c> when none of them claims it.</returns>
    public static IPictureFormat? For(ReadOnlySpan<byte> head)
    {
        foreach (var format in Known)
        {
            if (format.Starts(head))
            {
                return format;
            }
        }

        return null;
    }

    /// <summary>Reads a picture of whichever format it turns out to be.</summary>
    /// <param name="bytes">The whole file.</param>
    /// <returns>The pixels, or <c>null</c> when nothing here can read it.</returns>
    public static Raster? Read(ReadOnlySpan<byte> bytes) => Read(bytes, PictureLimits.Default);

    /// <summary>Reads a picture of whichever format it turns out to be.</summary>
    /// <param name="bytes">The whole file.</param>
    /// <param name="limits">What the caller will hold and what it has a use for.</param>
    /// <returns>The pixels, or <c>null</c> when nothing here can read it.</returns>
    public static Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits) => For(bytes)?.Read(bytes, limits);
}
