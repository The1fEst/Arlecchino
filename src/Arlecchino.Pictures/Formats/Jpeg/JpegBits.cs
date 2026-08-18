using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// The bits of a scan, held eight bytes at a time in a register. Inside a scan an <c>FF</c> byte
/// carries a stuffed nought; anything else after an <c>FF</c> is a marker.
/// </summary>
/// <param name="bytes">The file.</param>
/// <param name="at">Where the bits of the scan begin.</param>
internal ref struct JpegBits(ReadOnlySpan<byte> bytes, int at)
{
    private const int Register = 64;

    private readonly ReadOnlySpan<byte> _bytes = bytes;
    private int _at = at;
    private ulong _register;
    private int _count;
    private bool _spent;

    /// <summary>
    /// How far into the file the register has been filled from. Bytes are only ever taken up to the
    /// marker that ends the scan, never past it, so a caller looking for that marker searches forward
    /// from here.
    /// </summary>
    internal readonly int At => _at;

    /// <summary>
    /// Whether both the bytes and the register have run out. The register is filled ahead of what is
    /// being read, so bytes alone running out is not the end of the scan.
    /// </summary>
    internal readonly bool Ended => _spent && _count <= 0;

    /// <summary>Reads one bit, which is nought once the scan has ended.</summary>
    /// <returns>The bit.</returns>
    internal int Bit() => Read(1);

    /// <summary>Reads several bits, the deepest first.</summary>
    /// <param name="count">How many.</param>
    /// <returns>The value they spell.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Read(int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        var value = Peek(count);

        Skip(count);

        return value;
    }

    /// <summary>Looks at the next bits without taking them.</summary>
    /// <param name="count">How many, at most the width of the register.</param>
    /// <returns>The value they spell, filled out with noughts past the end of the scan.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Peek(int count)
    {
        if (_count < count)
        {
            Fill();
        }

        return (int)(_register >> (Register - count));
    }

    /// <summary>Steps over bits that have been looked at.</summary>
    /// <param name="count">How many.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Skip(int count)
    {
        _register <<= count;
        _count -= count;

        if (_count < 0)
        {
            _count = 0;
        }
    }

    /// <summary>
    /// Throws away the rest of the byte being read and steps over a restart marker, which is what stands
    /// between one run of blocks and the next.
    /// </summary>
    internal void Restart()
    {
        _register = 0;
        _count = 0;
        _spent = false;

        while (_at + 1 < _bytes.Length)
        {
            if (_bytes[_at] != 0xFF)
            {
                _at++;

                continue;
            }

            if (_bytes[_at + 1] == 0)
            {
                _at += 2;

                continue;
            }

            if (_bytes[_at + 1] is >= 0xD0 and <= 0xD7)
            {
                _at += 2;
            }

            return;
        }
    }

    /// <summary>
    /// Whether any of the eight bytes is <c>FF</c>, tested as one word. Subtracting one from each byte
    /// borrows only where that byte was nought, and flipping makes <c>FF</c> nought.
    /// </summary>
    /// <param name="eight">Eight bytes of the scan.</param>
    /// <returns><c>true</c> when one of them needs looking at.</returns>
    private static bool Marked(ulong eight)
    {
        var inverse = ~eight;

        return ((inverse - 0x0101010101010101UL) & ~inverse & 0x8080808080808080UL) != 0;
    }

    /// <summary>
    /// Tops the register up with whole bytes. A byte of <c>FF</c> carries a stuffed nought that is not
    /// part of the picture; anything else after an <c>FF</c> is the marker that ends the scan.
    /// </summary>
    private void Fill()
    {
        if (_at + 8 <= _bytes.Length)
        {
            var eight = BinaryPrimitives.ReadUInt64BigEndian(_bytes.Slice(_at, 8));

            if (!Marked(eight))
            {
                var room = (Register - _count) / 8 * 8;

                _register |= (eight >> (Register - room)) << (Register - _count - room);
                _count += room;
                _at += room / 8;

                return;
            }
        }

        while (_count <= Register - 8)
        {
            if (_spent || _at >= _bytes.Length)
            {
                _spent = true;

                return;
            }

            var value = _bytes[_at];

            if (value == 0xFF)
            {
                var next = _at + 1 < _bytes.Length ? _bytes[_at + 1] : -1;

                if (next is >= 0xD0 and <= 0xD7)
                {
                    return;
                }

                if (next != 0)
                {
                    _spent = true;

                    return;
                }

                _at += 2;
            }
            else
            {
                _at++;
            }

            _register |= (ulong)value << (Register - 8 - _count);
            _count += 8;
        }
    }
}
