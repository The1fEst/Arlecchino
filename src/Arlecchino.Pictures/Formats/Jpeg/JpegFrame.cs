using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// What the segments before a scan said: how large the picture is, which components it is made of, the
/// tables their coefficients were divided by, and the tables their bits are read with.
/// </summary>
internal sealed class JpegFrame
{
    /// <summary>How wide the picture is.</summary>
    internal int Width { get; private set; }

    /// <summary>How tall the picture is.</summary>
    internal int Height { get; private set; }

    /// <summary>How many blocks a unit of the picture is at its widest.</summary>
    internal int Wide { get; private set; } = 1;

    /// <summary>How many blocks a unit of the picture is at its tallest.</summary>
    internal int Tall { get; private set; } = 1;

    /// <summary>How many units stand between one restart marker and the next, or nought for none.</summary>
    internal int Restart { get; set; }

    /// <summary>What the components were told to do with their colors, when a file says so at all.</summary>
    internal int Transform { get; private set; } = -1;

    /// <summary>Whether the picture is written down several times over, at growing detail.</summary>
    internal bool Progressive { get; private set; }

    /// <summary>
    /// How many eighths of the picture are read: eight for all of it, one for an eighth of its side.
    /// Reading only the flattest waves of a block gives a smaller square of samples.
    /// </summary>
    internal int Eighths { get; private set; } = 8;

    /// <summary>How wide the picture comes out at the size it is being read.</summary>
    internal int Shown => Part(Width);

    /// <summary>How tall the picture comes out at the size it is being read.</summary>
    internal int Deep => Part(Height);

    /// <summary>The first coefficient the scan carries.</summary>
    internal int First { get; private set; }

    /// <summary>The last coefficient the scan carries.</summary>
    internal int Last { get; private set; } = 63;

    /// <summary>Which bit of a coefficient the scans so far have reached, or nought for the first of them.</summary>
    internal int Reached { get; private set; }

    /// <summary>Which bit of a coefficient this scan carries.</summary>
    internal int Carrying { get; private set; }

    /// <summary>The components, in the order the frame named them.</summary>
    internal List<JpegPart> Parts { get; } = [];

    /// <summary>The components this scan carries, which of a progressive file may be one of them.</summary>
    internal List<JpegPart> Scanned { get; } = [];

    /// <summary>The quantization tables, in the order the coefficients are read.</summary>
    internal int[]?[] Divisors { get; } = new int[]?[4];

    /// <summary>The tables that read the first coefficient of a block.</summary>
    internal JpegHuffman?[] DcTables { get; } = new JpegHuffman?[4];

    /// <summary>The tables that read the rest of the coefficients.</summary>
    internal JpegHuffman?[] AcTables { get; } = new JpegHuffman?[4];

    /// <summary>Reads the frame header, which says what the picture is made of.</summary>
    /// <param name="body">The segment.</param>
    /// <param name="limits">What the caller will hold and what it has a use for.</param>
    /// <param name="progressive">Whether the frame marker was the progressive one.</param>
    /// <returns><c>false</c> when the header is short, enormous, or names a picture that is not read.</returns>
    internal bool Sof(ReadOnlySpan<byte> body, PictureLimits limits, bool progressive)
    {
        if (body.Length < 6)
        {
            return false;
        }

        Progressive = progressive;

        Height = BinaryPrimitives.ReadUInt16BigEndian(body.Slice(1, 2));
        Width = BinaryPrimitives.ReadUInt16BigEndian(body.Slice(3, 2));

        var counted = body[5];

        if (body[0] != 8 || counted is not (1 or 3) || body.Length < 6 + (counted * 3))
        {
            return false;
        }

        if (Width <= 0 || Height <= 0 || (long)Width * Height > limits.Most)
        {
            return false;
        }

        Eighths = Smallest(limits.Enough);

        for (var index = 0; index < counted; index++)
        {
            var at = 6 + (index * 3);
            var part = new JpegPart
            {
                Id = body[at],
                Wide = body[at + 1] >> 4,
                Tall = body[at + 1] & 0x0F,
                Quant = body[at + 2],
            };

            if (part.Wide is < 1 or > 4 || part.Tall is < 1 or > 4 || part.Quant > 3)
            {
                return false;
            }

            Parts.Add(part);

            Wide = Math.Max(Wide, part.Wide);
            Tall = Math.Max(Tall, part.Tall);
        }

        return true;
    }

