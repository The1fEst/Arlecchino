---
title: IArlecchinoWidget
sidebar_label: IArlecchinoWidget
---

# IArlecchinoWidget interface

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of its own, so the same widget works in a pane, in a column or across the whole frame. This is the contract every built-in widget answers, and the one to implement for a widget of your own. A widget that also takes keys or the mouse implements [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md) instead.

```csharp
public interface IArlecchinoWidget
```

**Implemented by** [`Form`](../arlecchino.forms/Form.md), [`AreaChart`](../arlecchino.widgets/AreaChart.md), [`BarChart`](../arlecchino.widgets/BarChart-1.md), [`Gauge`](../arlecchino.widgets/Gauge.md), [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`ListBox`](../arlecchino.widgets/ListBox-1.md), [`Picture`](../arlecchino.widgets/Picture.md), [`ProgressBar`](../arlecchino.widgets/ProgressBar.md), [`ScrollPane`](../arlecchino.widgets/ScrollPane.md), [`Sparkline`](../arlecchino.widgets/Sparkline.md), [`Spinner`](../arlecchino.widgets/Spinner.md), [`StatusBar`](../arlecchino.widgets/StatusBar.md), [`Table`](../arlecchino.widgets/Table-1.md), [`Tabs`](../arlecchino.widgets/Tabs.md), [`TextView`](../arlecchino.widgets/TextView.md), [`Tree`](../arlecchino.widgets/Tree-1.md)

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the widget and answers what is left of the region underneath it, so the caller can stack the next thing without knowing how tall this one is. Called once per frame with the region it may paint; anything written outside is clipped rather than spilled onto a neighbour. A widget that fills whatever it is given — a list, a pane, a tree — returns an empty region. One that occupies a known number of rows returns the rest, which is what makes `var rest = header.Draw(surface.Content);` replace a hand-counted `SplitTop`. |

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the widget and answers what is left of the region underneath it, so the caller can stack the next thing without knowing how tall this one is. Called once per frame with the region it may paint; anything written outside is clipped rather than spilled onto a neighbour. A widget that fills whatever it is given — a list, a pane, a tree — returns an empty region. One that occupies a known number of rows returns the rest, which is what makes `var rest = header.Draw(surface.Content);` replace a hand-counted `SplitTop`.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw, in its own coordinates. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The part of `region` the widget did not paint, empty when none is.

