---
title: Sparkline
sidebar_label: Sparkline
---

# Sparkline class

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

A series of numbers as one row of blocks, tallest for the largest of them. It says nothing about what the numbers are — no axis, no scale, no grid — which is what lets it sit in a status bar, a table cell or a corner of a pane and still be read at a glance: the shape of the line is the point. The newest value is the rightmost, and only the last of them fit the row, so a widening terminal shows more history rather than a wider drawing of the same history.

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
| [`Minimum`](#minimum) | The value the lowest block stands for. The smallest of the drawn values when left alone, which makes the line fill the row and answer "how does it move"; pinning it answers "how big is it" instead, and keeps the line still when the numbers barely change. |
| [`Style`](#style) | Colour of the line. The theme's active colour when left alone. |
| [`Values`](#values) | The numbers to draw, oldest first. Nothing is copied, so a ring buffer the application appends to between frames is exactly the right thing to hand over. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the line across the first row of the region, leaving room for the caption when there is one, and returns the rows below it. A series with no spread at all — every number the same, or one number on its own — draws as the lowest block rather than as a full row. |

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

The value the lowest block stands for. The smallest of the drawn values when left alone, which makes the line fill the row and answer "how does it move"; pinning it answers "how big is it" instead, and keeps the line still when the numbers barely change.

**Type** `Nullable<T>`&lt;`decimal`&gt;

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; init; }
```

Colour of the line. The theme's active colour when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md)

### `Values` {#values}

```csharp
public IReadOnlyList<decimal> Values { get; set; }
```

The numbers to draw, oldest first. Nothing is copied, so a ring buffer the application appends to between frames is exactly the right thing to hand over.

**Type** `IReadOnlyList<T>`&lt;`decimal`&gt;

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the line across the first row of the region, leaving room for the caption when there is one, and returns the rows below it. A series with no spread at all — every number the same, or one number on its own — draws as the lowest block rather than as a full row.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw; only its first row is used. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the line.

