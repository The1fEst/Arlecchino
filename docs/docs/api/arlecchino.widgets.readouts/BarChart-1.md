---
title: BarChart&lt;T&gt;
sidebar_label: BarChart&lt;T&gt;
---

# BarChart&lt;T&gt; class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

One bar per item, laid out down the region: the label in front, the bar across the middle, the readout behind. Bars are measured against the largest item unless told otherwise, so a chart of things that are all small still fills the pane instead of drawing four invisible stubs.

```csharp
public sealed class BarChart<T> : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`BarChart()`](#barchart) |  |

## Properties

| Member | Summary |
|---|---|
| [`Caption`](#caption) | Builds the readout drawn after each bar, given that bar's value. The readouts share one column, as wide as the longest of them, so the numbers line up under one another. |
| [`ItemStyle`](#itemstyle) | Colours one bar, for charts where a row means something — over budget, offline, picked. |
| [`Items`](#items) | What to chart, one bar per row. Replacing it between frames is a normal thing to do. |
| [`LabelWidth`](#labelwidth) | Columns kept for the labels. The widest label when left alone, up to a third of the region so a long name cannot squeeze the bars out of the pane. |
| [`Maximum`](#maximum) | The value at which a bar is full. The largest of the items when left alone; pin it to compare one frame against the next, or to keep a percentage chart honest when nothing has reached 100 yet. |
| [`Render`](#render) | Turns an item into the label in front of its bar. Longer labels are truncated by column. |
| [`Value`](#value) | The number the length of the bar stands for. Anything below zero draws as an empty bar. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws a bar for every item that fits and returns the rows below them, so a chart shorter than its pane leaves the rest to whatever comes next. Items past the bottom of the region are not drawn: the chart does not scroll, which is what keeps it readable without the focus. |

## Constructors in detail

### `BarChart()` {#barchart}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public BarChart();
```

## Properties in detail

### `Caption` {#caption}

```csharp
public Func<decimal, string> Caption { get; init; }
```

Builds the readout drawn after each bar, given that bar's value. The readouts share one column, as wide as the longest of them, so the numbers line up under one another.

**Type** `Func<T, TResult>`&lt;`decimal`, `string`&gt;

### `ItemStyle` {#itemstyle}

```csharp
public Func<T, IArlecchinoColor> ItemStyle { get; set; }
```

Colours one bar, for charts where a row means something — over budget, offline, picked.

**Type** `Func<T, TResult>`&lt;`T`, [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)&gt;

### `Items` {#items}

```csharp
public IReadOnlyList<T> Items { get; set; }
```

What to chart, one bar per row. Replacing it between frames is a normal thing to do.

**Type** `IReadOnlyList<T>`&lt;`T`&gt;

### `LabelWidth` {#labelwidth}

```csharp
public int LabelWidth { get; init; }
```

Columns kept for the labels. The widest label when left alone, up to a third of the region so a long name cannot squeeze the bars out of the pane.

**Type** `int`

### `Maximum` {#maximum}

```csharp
public Nullable<decimal> Maximum { get; init; }
```

The value at which a bar is full. The largest of the items when left alone; pin it to compare one frame against the next, or to keep a percentage chart honest when nothing has reached 100 yet.

**Type** `Nullable<T>`&lt;`decimal`&gt;

### `Render` {#render}

```csharp
public Func<T, string> Render { get; init; }
```

Turns an item into the label in front of its bar. Longer labels are truncated by column.

**Type** `Func<T, TResult>`&lt;`T`, `string`&gt;

### `Value` {#value}

```csharp
public Func<T, decimal> Value { get; init; }
```

The number the length of the bar stands for. Anything below zero draws as an empty bar.

**Type** `Func<T, TResult>`&lt;`T`, `decimal`&gt;

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws a bar for every item that fits and returns the rows below them, so a chart shorter than its pane leaves the rest to whatever comes next. Items past the bottom of the region are not drawn: the chart does not scroll, which is what keeps it readable without the focus.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the bars.

