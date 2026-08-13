using System;

namespace Arlecchino.Pictures;

/// <summary>
/// One picture format. It claims a file by the head of it and reads the whole of one into pixels,
/// answering <c>null</c> rather than throwing.
/// </summary>
public interface IPictureFormat
{
    /// <summary>What the format is called, in lower case, for a caller that shows it.</summary>
    string Name { get; }

    /// <summary>Whether the bytes begin the way this format does.</summary>
    /// <param name="bytes">The head of a file.</param>
    /// <returns><c>true</c> when the file is worth trying to read.</returns>
    bool Starts(ReadOnlySpan<byte> bytes);

    /// <summary>Reads the picture.</summary>
    /// <param name="bytes">The whole file.</param>
    /// <param name="limits">What the caller will hold and what it has a use for.</param>
    /// <returns>The pixels, or <c>null</c> when this is not a file of this format that can be read.</returns>
    Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits);
}
