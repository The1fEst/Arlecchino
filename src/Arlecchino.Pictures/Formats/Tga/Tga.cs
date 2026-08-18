using System;
using System.Buffers.Binary;

namespace Arlecchino.Pictures.Formats.Tga;

/// <summary>
/// Reads a <c>Targa</c>. The format begins with no signature of its own, so a file is claimed only
/// when every field of the header is one of those the format allows.
/// </summary>
public sealed class Tga : IPictureFormat
{
    private const int Header = 18;

    /// <inheritdoc />
    public string Name => "tga";

    /// <inheritdoc />
    public bool Starts(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < Header)
        {
            return false;
        }

        var mapKind = bytes[1];
        var kind = bytes[2];
        var entry = bytes[7];
        var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(12, 2));
        var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(14, 2));

        if (mapKind > 1 || kind is not (1 or 2 or 3 or 9 or 10 or 11) || width == 0 || height == 0)
        {
            return false;
        }

        if (bytes[16] is not (8 or 15 or 16 or 24 or 32) || (bytes[17] & 0xD0) != 0)
        {
            return false;
        }

        var wants = kind is 1 or 9;

        return mapKind == (wants ? 1 : 0) && (mapKind == 0 || entry is 15 or 16 or 24 or 32);
    }

    /// <inheritdoc />
    public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        if (!Starts(bytes))
        {
            return null;
        }

        var kind = bytes[2];
        var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(12, 2));
        var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(14, 2));

        if ((long)width * height > limits.MostPixels)
        {
            return null;
        }

        var entries = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(5, 2));
        var entry = bytes[1] == 1 ? bytes[7] : 0;
        var map = Header + bytes[0];
        var at = map + (entries * ((entry + 7) / 8));

        if (at > bytes.Length)
        {
            return null;
        }

        var size = bytes[16] == 8 ? 1 : bytes[16] <= 16 ? 2 : bytes[16] / 8;
        var raw = kind >= 9
            ? Unpacked(bytes, at, width * height, size)
            : Straight(bytes, at, width * height, size);

        if (raw is null)
        {
            return null;
        }

        var palette = bytes[1] == 1
            ? TgaPixels.Palette(bytes, map, BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(3, 2)), entries, entry)
            : null;

        return TgaPixels.Read(raw, width, height, bytes[16], kind, palette, (bytes[17] & 0x20) != 0);
    }

    private static byte[]? Straight(ReadOnlySpan<byte> bytes, int at, int count, int size)
    {
        if (at + ((long)count * size) > bytes.Length)
        {
            return null;
        }

        return bytes.Slice(at, count * size).ToArray();
    }

    /// <summary>
    /// Undoes the run-length encoding. A packet is either one pixel to repeat or a count of pixels that
    /// follow, and a run is allowed to carry on across the end of a row.
    /// </summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where the packets begin.</param>
    /// <param name="count">How many pixels the picture holds.</param>
    /// <param name="size">How many bytes one pixel takes.</param>
    /// <returns>The pixels as they were before packing, or <c>null</c> when the packets run out.</returns>
    private static byte[]? Unpacked(ReadOnlySpan<byte> bytes, int at, int count, int size)
    {
        var raw = new byte[count * size];
        var offset = 0;

        while (offset < raw.Length)
        {
            if (at >= bytes.Length)
            {
                return null;
            }

            var packet = bytes[at++];
            var run = (packet & 0x7F) + 1;

            if ((packet & 0x80) != 0)
            {
                if (at + size > bytes.Length)
                {
                    return null;
                }

                for (var repeat = 0; repeat < run && offset < raw.Length; repeat++, offset += size)
                {
                    bytes.Slice(at, size).CopyTo(raw.AsSpan(offset));
                }

                at += size;

                continue;
            }

            var span = run * size;

            if (at + span > bytes.Length)
            {
                return null;
            }

            var length = Math.Min(span, raw.Length - offset);

            bytes.Slice(at, length).CopyTo(raw.AsSpan(offset));

            offset += length;
            at += length;
        }

        return raw;
    }
}
