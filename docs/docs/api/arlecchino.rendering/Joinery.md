---
title: Joinery
sidebar_label: Joinery
---

# Joinery class

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

Lines that know about one another. Boxes and rules are recorded first and painted at the end, so where two of them meet the shared cell becomes the glyph that joins them — `┬`, `├`, `┼` — instead of one line drawn over the other. [`SurfaceRegion.Border`](../arlecchino.rendering/SurfaceRegion.md#border-iarlecchinocolor-string) draws a box that knows nothing of its neighbours, which is right for a box standing on its own and wrong for panes that touch: two of those side by side put two verticals where the eye expects one. Recording them here instead costs one object per frame and gives the drawing of a window manager.

```csharp
var joinery = new Joinery();

var files = joinery.Box(left, Theme.Info, "files");
var log = joinery.Box(right, Theme.Active, "log");

joinery.Draw(surface.Content, Theme.Info);

```

A cell takes the style of the last thing recorded over it, so the pane that holds the focus is recorded last and its edges win where they are shared.

```csharp
public sealed class Joinery
```

## Constructors

| Member | Summary |
|---|---|
| [`Joinery()`](#joinery) |  |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many cells carry a line so far. Nothing has reached the surface yet. |

## Methods

| Member | Summary |
|---|---|
| [`Across(SurfaceRegion, int, IArlecchinoColor)`](#across-surfaceregion-int-iarlecchinocolor) | Records a rule across a region, for a divider that should join the box around it. |
| [`Box(SurfaceRegion, IArlecchinoColor, string)`](#box-surfaceregion-iarlecchinocolor-string) | Records the four edges of a region and hands back the room inside them, the way [`SurfaceRegion.Border`](../arlecchino.rendering/SurfaceRegion.md#border-iarlecchinocolor-string) does. |
| [`Down(SurfaceRegion, int, IArlecchinoColor)`](#down-surfaceregion-int-iarlecchinocolor) | Records a rule down a region. |
| [`Draw(SurfaceRegion, IArlecchinoColor)`](#draw-surfaceregion-iarlecchinocolor) | Paints everything recorded, resolving each cell into the glyph its neighbours ask for, and then writes the titles over the top edges they belong to. Anything falling outside the region is left undrawn rather than clamped into it. |

## Constructors in detail

### `Joinery()` {#joinery}

```csharp
public Joinery();
```

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many cells carry a line so far. Nothing has reached the surface yet.

**Type** `int`

## Methods in detail

### `Across(SurfaceRegion, int, IArlecchinoColor)` {#across-surfaceregion-int-iarlecchinocolor}

```csharp
public void Across(SurfaceRegion region, int row, IArlecchinoColor? style = null);
```

Records a rule across a region, for a divider that should join the box around it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | The region to cross. |
| `row` | `int` | Which of its rows, counted from its top. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | How it is drawn; the style given to [`Joinery.Draw`](../arlecchino.rendering/Joinery.md#draw-surfaceregion-iarlecchinocolor) when omitted. |

### `Box(SurfaceRegion, IArlecchinoColor, string)` {#box-surfaceregion-iarlecchinocolor-string}

```csharp
public SurfaceRegion Box(SurfaceRegion region, IArlecchinoColor? style = null, string title = "");
```

Records the four edges of a region and hands back the room inside them, the way [`SurfaceRegion.Border`](../arlecchino.rendering/SurfaceRegion.md#border-iarlecchinocolor-string) does.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | What to draw a box around. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | How its lines are drawn; the style given to [`Joinery.Draw`](../arlecchino.rendering/Joinery.md#draw-surfaceregion-iarlecchinocolor) when omitted. |
| `title` | `string` | What to write into the top edge, or nothing. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region inside the box.

### `Down(SurfaceRegion, int, IArlecchinoColor)` {#down-surfaceregion-int-iarlecchinocolor}

```csharp
public void Down(SurfaceRegion region, int column, IArlecchinoColor? style = null);
```

Records a rule down a region.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | The region to cross. |
| `column` | `int` | Which of its columns, counted from its left. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | How it is drawn; the style given to [`Joinery.Draw`](../arlecchino.rendering/Joinery.md#draw-surfaceregion-iarlecchinocolor) when omitted. |

### `Draw(SurfaceRegion, IArlecchinoColor)` {#draw-surfaceregion-iarlecchinocolor}

```csharp
public void Draw(SurfaceRegion into, IArlecchinoColor style);
```

Paints everything recorded, resolving each cell into the glyph its neighbours ask for, and then writes the titles over the top edges they belong to. Anything falling outside the region is left undrawn rather than clamped into it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `into` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to paint; coordinates recorded are the surface's own. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | How lines recorded without a style of their own are drawn. |

