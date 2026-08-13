namespace Arlecchino.Pictures.Formats.Jpeg;

/// <summary>
/// Holds the blocks of a progressive picture while the scans fill them in, and turns them into samples
/// once the last scan has been read.
/// </summary>
internal static class JpegPlanes
{
    /// <summary>Makes room for every block of every component.</summary>
    /// <param name="frame">What the frame header said.</param>
    internal static void Room(JpegFrame frame)
    {
        var across = (frame.Width + (frame.Wide * 8) - 1) / (frame.Wide * 8);
        var down = (frame.Height + (frame.Tall * 8) - 1) / (frame.Tall * 8);

        foreach (var part in frame.Parts)
        {
            part.BlocksWide = across * part.Wide;
            part.BlocksTall = down * part.Tall;
            part.PlaneWidth = part.BlocksWide * frame.Eighths;
            part.Blocks = new int[part.BlocksWide * part.BlocksTall * 64];
        }
    }

    /// <summary>Turns the blocks of every component into its samples.</summary>
    /// <param name="frame">The components, with their blocks filled in.</param>
    /// <returns><c>false</c> when a component was divided by a table the file never wrote down.</returns>
    internal static bool Fill(JpegFrame frame)
    {
        var blocks = new JpegBlocks();

        foreach (var part in frame.Parts)
        {
            var divisors = frame.Divisors[part.Quant];

            if (divisors is null)
            {
                return false;
            }

            part.Plane = new byte[part.PlaneWidth * part.BlocksTall * frame.Eighths];

            for (var row = 0; row < part.BlocksTall; row++)
            {
                for (var column = 0; column < part.BlocksWide; column++)
                {
                    blocks.Restore(
                        part.Blocks,
                        ((row * part.BlocksWide) + column) * 64,
                        divisors,
                        part.Plane,
                        part.PlaneWidth,
                        column * frame.Eighths,
                        row * frame.Eighths,
                        frame.Eighths);
                }
            }
        }

        return true;
    }
}
