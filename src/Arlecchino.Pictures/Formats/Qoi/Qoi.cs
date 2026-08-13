using System;
using System.Buffers.Binary;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Qoi;

/// <summary>
/// Reads a <c>QOI</c>, which is one pass over the bytes: a chunk is either a color, a small step from
/// the last one, a run of it, or one of the sixty-four colors seen recently.
/// </summary>
public sealed class Qoi : IPictureFormat
{
    private const int Header = 14;
    private const int Rgb = 0xFE;
    private const int Rgba = 0xFF;
    private const int Index = 0x00;
    private const int Diff = 0x40;
    private const int Luma = 0x80;
    private const int Run = 0xC0;

    /// <inheritdoc />
    public string Name => "qoi";

    /// <inheritdoc />
    public bool Starts(ReadOnlySpan<byte> bytes) => bytes.Length >= Header && bytes[..4].SequenceEqual("qoif"u8);

    /// <inheritdoc />
    public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        if (!Starts(bytes))
        {
            return null;
        }

        var width = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(4, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(8, 4));

        if (width <= 0 || height <= 0 || (long)width * height > limits.Most)
        {
            return null;
        }

        return Decode(bytes[Header..], width, height);
    }

    private static Raster? Decode(ReadOnlySpan<byte> body, int width, int height)
    {
        var painted = new Rgb[width * height];
        var seen = new Rgb[64];
        var alpha = new byte[64];
        var color = new Rgb(0, 0, 0);
        var opacity = (byte)255;
        var at = 0;

        for (var index = 0; index < painted.Length; index++)
        {
            if (at >= body.Length)
            {
                return null;
            }

            var tag = body[at++];
            var run = 0;

            switch (tag)
            {
                case Rgb when at + 3 <= body.Length:
                    color = new(body[at], body[at + 1], body[at + 2]);
                    at += 3;
                    break;

                case Rgba when at + 4 <= body.Length:
                    color = new(body[at], body[at + 1], body[at + 2]);
                    opacity = body[at + 3];
                    at += 4;
                    break;

                default:
                    switch (tag & 0xC0)
                    {
                        case Index:
                            color = seen[tag & 0x3F];
                            opacity = alpha[tag & 0x3F];
                            break;

                        case Diff:
                            color = new(
                                (byte)(color.Red + ((tag >> 4) & 0x03) - 2),
                                (byte)(color.Green + ((tag >> 2) & 0x03) - 2),
                                (byte)(color.Blue + (tag & 0x03) - 2));
                            break;

                        case Luma when at < body.Length:
                            var green = (tag & 0x3F) - 32;
                            var pair = body[at++];

                            color = new(
                                (byte)(color.Red + green - 8 + ((pair >> 4) & 0x0F)),
                                (byte)(color.Green + green),
                                (byte)(color.Blue + green - 8 + (pair & 0x0F)));

                            break;

                        case Run:
                            run = tag & 0x3F;
                            break;

                        default:
                            return null;
                    }

                    break;
            }

            var slot = Slot(color, opacity);

            seen[slot] = color;
            alpha[slot] = opacity;

            for (var repeat = 0; repeat <= run && index < painted.Length; repeat++)
            {
                painted[index] = color;

                if (repeat < run)
                {
                    index++;
                }
            }
        }

        return new(painted, width, height);
    }

    private static int Slot(Rgb color, byte opacity) =>
        ((color.Red * 3) + (color.Green * 5) + (color.Blue * 7) + (opacity * 11)) % 64;
}
