namespace Arlecchino.Pictures;

/// <summary>
/// What a caller will hold, and what it has a use for. A format that can read itself at a smaller size
/// does so rather than decoding pixels that will never be drawn.
/// </summary>
/// <param name="Most">How many pixels may be held at once, refusing anything larger.</param>
/// <param name="Enough">
/// How many pixels the caller has a use for, or nought for as many as the picture holds. A decoder
/// answers with at least this many where it can; the size it lands on is its own.
/// </param>
public readonly record struct PictureLimits(int Most, int Enough)
{
    /// <summary>What a caller gets by asking for nothing in particular: the whole picture, within reason.</summary>
    public static PictureLimits Default { get; } = new(PictureFormats.DefaultPixels, 0);

    /// <summary>Limits for a caller that will draw the picture no larger than a given number of pixels.</summary>
    /// <param name="pixels">How many pixels the caller has a use for.</param>
    /// <returns>The limits.</returns>
    public static PictureLimits For(int pixels) => new(PictureFormats.DefaultPixels, pixels);
}
