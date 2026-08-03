---
title: Spinner
sidebar_label: Spinner
---

# Spinner class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

A one-cell animation for work of unknown length. It does not run on its own: something has to step it, which keeps the framework free of timers the application did not ask for.

```csharp
public sealed class Spinner : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`Spinner()`](#spinner) |  |

## Properties

| Member | Summary |
|---|---|
| [`Current`](#current) | The frame to draw right now. |
| [`Frames`](#frames) | The frames cycled through. Braille dots by default, which most terminals render in one cell. |
| [`Style`](#style) | Colour to draw in. The theme's informational colour when left alone. |

## Methods

| Member | Summary |
|---|---|
| [`Advance()`](#advance) | Moves to the next frame, wrapping at the end. |
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the current frame in the first cell of the region and returns the rows below it. One cell is all a spinner needs, so hand it the cell it belongs in — `region.Rows(0, 1)`, a column split, or whatever the layout gives. |

## Constructors in detail

### `Spinner()` {#spinner}

```csharp
public Spinner();
```

## Properties in detail

### `Current` {#current}

```csharp
public string Current { get; }
```

The frame to draw right now.

**Type** `string`

### `Frames` {#frames}

```csharp
public string[] Frames { get; init; }
```

The frames cycled through. Braille dots by default, which most terminals render in one cell.

**Type** `string`\[\]

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; init; }
```

Colour to draw in. The theme's informational colour when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

## Methods in detail

### `Advance()` {#advance}

```csharp
public void Advance();
```

Moves to the next frame, wrapping at the end.

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the current frame in the first cell of the region and returns the rows below it. One cell is all a spinner needs, so hand it the cell it belongs in — `region.Rows(0, 1)`, a column split, or whatever the layout gives.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw; the top-left cell is used. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the spinner's row.

