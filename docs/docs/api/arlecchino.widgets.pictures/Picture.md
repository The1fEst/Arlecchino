---
title: "Picture"
sidebar_label: "Picture"
---

# Picture class

**Namespace:** `Arlecchino.Widgets.Pictures` &middot; **Assembly:** `Arlecchino`

An image drawn in cells. Each cell carries two pixels — the upper half block is painted in the color of the pixel above and its background in the color of the pixel below — so a cell, which is about twice as tall as it is wide, comes out roughly square per pixel. That is the default because it needs nothing of the terminal but the color it already draws in: no protocol, no state left behind, nothing to clean up when the picture goes away. Where the terminal speaks a graphics protocol, [`Picture.Protocol`](../arlecchino.widgets.pictures/Picture.md#protocol) sends the pixels themselves instead and the picture is as sharp as the screen allows. The pixels are handed over rather than read from a file: decoding PNG or JPEG belongs to the application, which knows what it wants to depend on, while the framework only draws what it is given.

```csharp
private readonly Picture _preview = new();

_preview.Show(pixels, width, height);
_preview.Draw(region);

```

```csharp
public sealed class Picture : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`Picture()`](#picture) |  |

## Properties

| Member | Summary |
|---|---|
| [`Background`](#background) | What to draw behind the picture where the region is wider or taller than the picture fits. The terminal's own background when left alone. |
| [`IsEmpty`](#isempty) | Whether there is anything to draw. |
| [`PixelHeight`](#pixelheight) | How tall the picture is, in pixels. |
| [`PixelWidth`](#pixelwidth) | How wide the picture is, in pixels. |
| [`Protocol`](#protocol) | How the picture reaches the terminal. The application's own setting — [`Glyphs.Picture`](../arlecchino.rendering.text/Glyphs.md#picture) — when left alone, so one pane can differ without every other one being told. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Forgets the picture, leaving the region to whatever draws next. What the terminal was handed as pixels is undrawn on the next frame — see the undraw that goes with [`Surface.Passthrough`](../arlecchino.rendering/Surface.md#passthrough-int-int-string-string) — so this needs no more than forgetting them. |
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the picture as large as it goes inside the region without stretching it, centered, and returns an empty region: a picture fills what it is given, so hand it the pane it belongs in. |
| [`Show(ReadOnlySpan<Rgb>, int, int)`](#show-readonlyspan-rgb-int-int) | Hands over the pixels to draw, row by row from the top left. They are copied, so the caller is free to reuse its buffer. |

## Constructors in detail

### `Picture()` {#picture}

```csharp
public Picture();
```

## Properties in detail

### `Background` {#background}

```csharp
public IArlecchinoColor? Background { get; init; }
```

What to draw behind the picture where the region is wider or taller than the picture fits. The terminal's own background when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `IsEmpty` {#isempty}

```csharp
public bool IsEmpty { get; }
```

Whether there is anything to draw.

**Type** `bool`

### `PixelHeight` {#pixelheight}

```csharp
public int PixelHeight { get; }
```

How tall the picture is, in pixels.

**Type** `int`

### `PixelWidth` {#pixelwidth}

```csharp
public int PixelWidth { get; }
```

How wide the picture is, in pixels.

**Type** `int`

### `Protocol` {#protocol}

```csharp
public Nullable<ImageProtocol> Protocol { get; set; }
```

How the picture reaches the terminal. The application's own setting — [`Glyphs.Picture`](../arlecchino.rendering.text/Glyphs.md#picture) — when left alone, so one pane can differ without every other one being told.

**Type** `Nullable<T>`&lt;[`ImageProtocol`](../arlecchino.rendering.terminals/ImageProtocol.md)&gt;

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Forgets the picture, leaving the region to whatever draws next. What the terminal was handed as pixels is undrawn on the next frame — see the undraw that goes with [`Surface.Passthrough`](../arlecchino.rendering/Surface.md#passthrough-int-int-string-string) — so this needs no more than forgetting them.

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the picture as large as it goes inside the region without stretching it, centered, and returns an empty region: a picture fills what it is given, so hand it the pane it belongs in.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region.

### `Show(ReadOnlySpan<Rgb>, int, int)` {#show-readonlyspan-rgb-int-int}

```csharp
public void Show(ReadOnlySpan<Rgb> pixels, int width, int height);
```

Hands over the pixels to draw, row by row from the top left. They are copied, so the caller is free to reuse its buffer.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pixels` | `ReadOnlySpan<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt; | The pixels, `width` × `height` of them. |
| `width` | `int` | How wide the picture is. |
| `height` | `int` | How tall the picture is. |

