using System;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// Reads a progressive scan. Such a file says the same blocks several times over, each scan carrying
/// either new coefficients or one more bit of the ones already read.
/// </summary>
internal static class JpegProgressive
{
    /// <summary>Reads one scan into the coefficients of the components it names.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where the bits of the scan begin.</param>
    /// <param name="frame">What the segments so far have said, including this scan's header.</param>
    /// <returns>Where the marker after the scan stands, or <c>-1</c> when the scan cannot be read.</returns>
    internal static int Read(ReadOnlySpan<byte> bytes, int at, JpegFrame frame)
    {
        var bits = new JpegBits(bytes, at);
        var run = 0;

        foreach (var part in frame.Parts)
        {
            part.Predicted = 0;
        }

        var single = frame.Scanned.Count == 1;
        var across = single ? Wide(frame, frame.Scanned[0]) : (frame.Width + (frame.Wide * 8) - 1) / (frame.Wide * 8);
        var down = single ? Tall(frame, frame.Scanned[0]) : (frame.Height + (frame.Tall * 8) - 1) / (frame.Tall * 8);
        var since = 0;

        for (var unit = 0; unit < across * down && !bits.Ended; unit++, since++)
        {
            if (frame.Restart > 0 && since == frame.Restart)
            {
                bits.Restart();

                since = 0;
                run = 0;

                foreach (var part in frame.Parts)
                {
                    part.Predicted = 0;
                }
            }

            if (single)
            {
                var part = frame.Scanned[0];
                var into = (((unit / across) * part.BlocksWide) + (unit % across)) * 64;

                if (!Block(ref bits, frame, part, into, ref run))
                {
                    return -1;
                }

                continue;
            }

            foreach (var part in frame.Scanned)
            {
                for (var row = 0; row < part.Tall; row++)
                {
                    for (var column = 0; column < part.Wide; column++)
                    {
                        var block = ((((unit / across) * part.Tall) + row) * part.BlocksWide) + ((unit % across) * part.Wide) + column;

                        if (!Block(ref bits, frame, part, block * 64, ref run))
                        {
                            return -1;
                        }
                    }
                }
            }
        }

        return Next(bytes, bits.At);
    }

    private static int Wide(JpegFrame frame, JpegPart part) =>
        (((frame.Width * part.Wide / frame.Wide) + 7) / 8) is var counted && counted > 0 ? counted : 1;

    private static int Tall(JpegFrame frame, JpegPart part) =>
        (((frame.Height * part.Tall / frame.Tall) + 7) / 8) is var counted && counted > 0 ? counted : 1;

    /// <summary>Reads one block, in whichever of the four ways this scan asks for.</summary>
    /// <param name="bits">The bits of the scan.</param>
    /// <param name="frame">What the segments so far have said.</param>
    /// <param name="part">Which component the block belongs to.</param>
    /// <param name="into">Where the block stands among the coefficients of the component.</param>
    /// <param name="run">How many blocks are known to be finished already.</param>
    /// <returns><c>false</c> when a table is missing or a code is not in one.</returns>
    private static bool Block(ref JpegBits bits, JpegFrame frame, JpegPart part, int into, ref int run)
    {
        if (into < 0 || into + 64 > part.Blocks.Length)
        {
            return false;
        }

        if (frame.First == 0)
        {
            return frame.Reached == 0
                ? FirstDc(ref bits, frame, part, into)
                : RefinedDc(ref bits, frame, part, into);
        }

        return frame.Reached == 0
            ? FirstAc(ref bits, frame, part, into, ref run)
            : RefinedAc(ref bits, frame, part, into, ref run);
    }

    private static bool FirstDc(ref JpegBits bits, JpegFrame frame, JpegPart part, int into)
    {
        var table = frame.DcTables[part.Dc];

        if (table is null)
        {
            return false;
        }

        var size = table.Read(ref bits);

        if (size is < 0 or > 15)
        {
            return false;
        }

        part.Predicted += size == 0 ? 0 : JpegCoefficients.Signed(bits.Read(size), size);
        part.Blocks[into] = part.Predicted << frame.Carrying;

        return true;
    }

    private static bool RefinedDc(ref JpegBits bits, JpegFrame frame, JpegPart part, int into)
    {
        if (bits.Bit() != 0)
        {
            part.Blocks[into] |= 1 << frame.Carrying;
        }

        return true;
    }

