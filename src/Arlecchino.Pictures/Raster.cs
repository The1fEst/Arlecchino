using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures;

/// <summary>What a picture turned out to hold, ready to be handed to a widget that draws pixels.</summary>
/// <param name="Pixels">The pixels, row by row from the top left.</param>
/// <param name="Width">How wide it is.</param>
/// <param name="Height">How tall it is.</param>
public sealed record Raster(Rgb[] Pixels, int Width, int Height);
