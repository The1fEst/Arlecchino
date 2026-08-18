using System;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// Reads the scan: the picture in units of one block from each component at its own size, each block a
/// difference from the one before it for the first coefficient and a run of the rest.
/// </summary>
internal static class JpegScan
{
    /// <summary>Decodes every block into the planes of the components.</summary>
    /// <param name="bytes">The file.</param>
    /// <param name="at">Where the bits of the scan begin.</param>
    /// <param name="frame">What the segments before the scan said.</param>
    /// <returns><c>false</c> when a table is missing or the bits name a code that is not in one.</returns>
    internal static bool Read(ReadOnlySpan<byte> bytes, int at, JpegFrame frame)
    {
        var blockColumns = (frame.Width + (frame.Wide * 8) - 1) / (frame.Wide * 8);
        var blockRows = (frame.Height + (frame.Tall * 8) - 1) / (frame.Tall * 8);

        var side = frame.Eighths;

        foreach (var part in frame.Parts)
        {
            part.PlaneWidth = blockColumns * part.Wide * side;
            part.Plane = new byte[part.PlaneWidth * blockRows * part.Tall * side];
            part.Prediction = 0;
        }

        var bits = new JpegBits(bytes, at);
        var coefficients = new int[64];
        var blocks = new JpegBlocks();
        var sinceRestart = 0;

        for (var unit = 0; unit < blockColumns * blockRows && !bits.Ended; unit++, sinceRestart++)
        {
            if (frame.Restart > 0 && sinceRestart == frame.Restart)
            {
                bits.Restart();

                sinceRestart = 0;

                foreach (var part in frame.Parts)
                {
                    part.Prediction = 0;
                }
            }

            foreach (var part in frame.Parts)
            {
                var divisors = frame.Divisors[part.Quant];

                if (divisors is null)
                {
                    return false;
                }

                for (var row = 0; row < part.Tall; row++)
                {
                    for (var column = 0; column < part.Wide; column++)
                    {
                        if (!Block(ref bits, frame, part, coefficients))
                        {
                            return false;
                        }

                        blocks.Restore(
                            coefficients,
                            0,
                            divisors,
                            part.Plane,
                            part.PlaneWidth,
                            (((unit % blockColumns) * part.Wide) + column) * side,
                            (((unit / blockColumns) * part.Tall) + row) * side,
                            side);
                    }
                }
            }
        }

        return true;
    }

    /// <summary>Reads one block of sixty-four coefficients.</summary>
    /// <param name="bits">The bits of the scan.</param>
    /// <param name="frame">What the segments before the scan said.</param>
    /// <param name="part">Which component the block belongs to.</param>
    /// <param name="coefficients">Where the block is read into.</param>
    /// <returns><c>false</c> when a table is missing or a code is not in one.</returns>
    private static bool Block(ref JpegBits bits, JpegFrame frame, JpegPart part, int[] coefficients)
    {
        var first = frame.DcTables[part.Dc];
        var rest = frame.AcTables[part.Ac];

        if (first is null || rest is null)
        {
            return false;
        }

        foreach (var place in JpegCoefficients.Places[frame.Eighths])
        {
            coefficients[place] = 0;
        }

        var size = first.Read(ref bits);

        if (size is < 0 or > 15)
        {
            return false;
        }

        part.Prediction += size == 0 ? 0 : JpegCoefficients.Signed(bits.Read(size), size);
        coefficients[0] = part.Prediction;

        var index = 1;

        while (index < 64)
        {
            var symbol = rest.Read(ref bits);

            if (symbol < 0)
            {
                return false;
            }

            var run = symbol >> 4;
            var width = symbol & 0x0F;

            if (width == 0)
            {
                if (run != 15)
                {
                    break;
                }

                index += 16;

                continue;
            }

            index += run;

            if (index >= 64)
            {
                break;
            }

            coefficients[index] = JpegCoefficients.Signed(bits.Read(width), width);
            index++;
        }

        return true;
    }
}
