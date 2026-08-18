using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Arlecchino.Pictures.Formats.Png;

/// <summary>
/// Reads a PNG into pixels, dropping the alpha a terminal has nothing to show against. Every color type
/// and depth the format allows is read, interlaced or not.
/// </summary>
public sealed class Png : IPictureFormat
{
    private static readonly byte[] Signature = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A];

    /// <inheritdoc />
    public string Name => "png";

    /// <inheritdoc />
    public bool Starts(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= Signature.Length && bytes[..Signature.Length].SequenceEqual(Signature);

    /// <inheritdoc />
    public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        try
        {
            return Decode(bytes, limits.MostPixels);
        }
        catch (Exception failure) when (failure is InvalidDataException or IOException or
                                            ArgumentException or IndexOutOfRangeException or
                                            OverflowException or OutOfMemoryException)
        {
            return null;
        }
    }

    private Raster? Decode(ReadOnlySpan<byte> bytes, int pixels)
    {
        if (!Starts(bytes))
        {
            return null;
        }

        using var stream = new MemoryStream();

        var at = Signature.Length;
        var header = default(PngHeader);
        byte[]? palette = null;

        while (at + 8 <= bytes.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(at, 4));

            if (length < 0 || at + 12 + length > bytes.Length)
            {
                break;
            }

            var kind = Encoding.ASCII.GetString(bytes.Slice(at + 4, 4));
            var chunk = bytes.Slice(at + 8, length);

            at += 12 + length;

            switch (kind)
            {
                case "IHDR":
                    header = Header(chunk);
                    break;

                case "PLTE":
                    palette = chunk.ToArray();
                    break;

                case "IDAT":
                    stream.Write(chunk);
                    break;

                case "IEND":
                    at = bytes.Length;
                    break;
            }
        }

        if (!Sound(header, palette, pixels) || stream.Length == 0)
        {
            return null;
        }

        stream.Position = 0;

        return PngRows.Read(header, stream, palette);
    }

    /// <summary>Reads the header chunk, which every other chunk is read against.</summary>
    /// <param name="chunk">The body of the <c>IHDR</c> chunk.</param>
    /// <returns>What it says the picture is; an empty header when the chunk is too short to say.</returns>
    private static PngHeader Header(ReadOnlySpan<byte> chunk)
    {
        if (chunk.Length < 13)
        {
            return default;
        }

        return new(
            BinaryPrimitives.ReadInt32BigEndian(chunk[..4]),
            BinaryPrimitives.ReadInt32BigEndian(chunk.Slice(4, 4)),
            chunk[8],
            chunk[9],
            chunk[12] == 1);
    }

    /// <summary>
    /// Whether the header is one that can be read at all. A depth belongs to some color types and not to
    /// others, and a palette picture without a palette is nothing to look up.
    /// </summary>
    /// <param name="header">What the header chunk said.</param>
    /// <param name="palette">The palette, when one was written down.</param>
    /// <param name="pixels">How many pixels the caller will hold.</param>
    /// <returns><c>true</c> when the rows are worth unpacking.</returns>
    private static bool Sound(PngHeader header, byte[]? palette, int pixels)
    {
        if (header.Width <= 0 || header.Height <= 0 || (long)header.Width * header.Height > pixels)
        {
            return false;
        }

        if (header.Color == 3 && palette is null)
        {
            return false;
        }

        return header.Color switch
        {
            0 => header.Depth is 1 or 2 or 4 or 8 or 16,
            3 => header.Depth is 1 or 2 or 4 or 8,
            2 or 4 or 6 => header.Depth is 8 or 16,
            _ => false,
        };
    }
}
