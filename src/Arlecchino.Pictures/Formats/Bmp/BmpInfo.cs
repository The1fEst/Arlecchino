namespace Arlecchino.Pictures.Formats.Bmp;

/// <summary>What the header of a bitmap said the picture is.</summary>
/// <param name="Width">How wide it is.</param>
/// <param name="Height">How tall it is, as a count of rows rather than as it was written down.</param>
/// <param name="TopDown">Whether the first row is the top one, which a negative height asks for.</param>
/// <param name="Bits">Bits a pixel.</param>
/// <param name="Compression">How the rows were written: plainly, run-length encoded, or with named masks.</param>
/// <param name="Used">How many palette entries are written down, or nought for as many as the depth allows.</param>
/// <param name="RedMask">Which bits of a pixel are the red, when the header names them.</param>
/// <param name="GreenMask">Which bits of a pixel are the green, when the header names them.</param>
/// <param name="BlueMask">Which bits of a pixel are the blue, when the header names them.</param>
internal readonly record struct BmpInfo(
    int Width,
    int Height,
    bool TopDown,
    int Bits,
    int Compression,
    int Used,
    uint RedMask,
    uint GreenMask,
    uint BlueMask)
{
    /// <summary>Whether the header named the bits of each color itself.</summary>
    internal bool Masks => (RedMask | GreenMask | BlueMask) != 0;
}