    private static bool FirstAc(ref JpegBits bits, JpegFrame frame, JpegPart part, int into, ref int run)
    {
        if (run > 0)
        {
            run--;

            return true;
        }

        var table = frame.AcTables[part.Ac];

        if (table is null)
        {
            return false;
        }

        var index = frame.First;

        while (index <= frame.Last)
        {
            var symbol = table.Read(ref bits);

            if (symbol < 0)
            {
                return false;
            }

            var skip = symbol >> 4;
            var size = symbol & 0x0F;

            if (size == 0)
            {
                if (skip < 15)
                {
                    run = (1 << skip) - 1 + (skip > 0 ? bits.Read(skip) : 0);

                    break;
                }

                index += 16;

                continue;
            }

            index += skip;

            if (index > frame.Last)
            {
                break;
            }

            part.Blocks[into + index] =
                JpegCoefficients.Signed(bits.Read(size), size) << frame.Carrying;
            index++;
        }

        return true;
    }

    /// <summary>
    /// Reads one more bit of the coefficients an earlier scan already named, and of the ones it did not,
    /// starts them at that bit. What is skipped over still carries a bit for every value already there.
    /// </summary>
    /// <param name="bits">The bits of the scan.</param>
    /// <param name="frame">What the segments so far have said.</param>
    /// <param name="part">Which component the block belongs to.</param>
    /// <param name="into">Where the block stands among the coefficients of the component.</param>
    /// <param name="run">How many blocks are known to hold nothing new.</param>
    /// <returns><c>false</c> when a table is missing or a code is not in one.</returns>
    private static bool RefinedAc(ref JpegBits bits, JpegFrame frame, JpegPart part, int into, ref int run)
    {
        var table = frame.AcTables[part.Ac];

        if (table is null)
        {
            return false;
        }

        var deeper = 1 << frame.Carrying;
        var shallower = -1 << frame.Carrying;
        var index = frame.First;

        if (run == 0)
        {
            while (index <= frame.Last)
            {
                var symbol = table.Read(ref bits);

                if (symbol < 0)
                {
                    return false;
                }

                var skip = symbol >> 4;
                var size = symbol & 0x0F;
                var value = 0;

                if (size == 0)
                {
                    if (skip < 15)
                    {
                        run = (1 << skip) + (skip > 0 ? bits.Read(skip) : 0);

                        break;
                    }
                }
                else
                {
                    value = bits.Bit() != 0 ? deeper : shallower;
                }

                while (index <= frame.Last)
                {
                    var coefficient = into + index;

                    if (part.Blocks[coefficient] != 0)
                    {
                        Deepen(ref bits, part.Blocks, coefficient, deeper, shallower);
                    }
                    else
                    {
                        if (skip == 0)
                        {
                            if (value != 0)
                            {
                                part.Blocks[coefficient] = value;
                            }

                            index++;

                            break;
                        }

                        skip--;
                    }

                    index++;
                }
            }
        }

        if (run <= 0)
        {
            return true;
        }

        while (index <= frame.Last)
        {
            var coefficient = into + index;

            if (part.Blocks[coefficient] != 0)
            {
                Deepen(ref bits, part.Blocks, coefficient, deeper, shallower);
            }

            index++;
        }

        run--;

        return true;
    }

    private static void Deepen(ref JpegBits bits, int[] blocks, int at, int deeper, int shallower)
    {
        if (bits.Bit() == 0 || (blocks[at] & deeper) != 0)
        {
            return;
        }

        blocks[at] += blocks[at] >= 0 ? deeper : shallower;
    }

    /// <summary>
    /// Where the marker after the scan stands. A scan ends at the first <c>FF</c> that is neither a
    /// stuffed nought nor a restart, and a decoder that stopped early has to walk to it.
    /// </summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where reading got to.</param>
    /// <returns>The marker, or the end of the file.</returns>
    private static int Next(ReadOnlySpan<byte> bytes, int at)
    {
        for (var index = Math.Max(at, 0); index + 1 < bytes.Length; index++)
        {
            if (bytes[index] == 0xFF && bytes[index + 1] != 0 && bytes[index + 1] is not (>= 0xD0 and <= 0xD7))
            {
                return index;
            }
        }

        return bytes.Length;
    }
}
