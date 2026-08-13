using System;
using System.Buffers.Binary;
using System.IO;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// Reads a JPEG. A baseline file is turned into samples as it is read; a progressive one is held as
/// coefficients until the last of its scans has been read.
/// </summary>
public sealed class Jpeg : IPictureFormat
{
    /// <inheritdoc />
    public string Name => "jpeg";

    /// <inheritdoc />
    public bool Starts(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

    /// <inheritdoc />
    public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        try
        {
            return Decode(bytes, limits);
        }
        catch (Exception failure) when (failure is InvalidDataException or ArgumentException or
                                            IndexOutOfRangeException or OverflowException or
                                            OutOfMemoryException)
        {
            return null;
        }
    }

    private Raster? Decode(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        if (!Starts(bytes))
        {
            return null;
        }

        var frame = new JpegFrame();
        var at = 2;

        while (at + 3 < bytes.Length)
        {
            if (bytes[at] != 0xFF)
            {
                return null;
            }

            var marker = bytes[at + 1];

            at += 2;

            if (marker is 0xD8 or 0x01 or (>= 0xD0 and <= 0xD7))
            {
                continue;
            }

            if (marker == 0xD9)
            {
                return frame.Progressive && JpegPlanes.Fill(frame) ? JpegColors.Read(frame) : null;
            }

            var length = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(at, 2));

            if (length < 2 || at + length > bytes.Length)
            {
                return null;
            }

            var body = bytes.Slice(at + 2, length - 2);

            if (marker == 0xDA)
            {
                if (!frame.Scan(body))
                {
                    return null;
                }

                if (!frame.Progressive)
                {
                    return JpegScan.Read(bytes, at + length, frame) ? JpegColors.Read(frame) : null;
                }

                at = JpegProgressive.Read(bytes, at + length, frame);

                if (at < 0)
                {
                    return null;
                }

                continue;
            }

            if (!Segment(frame, marker, body, limits))
            {
                return null;
            }

            at += length;
        }

        return frame.Progressive && JpegPlanes.Fill(frame) ? JpegColors.Read(frame) : null;
    }

    /// <summary>Reads one segment of the file, and says which of them cannot be read at all.</summary>
    /// <param name="frame">What the segments so far have said.</param>
    /// <param name="marker">Which segment this is.</param>
    /// <param name="body">Its body, without the two bytes that hold the length.</param>
    /// <param name="limits">What the caller will hold and what it has a use for.</param>
    /// <returns><c>false</c> when the segment is malformed or names a picture that is not read.</returns>
    private static bool Segment(JpegFrame frame, byte marker, ReadOnlySpan<byte> body, PictureLimits limits) => marker switch
    {
        0xC0 or 0xC1 => frame.Sof(body, limits, false),
        0xC2 => frame.Sof(body, limits, true) && Room(frame),
        0xC4 => frame.Huffman(body),
        0xDB => frame.Quantization(body),
        0xDD => body.Length >= 2 && Restart(frame, body),
        0xEE => Told(frame, body),
        >= 0xC3 and <= 0xCF => false,
        _ => true,
    };

    private static bool Room(JpegFrame frame)
    {
        JpegPlanes.Room(frame);

        return true;
    }

    private static bool Restart(JpegFrame frame, ReadOnlySpan<byte> body)
    {
        frame.Restart = BinaryPrimitives.ReadUInt16BigEndian(body[..2]);

        return true;
    }

    private static bool Told(JpegFrame frame, ReadOnlySpan<byte> body)
    {
        frame.Adobe(body);

        return true;
    }
}
