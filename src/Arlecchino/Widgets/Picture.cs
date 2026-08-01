using System;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// An image drawn in cells. Each cell carries two pixels — the upper half block is painted in the
/// colour of the pixel above and its background in the colour of the pixel below — so a cell, which
/// is about twice as tall as it is wide, comes out roughly square per pixel.
///
/// That is the default because it needs nothing of the terminal but the colour it already draws in:
/// no protocol, no state left behind, nothing to clean up when the picture goes away. Where the
/// terminal speaks a graphics protocol, <see cref="Protocol"/> sends the pixels themselves instead and
/// the picture is as sharp as the screen allows.
///
/// The pixels are handed over rather than read from a file: decoding PNG or JPEG belongs to the
/// application, which knows what it wants to depend on, while the framework only draws what it is
/// given.
///
/// <code>
/// private readonly Picture _preview = new();
///
/// _preview.Show(pixels, width, height);
/// _preview.Draw(region);
/// </code>
/// </summary>
public sealed class Picture : IArlecchinoWidget
{
    private Rgb[] _pixels = [];
    private PictureProtocol? _drawing;
    private int _version;

    /// <summary>How wide the picture is, in pixels.</summary>
    public int PixelWidth { get; private set; }

    /// <summary>How tall the picture is, in pixels.</summary>
    public int PixelHeight { get; private set; }

    /// <summary>Whether there is anything to draw.</summary>
    public bool IsEmpty => _pixels.Length == 0;

    /// <summary>
    /// What to draw behind the picture where the region is wider or taller than the picture fits.
    /// The terminal's own background when left alone.
    /// </summary>
    public IArlecchinoColor? Background { get; init; }

    /// <summary>
    /// How the picture reaches the terminal. The application's own setting —
    /// <see cref="Glyphs.Picture"/> — when left alone, so one pane can differ without every other one
    /// being told.
    /// </summary>
    public ImageProtocol? Protocol { get; set; }

    /// <summary>
    /// Hands over the pixels to draw, row by row from the top left. They are copied, so the caller is
    /// free to reuse its buffer.
    /// </summary>
    /// <param name="pixels">The pixels, <paramref name="width"/> × <paramref name="height"/> of them.</param>
    /// <param name="width">How wide the picture is.</param>
    /// <param name="height">How tall the picture is.</param>
    public void Show(ReadOnlySpan<Rgb> pixels, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);

        if (pixels.Length < width * height)
        {
            throw new ArgumentException(
                $"a {width}×{height} picture needs {width * height} pixels, not {pixels.Length}",
                nameof(pixels));
        }

        _pixels = width * height == 0 ? [] : pixels[..(width * height)].ToArray();
        PixelWidth = width;
        PixelHeight = height;
        _version++;
    }

    /// <summary>
    /// Forgets the picture, leaving the region to whatever draws next. What the terminal was handed as
    /// pixels is undrawn on the next frame — see the undraw that goes with
    /// <see cref="Surface.Passthrough"/> — so this needs no more than forgetting them.
    /// </summary>
    public void Clear()
    {
        _pixels = [];
        PixelWidth = 0;
        PixelHeight = 0;
        _version++;
    }

    /// <summary>
    /// Draws the picture as large as it goes inside the region without stretching it, centred, and
    /// returns an empty region: a picture fills what it is given, so hand it the pane it belongs in.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        if (Background is not null)
        {
            region.Fill(Background);
        }

        if (IsEmpty)
        {
            return region.Rows(region.Height, 0);
        }

        var drawing = Chosen(TerminalCapabilities.Resolve(Protocol ?? Glyphs.Picture));
        var cellWidth = Math.Max(1, Glyphs.CellWidth);
        var cellHeight = Math.Max(1, Glyphs.CellHeight);

        var perCell = drawing.PerCell(cellWidth, cellHeight);

        var scale = Math.Min(
            (double)region.Width / PixelWidth,
            region.Height * perCell / PixelHeight);

        var columns = Math.Clamp((int)Math.Round(PixelWidth * scale), 1, region.Width);
        var rows = Math.Clamp(
            (int)Math.Round(PixelHeight * scale / perCell),
            1,
            region.Height);

        var left = (region.Width - columns) / 2;
        var top = (region.Height - rows) / 2;

        drawing.Draw(
            region,
            _pixels,
            PixelWidth,
            PixelHeight,
            new(left, top, columns, rows, cellWidth, cellHeight, _version));

        return region.Rows(region.Height, 0);
    }

    /// <summary>
    /// Picks the way of drawing that was asked for. This is the one place the protocol is named: the
    /// three ways share the arithmetic above and nothing else, so past this line each runs only its own
    /// code and holds only its own state.
    ///
    /// Only the one in use is ever built, and it is kept until the protocol changes — which is what
    /// makes the caches inside it worth having. A picture that is never drawn builds nothing at all, and
    /// a picture drawn in cells never takes a kitty image number it would not use.
    /// </summary>
    /// <param name="protocol">Which protocol the terminal and the application settled on.</param>
    /// <returns>The one that draws it.</returns>
    private PictureProtocol Chosen(ImageProtocol protocol)
    {
        if (_drawing?.Kind == protocol)
        {
            return _drawing;
        }

        return _drawing = protocol switch
        {
            ImageProtocol.Kitty => new KittyPicture(),
            ImageProtocol.Sixel => new SixelPicture(),
            _ => new BlockPicture(),
        };
    }
}
