---
title: ProgressBar
sidebar_label: ProgressBar
---

# ProgressBar class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

A filled bar showing how far along something is, with an optional readout beside it.

```csharp
public sealed class ProgressBar : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`ProgressBar()`](#progressbar) |  |

## Properties

| Member | Summary |
|---|---|
| [`Caption`](#caption) | Builds the text drawn after the bar, given the value. Supplied as a delegate so the wording and units stay with the application rather than the widget. |
| [`Fraction`](#fraction) | How full the bar is, from `0` to `1`. An empty range reads as `0`. |
| [`Maximum`](#maximum) | Value at which the bar is full. |
| [`Minimum`](#minimum) | Value at which the bar is empty. |
| [`Style`](#style) | Colour of the filled part. The theme's active colour when left alone. |
| [`Value`](#value) | How far along it is now. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the bar across the first row of the region, leaving room for the caption when there is one, and returns the rows below it. |

## Constructors in detail

### `ProgressBar()` {#progressbar}

```csharp
public ProgressBar();
```

## Properties in detail

### `Caption` {#caption}

```csharp
public Func<decimal, string>? Caption { get; init; }
```

Builds the text drawn after the bar, given the value. Supplied as a delegate so the wording and units stay with the application rather than the widget.

**Type** `Func<T, TResult>`&lt;`decimal`, `string`&gt;

### `Fraction` {#fraction}

```csharp
public decimal Fraction { get; }
```

How full the bar is, from `0` to `1`. An empty range reads as `0`.

**Type** `decimal`

### `Maximum` {#maximum}

```csharp
public decimal Maximum { get; init; }
```

Value at which the bar is full.

**Type** `decimal`

### `Minimum` {#minimum}

```csharp
public decimal Minimum { get; init; }
```

Value at which the bar is empty.

**Type** `decimal`

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; init; }
```

Colour of the filled part. The theme's active colour when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Value` {#value}

```csharp
public decimal Value { get; set; }
```

How far along it is now.

**Type** `decimal`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the bar across the first row of the region, leaving room for the caption when there is one, and returns the rows below it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw; only its first row is used. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the bar.

