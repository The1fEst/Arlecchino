namespace Arlecchino.Pictures.Formats.Png;

/// <summary>What the <c>IHDR</c> chunk said the picture is.</summary>
/// <param name="Width">How wide it is.</param>
/// <param name="Height">How tall it is.</param>
/// <param name="Depth">Bits a sample.</param>
/// <param name="Color">The PNG color type.</param>
/// <param name="Interlaced">Whether the rows were written down in seven passes.</param>
internal readonly record struct PngHeader(int Width, int Height, int Depth, int Color, bool Interlaced)
{
    /// <summary>How many samples one pixel is written as.</summary>
    internal int Channels => Color switch
    {
        0 => 1,
        2 => 3,
        3 => 1,
        4 => 2,
        6 => 4,
        _ => 0,
    };
}
