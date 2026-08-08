---
title: "AreaChart"
sidebar_label: "AreaChart"
---

# AreaChart class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

A series drawn as a filled area over as many rows as it is given — the shape a system monitor shows. Where [`Sparkline`](../arlecchino.widgets.readouts/Sparkline.md) fits a row and reads at a glance, this one fills a pane and is meant to be looked at. The newest value is at the right, the fill climbs with the value, and the color comes from how high it climbed rather than from anything the view works out. A series with no spread at all — every number the same — draws as the lowest level along the bottom rather than as nothing, the way a [`Sparkline`](../arlecchino.widgets.readouts/Sparkline.md) does. The resolution is in the characters. A cell carries two samples side by side and several levels of height, so a chart eight rows tall has thirty-two levels between empty and full and holds twice the history a row of blocks would. See [`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md) for what each set costs in font support.

```csharp
public sealed class AreaChart : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`AreaChart()`](#areachart) |  |

## Properties

| Member | Summary |
|---|---|
| [`Bands`](#bands) | Where the color changes as the fill climbs, in the same units as the values and in ascending order. A terminal with truecolor blends between them, one without takes the nearest, and a chart given none is drawn in [`AreaChart.Style`](../arlecchino.widgets.readouts/AreaChart.md#style) throughout. |
| [`Invert`](#invert) | Draws it hanging from the top rather than standing on the bottom, for the second half of a mirrored pair — what comes in above, what goes out below. |
| [`Maximum`](#maximum) | The value a full chart stands for. The largest of the drawn values when left alone. |
| [`Minimum`](#minimum) | The value an empty chart stands for. The smallest of the drawn values when left alone. |
| [`Style`](#style) | Color of the fill outside every band. The theme's active color when left alone. |
| [`Symbols`](#symbols) | What to draw with. The application's own setting — [`Glyphs.Graph`](../arlecchino.rendering.text/Glyphs.md#graph) — when left alone, so one chart can differ without every other one being told. |
| [`Values`](#values) | The numbers to draw, the oldest first. Nothing is copied, so a ring buffer the application appends to between frames is exactly the right thing to hand over. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the chart over every row of the region and returns what is left, which is nothing. A chart fills what it is given, so hand over the pane it belongs in. |

## Constructors in detail

### `AreaChart()` {#areachart}

```csharp
public AreaChart();
```

## Properties in detail

### `Bands` {#bands}

```csharp
public IReadOnlyList<GaugeBand> Bands { get; init; }
```

Where the color changes as the fill climbs, in the same units as the values and in ascending order. A terminal with truecolor blends between them, one without takes the nearest, and a chart given none is drawn in [`AreaChart.Style`](../arlecchino.widgets.readouts/AreaChart.md#style) throughout.

**Type** `IReadOnlyList<T>`&lt;[`GaugeBand`](../arlecchino.widgets.readouts/GaugeBand.md)&gt;

### `Invert` {#invert}

```csharp
public bool Invert { get; init; }
```

Draws it hanging from the top rather than standing on the bottom, for the second half of a mirrored pair — what comes in above, what goes out below.

**Type** `bool`

### `Maximum` {#maximum}

```csharp
public Nullable<decimal> Maximum { get; init; }
```

The value a full chart stands for. The largest of the drawn values when left alone.

**Type** `Nullable<T>`&lt;`decimal`&gt;

### `Minimum` {#minimum}

```csharp
public Nullable<decimal> Minimum { get; init; }
```

The value an empty chart stands for. The smallest of the drawn values when left alone.

**Type** `Nullable<T>`&lt;`decimal`&gt;

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; init; }
```

Color of the fill outside every band. The theme's active color when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Symbols` {#symbols}

```csharp
public Nullable<GraphSymbols> Symbols { get; set; }
```

What to draw with. The application's own setting — [`Glyphs.Graph`](../arlecchino.rendering.text/Glyphs.md#graph) — when left alone, so one chart can differ without every other one being told.

**Type** `Nullable<T>`&lt;[`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md)&gt;

### `Values` {#values}

```csharp
public IReadOnlyList<decimal> Values { get; set; }
```

The numbers to draw, the oldest first. Nothing is copied, so a ring buffer the application appends to between frames is exactly the right thing to hand over.

**Type** `IReadOnlyList<T>`&lt;`decimal`&gt;

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the chart over every row of the region and returns what is left, which is nothing. A chart fills what it is given, so hand over the pane it belongs in.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region.