    /// <summary>
    /// The smallest of the four sizes a JPEG can be read at that still holds as many pixels as the
    /// caller says it has a use for. Asking for nothing in particular reads all of it.
    /// </summary>
    /// <param name="enough">How many pixels the caller has a use for.</param>
    /// <returns>Eighths of the full size: one, two, four or eight.</returns>
    private int Smallest(int enough)
    {
        if (enough <= 0)
        {
            return 8;
        }

        var eighths = 8;

        while (eighths > 1 && (long)Part(Width, eighths / 2) * Part(Height, eighths / 2) >= enough)
        {
            eighths /= 2;
        }

        return eighths;
    }

    private int Part(int whole) => Part(whole, Eighths);

    private static int Part(int whole, int eighths) => Math.Max(1, ((whole * eighths) + 7) / 8);

    /// <summary>Reads one or more Huffman tables.</summary>
    /// <param name="body">The segment.</param>
    /// <returns><c>false</c> when a table runs off the end of it.</returns>
    internal bool Huffman(ReadOnlySpan<byte> body)
    {
        var at = 0;

        while (at + 17 <= body.Length)
        {
            var kind = body[at] >> 4;
            var slot = body[at] & 0x0F;
            var counts = body.Slice(at + 1, 16).ToArray();
            var counted = 0;

            foreach (var count in counts)
            {
                counted += count;
            }

            if (slot > 3 || kind > 1 || at + 17 + counted > body.Length)
            {
                return false;
            }

            var table = new JpegHuffman(counts, body.Slice(at + 17, counted).ToArray());

            if (kind == 0)
            {
                DcTables[slot] = table;
            }
            else
            {
                AcTables[slot] = table;
            }

            at += 17 + counted;
        }

        return true;
    }

    /// <summary>Reads one or more quantization tables.</summary>
    /// <param name="body">The segment.</param>
    /// <returns><c>false</c> when a table runs off the end of it.</returns>
    internal bool Quantization(ReadOnlySpan<byte> body)
    {
        var at = 0;

        while (at < body.Length)
        {
            var wide = body[at] >> 4;
            var slot = body[at] & 0x0F;
            var size = wide == 1 ? 128 : 64;

            if (slot > 3 || wide > 1 || at + 1 + size > body.Length)
            {
                return false;
            }

            var table = new int[64];

            for (var index = 0; index < 64; index++)
            {
                table[index] = wide == 1
                    ? BinaryPrimitives.ReadUInt16BigEndian(body.Slice(at + 1 + (index * 2), 2))
                    : body[at + 1 + index];
            }

            Divisors[slot] = table;
            at += 1 + size;
        }

        return true;
    }

    /// <summary>
    /// Reads the scan header, which says which table reads which component. A progressive file also
    /// names which coefficients this scan carries, and to what depth.
    /// </summary>
    /// <param name="body">The segment.</param>
    /// <returns><c>false</c> when it names a component the frame did not, or a range that cannot be read.</returns>
    internal bool Scan(ReadOnlySpan<byte> body)
    {
        if (body.Length < 1)
        {
            return false;
        }

        var counted = body[0];

        if (counted < 1 || counted > Parts.Count || body.Length < 1 + (counted * 2) + 3)
        {
            return false;
        }

        Scanned.Clear();

        for (var index = 0; index < counted; index++)
        {
            var named = body[1 + (index * 2)];
            var part = Parts.Find(one => one.Id == named);

            if (part is null)
            {
                return false;
            }

            part.Dc = body[2 + (index * 2)] >> 4;
            part.Ac = body[2 + (index * 2)] & 0x0F;

            if (part.Dc > 3 || part.Ac > 3)
            {
                return false;
            }

            Scanned.Add(part);
        }

        var spectral = 1 + (counted * 2);

        First = body[spectral];
        Last = body[spectral + 1];
        Reached = body[spectral + 2] >> 4;
        Carrying = body[spectral + 2] & 0x0F;

        if (!Progressive)
        {
            return counted == Parts.Count && First == 0 && Last == 63 && Reached == 0 && Carrying == 0;
        }

        return First <= Last && Last <= 63 && Carrying <= 13 && (First != 0 || counted == Parts.Count);
    }

    /// <summary>Reads the Adobe segment, the one place a file says its colors are not the usual ones.</summary>
    /// <param name="body">The segment.</param>
    internal void Adobe(ReadOnlySpan<byte> body)
    {
        if (body.Length >= 12 && body[..5].SequenceEqual("Adobe"u8))
        {
            Transform = body[11];
        }
    }
}
