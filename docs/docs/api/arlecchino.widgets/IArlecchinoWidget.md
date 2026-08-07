---
title: "IArlecchinoWidget"
sidebar_label: "IArlecchinoWidget"
---

# IArlecchinoWidget interface

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of its own, so the same widget works in a pane, in a column or across the whole frame. This is the contract every built-in widget answers, and the one to implement for a widget of your own. A widget that also takes keys or the mouse implements [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md) instead.

```csharp
public interface IArlecchinoWidget
```

**Implemented by** [`Form`](../arlecchino.forms/Form.md), [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`ListBox`](../arlecchino.widgets.lists/ListBox-1.md), [`ScrollPane`](../arlecchino.widgets.lists/ScrollPane.md), [`Table`](../arlecchino.widgets.lists/Table-1.md), [`Tabs`](../arlecchino.widgets.lists/Tabs.md), [`Tree`](../arlecchino.widgets.lists/Tree-1.md), [`Picture`](../arlecchino.widgets.pictures/Picture.md), [`AreaChart`](../arlecchino.widgets.readouts/AreaChart.md), [`BarChart`](../arlecchino.widgets.readouts/BarChart-1.md), [`Gauge`](../arlecchino.widgets.readouts/Gauge.md), [`ProgressBar`](../arlecchino.widgets.readouts/ProgressBar.md), [`Sparkline`](../arlecchino.widgets.readouts/Sparkline.md), [`Spinner`](../arlecchino.widgets.readouts/Spinner.md), [`StatusBar`](../arlecchino.widgets.readouts/StatusBar.md), [`TextView`](../arlecchino.widgets.readouts/TextView.md)

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the widget and answers what is left of the region underneath it, so the caller can stack the next thing without knowing how tall this one is. Called once per frame with the region it may paint; anything written outside is clipped rather than spilled onto a neighbor. A widget that fills whatever it is given — a list, a pane, a tree — returns an empty region. One that occupies a known number of rows returns the rest, which is what makes `var rest = header.Draw(surface.Content);` replace a hand-counted `SplitTop`. |

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the widget and answers what is left of the region underneath it, so the caller can stack the next thing without knowing how tall this one is. Called once per frame with the region it may paint; anything written outside is clipped rather than spilled onto a neighbor. A widget that fills whatever it is given — a list, a pane, a tree — returns an empty region. One that occupies a known number of rows returns the rest, which is what makes `var rest = header.Draw(surface.Content);` replace a hand-counted `SplitTop`.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw, in its own coordinates. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The part of `region` the widget did not paint, empty when none is.

