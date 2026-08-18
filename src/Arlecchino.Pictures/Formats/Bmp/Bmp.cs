using System;
using System.Buffers.Binary;

namespace Arlecchino.Pictures.Formats.Bmp;

/// <summary>
/// Reads a Windows bitmap. The rows stand bottom to top unless the height is negative, the colors are
/// written blue first, and the rows come either plainly or run-length encoded.
/// </summary>
public sealed class Bmp : IPictureFormat
{
    private const int FileHeader = 14;
    private const int Core = 12;
    private const int Info = 40;

    /// <inheritdoc />
    public string Name => "bmp";

    /// <inheritdoc />
    public bool Starts(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= FileHeader + Core && bytes[0] == (byte)'B' && bytes[1] == (byte)'M';

    /// <inheritdoc />
    public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        if (!Starts(bytes))
        {
            return null;
        }

        var offset = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(10, 4));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(FileHeader, 4));

        if (size < Core || FileHeader + size > bytes.Length)
        {
            return null;
        }

        var header = size == Core ? Older(bytes) : Newer(bytes, size);

        if (header is null)
        {
            return null;
        }

        var info = header.Value;

        if (info.Width <= 0 || info.Height <= 0 || (long)info.Width * info.Height > limits.MostPixels)
        {
            return null;
        }

        if (offset <= 0 || offset > bytes.Length)
        {
            return null;
        }

        var palette = Palette(bytes, info, size);

        return info.Compression is 1 or 2
            ? BmpPacked.Read(bytes, info, palette, offset)
            : BmpRows.Read(bytes, info, palette, offset);
    }

    /// <summary>Reads the header a bitmap has had since Windows 3, and the versions that extend it.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="size">How long the header says it is.</param>
    /// <returns>What it says the picture is, or <c>null</c> when it is compressed in a way that is not read.</returns>
    private static BmpInfo? Newer(ReadOnlySpan<byte> bytes, int size)
    {
        if (size < Info)
        {
            return null;
        }

        var head = bytes[FileHeader..];
        var height = BinaryPrimitives.ReadInt32LittleEndian(head.Slice(8, 4));
        var bits = BinaryPrimitives.ReadInt16LittleEndian(head.Slice(14, 2));
        var compression = BinaryPrimitives.ReadInt32LittleEndian(head.Slice(16, 4));

        if (compression is not (0 or 1 or 2 or 3 or 6))
        {
            return null;
        }

        var masked = compression is 3 or 6;
        var maskAt = size >= 108 ? FileHeader + 40 : FileHeader + size;

        return new(
            BinaryPrimitives.ReadInt32LittleEndian(head.Slice(4, 4)),
            Math.Abs(height),
            height < 0,
            bits,
            compression,
            BinaryPrimitives.ReadInt32LittleEndian(head.Slice(32, 4)),
            masked ? Mask(bytes, maskAt) : 0,
            masked ? Mask(bytes, maskAt + 4) : 0,
            masked ? Mask(bytes, maskAt + 8) : 0);
    }

    /// <summary>Reads the header of an OS/2 bitmap, which states the size in two bytes and has no more to say.</summary>
    /// <param name="bytes">The file.</param>
    /// <returns>What it says the picture is.</returns>
    private static BmpInfo? Older(ReadOnlySpan<byte> bytes)
    {
        var head = bytes[FileHeader..];

        return new(
            BinaryPrimitives.ReadInt16LittleEndian(head.Slice(4, 2)),
            BinaryPrimitives.ReadInt16LittleEndian(head.Slice(6, 2)),
            false,
            BinaryPrimitives.ReadInt16LittleEndian(head.Slice(10, 2)),
            0,
            0,
            0,
            0,
            0);
    }

    /// <summary>
    /// The palette, as three bytes a color. A bitmap writes four bytes an entry and an OS/2 one writes
    /// three, and either way the count may be left at nought to mean as many as the depth allows.
    /// </summary>
    /// <param name="bytes">The file.</param>
    /// <param name="header">What the header said.</param>
    /// <param name="size">How long the header is.</param>
    /// <returns>The colors, or <c>null</c> when the depth does not look anything up.</returns>
    private static byte[]? Palette(ReadOnlySpan<byte> bytes, in BmpInfo header, int size)
    {
        if (header.Bits > 8)
        {
            return null;
        }

        var entry = size == Core ? 3 : 4;
        var at = FileHeader + size;
        var entries = header.Used > 0 ? header.Used : 1 << header.Bits;
        var colors = new byte[entries * 3];

        for (var index = 0; index < entries; index++)
        {
            var from = at + (index * entry);

            if (from + 2 >= bytes.Length)
            {
                break;
            }

            colors[index * 3] = bytes[from + 2];
            colors[(index * 3) + 1] = bytes[from + 1];
            colors[(index * 3) + 2] = bytes[from];
        }

        return colors;
    }

    private static uint Mask(ReadOnlySpan<byte> bytes, int at) =>
        at + 4 <= bytes.Length ? BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(at, 4)) : 0;
}
