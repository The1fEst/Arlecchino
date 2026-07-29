---
title: Gauge
sidebar_label: Gauge
---

# Gauge class

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

One value against a range that means something, drawn as a bar whose colour changes as it crosses the bands it was given: the fill turns amber where the load is worth watching and red where it is not, and each part keeps the colour of the band it lies in, so the tail of the bar shows how long it has been past the line. A [`ProgressBar`](../arlecchino.widgets/ProgressBar.md) answers "how far along", and this answers "how bad is it now" — the difference being the bands, and a range that need not start at zero.

```csharp
public sealed class Gauge : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`Gauge()`](#gauge) |  |

## Properties

| Member | Summary |
|---|---|
| [`Bands`](#bands) | The bands the track is coloured by, in ascending order of [`GaugeBand.From`](../arlecchino.widgets/GaugeBand.md#from). Without them the whole fill takes [`Gauge.Style`](../arlecchino.widgets/Gauge.md#style), which makes the gauge a bar with a range. |
| [`Caption`](#caption) | Builds the text drawn after the gauge, given the value. |
| [`Fraction`](#fraction) | How full the gauge is, from `0` to `1`. An empty range reads as `0`. |
| [`Maximum`](#maximum) | Value at which the gauge reads full. |
| [`Minimum`](#minimum) | Value at which the gauge reads empty. |
| [`Style`](#style) | Colour of the fill outside every band. The theme's active colour when left alone. |
| [`Value`](#value) | What it reads now. Anything outside the range draws as an empty or a full gauge. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the gauge across the first row of the region, leaving room for the caption when there is one, and returns the rows below it. |
| [`StyleAt(decimal)`](#styleat-decimal) | How a value of the range is drawn: the style of the last band at or below it, and [`Gauge.Style`](../arlecchino.widgets/Gauge.md#style) when it is under every band. Useful for colouring a label the same way the gauge under it is coloured. |

## Constructors in detail

### `Gauge()` {#gauge}

```csharp
public Gauge();
```

## Properties in detail

### `Bands` {#bands}

```csharp
public IReadOnlyList<GaugeBand> Bands { get; init; }
```

The bands the track is coloured by, in ascending order of [`GaugeBand.From`](../arlecchino.widgets/GaugeBand.md#from). Without them the whole fill takes [`Gauge.Style`](../arlecchino.widgets/Gauge.md#style), which makes the gauge a bar with a range.

**Type** `IReadOnlyList<T>`&lt;[`GaugeBand`](../arlecchino.widgets/GaugeBand.md)&gt;

### `Caption` {#caption}

```csharp
public Func<decimal, string>? Caption { get; init; }
```

Builds the text drawn after the gauge, given the value.

**Type** `Func<T, TResult>`&lt;`decimal`, `string`&gt;

### `Fraction` {#fraction}

```csharp
public decimal Fraction { get; }
```

How full the gauge is, from `0` to `1`. An empty range reads as `0`.

**Type** `decimal`

### `Maximum` {#maximum}

```csharp
public decimal Maximum { get; init; }
```

Value at which the gauge reads full.

**Type** `decimal`

### `Minimum` {#minimum}

```csharp
public decimal Minimum { get; init; }
```

Value at which the gauge reads empty.

**Type** `decimal`

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; init; }
```

Colour of the fill outside every band. The theme's active colour when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md)

### `Value` {#value}

```csharp
public decimal Value { get; set; }
```

What it reads now. Anything outside the range draws as an empty or a full gauge.

**Type** `decimal`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the gauge across the first row of the region, leaving room for the caption when there is one, and returns the rows below it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw; only its first row is used. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the gauge.

### `StyleAt(decimal)` {#styleat-decimal}

```csharp
public IArlecchinoColor StyleAt(decimal value);
```

How a value of the range is drawn: the style of the last band at or below it, and [`Gauge.Style`](../arlecchino.widgets/Gauge.md#style) when it is under every band. Useful for colouring a label the same way the gauge under it is coloured.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `decimal` | The value to look up. |

**Returns** [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) — The style that part of the track takes.

