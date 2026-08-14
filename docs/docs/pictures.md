---
title: Pictures
sidebar_label: Pictures
description: Picture — an image drawn in cells, or sent to the terminal as pixels where the terminal takes them.
---

# Pictures

`Picture` draws an image. It needs nothing of the terminal by default — each cell carries two pixels,
the upper half block painted in the color of the pixel above and its background in the color of the
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

## Showing a file from disk

`Picture` draws pixels; turning a file into pixels is `Arlecchino.Pictures`, a package of its own so
that an application that only ever draws a plot carries no decoder at all.

```
dotnet add package Arlecchino.Pictures
```

```csharp
if (PictureFormats.Read(File.ReadAllBytes(path)) is { } raster)
{
    _preview.Show(raster.Pixels, raster.Width, raster.Height);
}
```

PNG, JPEG, BMP, Netpbm, QOI and Targa, each written against its own specification, depending on
`Arlecchino.Core` and on nothing native. JPEG is read both ways round, baseline and progressive.

A file is recognized by what is in it rather than by what it is called — `PictureFormats.For` says
which format claimed the bytes, and `PictureFormats.All` is the list. Nothing throws: what cannot be
read comes back as `null`, which is what a viewer opening arbitrary files needs. Whole-file reads
matter, since several of these formats are one stream from end to end and the first half of one
decodes to nothing.

### Two ceilings

`PictureLimits` says how much work a file is allowed to be:

| Limit | What it does |
|---|---|
| `Most` | The header is refused before anything is allocated against it. A file claiming more pixels than this is not read at all |
| `Enough` | How many pixels the caller has a use for. A format that can read itself smaller does |

`Enough` is where the time goes on a photograph: a JPEG drawn into a pane is read at a quarter or an
eighth of its side rather than in full. `PictureLimits.For(pixels)` builds a pair from the size the
pane can actually show, and `PictureLimits.Default` is what the plain `Read` uses.

```csharp
var raster = PictureFormats.Read(bytes, PictureLimits.For(region.Width * region.Height * 4));
```

Reach for a decoder from NuGet when you need what this package does not read — WebP, animation, raw
camera files. Check its license before you do; the popular ones are not all as free as they look.

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

`Detail` is the other half of that: how many pixels a protocol that hands pixels over may write at
most, whatever size the pane comes to. It sits at half a megapixel, which trades a little sharpness
for a picture that appears at once on a large pane; `0` lifts the ceiling.

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
| What color is behind the text? | `TerminalCapabilities.Background` |

Sixel is measured in pixels rather than cells, so the cell size is what decides how large a sixel
picture is drawn. When the terminal will not say, `CellWidth` and `CellHeight` from the options are
used instead — ten by twenty, which is close enough for most terminals to look right.

:::note[Why the background color is asked for]

Sixel writes pixels into the screen rather than into a registry of images, so a sixel is gone only
once something is drawn over it. Undrawing one means painting a rectangle in the color behind the
text, and a guessed color would leave a rectangle anyone can see. A terminal that never said is left
with the picture where it is, which is the lesser of the two.

:::

## Fitting

A picture is drawn as large as it goes inside the region without stretching, centered, and returns an
empty region — it fills what it is given, so hand it the pane it belongs in.

`Background` says what to draw behind it where the region is wider or taller than the picture fits;
left alone, that is the terminal's own background.

```csharp
var rest = _preview.Draw(region);   // rest is empty
```
