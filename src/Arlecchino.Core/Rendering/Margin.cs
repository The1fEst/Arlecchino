namespace Arlecchino.Rendering;

/// <summary>
/// Blank space around something, measured in cells. Used both by the flow calls on
/// <see cref="Surface"/> and by <see cref="SurfaceRegion.Inset(Margin)"/>.
/// </summary>
/// <param name="Left">Cells kept free on the left.</param>
/// <param name="Top">Rows kept free above.</param>
/// <param name="Right">Cells kept free on the right.</param>
/// <param name="Bottom">Rows kept free below.</param>
public readonly record struct Margin(int Left, int Top, int Right, int Bottom)
{
    /// <summary>The same amount of space on all four sides.</summary>
    /// <param name="all">Cells kept free on every side.</param>
    public Margin(int all) : this(all, all, all, all) { }
}
