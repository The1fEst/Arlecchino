---
title: "Sparkline"
sidebar_label: "Sparkline"
---

# Sparkline class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

A series of numbers as one row of blocks, tallest for the largest of them, with no axis, scale or grid. The newest value is the rightmost, and only as many as fit the row are drawn.

```csharp
public sealed class Sparkline : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`Sparkline()`](#sparkline) |  |

## Properties

| Member | Summary |
|---|---|
| [`Caption`](#caption) | Builds the readout drawn after the line, given the newest value. Supplied as a delegate so the wording and units stay with the application rather than the widget. |
| [`Maximum`](#maximum) | The value the tallest block stands for. The largest of the drawn values when left alone. |
| [`Minimum`](#minimum) | The value the lowest block stands for, which is the smallest of the drawn values when left alone. Pinning it keeps the line still where the numbers barely change. |
| [`Style`](#style) | Color of the line. The theme's active color when left alone. |
| [`Values`](#values) | The numbers to draw, the oldest first. Nothing is copied, so a ring buffer the application appends to between frames is the right thing to hand over. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the line across the first row of the region, leaving room for the caption when there is one. A series with no spread at all draws as the lowest block rather than as a full row. |

## Constructors in detail

### `Sparkline()` {#sparkline}

```csharp
public Sparkline();
```

## Properties in detail

### `Caption` {#caption}

```csharp
public Func<decimal, string>? Caption { get; init; }
```

Builds the readout drawn after the line, given the newest value. Supplied as a delegate so the wording and units stay with the application rather than the widget.

**Type** `Func<T, TResult>`&lt;`decimal`, `string`&gt;

### `Maximum` {#maximum}

```csharp
public Nullable<decimal> Maximum { get; init; }
```

The value the tallest block stands for. The largest of the drawn values when left alone.

**Type** `Nullable<T>`&lt;`decimal`&gt;

### `Minimum` {#minimum}

```csharp
public Nullable<decimal> Minimum { get; init; }
```

The value the lowest block stands for, which is the smallest of the drawn values when left alone. Pinning it keeps the line still where the numbers barely change.

**Type** `Nullable<T>`&lt;`decimal`&gt;

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; init; }
```

Color of the line. The theme's active color when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Values` {#values}

```csharp
public IReadOnlyList<decimal> Values { get; set; }
```

The numbers to draw, the oldest first. Nothing is copied, so a ring buffer the application appends to between frames is the right thing to hand over.

**Type** `IReadOnlyList<T>`&lt;`decimal`&gt;

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the line across the first row of the region, leaving room for the caption when there is one. A series with no spread at all draws as the lowest block rather than as a full row.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw; only its first row is used. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the line.

