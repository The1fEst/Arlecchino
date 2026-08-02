---
title: Pictures
sidebar_label: Pictures
description: Picture — an image drawn in cells, or sent to the terminal as pixels where the terminal takes them.
---

# Pictures

`Picture` draws an image. It needs nothing of the terminal by default — each cell carries two pixels,
the upper half block painted in the colour of the pixel above and its background in the colour of the
one below, so a cell that is about twice as tall as it is wide comes out roughly square per pixel.
Where the terminal speaks a graphics protocol it sends the pixels themselves instead, and the picture
is as sharp as the screen allows.

```csharp
private readonly Picture _preview = new();

_preview.Show(pixels, width, height);
_preview.Draw(region);
```

`Show` copies the pixels, so the caller is free to reuse its buffer. `Clear` forgets them; whatever
the terminal was handed is undrawn on the next frame.

## Showing a PNG from disk

Arlecchino draws pixels rather than files. Decoding PNG or JPEG belongs to the application, which
knows what it wants to depend on — the framework carries no image library into your build.

A PNG needs no library. It is eight bytes of signature, a handful of chunks and one deflate stream,
and `ZLibStream` is in the framework already — a reader for the eight-bit non-interlaced files that
nearly all PNGs are fits in about two hundred lines:

```csharp
var raster = Png.Read(File.ReadAllBytes(path));

if (raster is not null)
{
    _preview.Show(raster.Pixels, raster.Width, raster.Height);
}
```

[Arlecchino Commander](https://github.com/The1fEst/Arlecchino.Commander/blob/master/src/Arlecchino.Commander/Files/Png.cs)
has that `Png` — grey, palette, truecolour and either with alpha, written so that anything it cannot
read comes back as `null` rather than throwing, which is what a viewer opening arbitrary files needs.
Copy it, or write your own against the
[specification](https://www.w3.org/TR/png/); the whole of the work is walking the chunks, inflating
the `IDAT`s and undoing five row filters.

Whole-file reads matter here: a PNG is one deflate stream from end to end, so the first half of one
decodes to nothing.

Reach for a decoder from NuGet when you need the formats a hand-written reader will not give you —
JPEG, WebP, animation. Check its licence before you do; the popular ones are not all as free as they
look.

## The array

An `Rgb` is three bytes, and the array runs row by row from the top left. Nothing says the pixels have
to come from a file — a plot, a heatmap or a gradient is the same array:

```csharp
var pixels = new Rgb[width * height];

for (var row = 0; row < height; row++)
{
    for (var column = 0; column < width; column++)
    {
        pixels[(row * width) + column] = new(red, green, blue);
    }
}
```

## Protocols

| Protocol | What it is |
|---|---|
| `Blocks` | Cells, two pixels each. Coarse, but works through the ordinary frame diff and leaves nothing behind |
| `Kitty` | The kitty graphics protocol — the pixels as they are. Kitty, WezTerm and Ghostty speak it |
| `Sixel` | The older protocol, and the one Windows Terminal, xterm, foot and WezTerm speak |
| `Auto` | The best of what the terminal admitted to when it was asked, and `Blocks` when it admitted to nothing |

`Auto` is the default, because the alternative is an application choosing on a terminal it cannot
see. Set `Picture.Protocol` for one picture, or `ArlecchinoOptions.ImageProtocol` for all of them.

```csharp
private readonly Picture _preview = new() { Protocol = ImageProtocol.Blocks };
```

## What the terminal was asked

Before the first frame the framework asks the terminal what it can do — see
[`TerminalProbe`](api/arlecchino.rendering.terminals/TerminalProbe.md) — and the answers are in
`TerminalCapabilities`. A terminal that says nothing costs the wait and leaves every setting as it
was.

| What was asked | Where the answer is |
|---|---|
| Does it speak sixel? | `TerminalCapabilities.Sixel` |
| Does it speak the kitty protocol? | `TerminalCapabilities.Kitty` |
| How large is a cell, in pixels? | `Glyphs.CellWidth`, `Glyphs.CellHeight`, and `TerminalCapabilities.CellSizeKnown` |
| What colour is behind the text? | `TerminalCapabilities.Background` |

Sixel is measured in pixels rather than cells, so the cell size is what decides how large a sixel
picture is drawn. When the terminal will not say, `CellWidth` and `CellHeight` from the options are
used instead — ten by twenty, which is close enough for most terminals to look right.

:::note[Why the background colour is asked for]

Sixel writes pixels into the screen rather than into a registry of images, so a sixel is gone only
once something is drawn over it. Undrawing one means painting a rectangle in the colour behind the
text, and a guessed colour would leave a rectangle anyone can see. A terminal that never said is left
with the picture where it is, which is the lesser of the two.

:::

## Fitting

A picture is drawn as large as it goes inside the region without stretching, centred, and returns an
empty region — it fills what it is given, so hand it the pane it belongs in.

`Background` says what to draw behind it where the region is wider or taller than the picture fits;
left alone, that is the terminal's own background.

```csharp
var rest = _preview.Draw(region);   // rest is empty
```
