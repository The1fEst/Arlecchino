using System.Runtime.CompilerServices;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// One Huffman table: how many codes there are of each length, and the values they stand for. Every
/// code of nine bits or fewer is also laid out in a table read by the next nine bits themselves.
/// </summary>
internal sealed class JpegHuffman
{
    private const int Quick = 9;

    private readonly int[] _smallest = new int[17];
    private readonly int[] _largest = new int[17];
    private readonly int[] _first = new int[17];
    private readonly short[] _ready = new short[1 << Quick];
    private readonly byte[] _values;

    /// <summary>Builds the table from the counts and the values.</summary>
    /// <param name="counts">How many codes there are of each length, from one bit to sixteen.</param>
    /// <param name="values">What the codes stand for, in the order the codes run.</param>
    internal JpegHuffman(byte[] counts, byte[] values)
    {
        _values = values;

        var code = 0;
        var index = 0;

        for (var length = 1; length <= 16; length++)
        {
            _first[length] = index;
            _smallest[length] = code;

            for (var counted = 0; counted < counts[length - 1]; counted++)
            {
                if (length <= Quick && index < values.Length)
                {
                    var from = (code + counted) << (Quick - length);

                    for (var fill = 0; fill < 1 << (Quick - length); fill++)
                    {
                        _ready[from + fill] = (short)((length << 8) | values[index]);
                    }
                }

                index++;
            }

            code += counts[length - 1];

            _largest[length] = counts[length - 1] == 0 ? -1 : code - 1;
            code <<= 1;
        }
    }

    /// <summary>Reads one value.</summary>
    /// <param name="bits">Where the bits come from.</param>
    /// <returns>The value, or <c>-1</c> when sixteen bits name no code at all.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Read(ref JpegBits bits)
    {
        var ready = _ready[bits.Peek(Quick)];

        if (ready != 0)
        {
            bits.Skip(ready >> 8);

            return ready & 0xFF;
        }

        var code = 0;

        for (var length = 1; length <= 16; length++)
        {
            code = (code << 1) | bits.Bit();

            if (_largest[length] < 0 || code > _largest[length])
            {
                continue;
            }

            var index = _first[length] + code - _smallest[length];

            return index >= 0 && index < _values.Length ? _values[index] : -1;
        }

        return -1;
    }
}
