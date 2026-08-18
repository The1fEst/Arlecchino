namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// One component of a JPEG: the brightness, or one of the two colors that ride along with it. Each is
/// decoded into a plane of its own and stretched back out afterward.
/// </summary>
internal sealed class JpegPart
{
    /// <summary>What the scan header calls it.</summary>
    internal int Id { get; init; }

    /// <summary>How many blocks wide it is within one unit of the picture.</summary>
    internal int Wide { get; init; }

    /// <summary>How many blocks tall it is within one unit of the picture.</summary>
    internal int Tall { get; init; }

    /// <summary>Which quantization table its coefficients were divided by.</summary>
    internal int Quant { get; init; }

    /// <summary>Which Huffman table reads the first coefficient of a block.</summary>
    internal int Dc { get; set; }

    /// <summary>Which Huffman table reads the rest of them.</summary>
    internal int Ac { get; set; }

    /// <summary>
    /// The coefficients of every block, held until the last scan has been read. A progressive file says
    /// the same block several times over, at growing detail, so nothing can be turned into samples until
    /// all of it has been read.
    /// </summary>
    internal int[] Blocks { get; set; } = [];

    /// <summary>How many blocks wide the component is.</summary>
    internal int BlocksWide { get; set; }

    /// <summary>How many blocks tall the component is.</summary>
    internal int BlocksTall { get; set; }

    /// <summary>The samples, once the blocks are decoded.</summary>
    internal byte[] Plane { get; set; } = [];

    /// <summary>How wide the plane is, which is a whole number of blocks.</summary>
    internal int PlaneWidth { get; set; }

    /// <summary>The first coefficient of the last block, which the next one is written against.</summary>
    internal int Prediction { get; set; }
}
