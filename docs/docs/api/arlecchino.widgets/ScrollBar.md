---
title: ScrollBar
sidebar_label: ScrollBar
---

# ScrollBar class

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

The bar down the side of a list that shows how much of it is on screen and where. Drawn only when there is more than fits, so a short list keeps its full width.

```csharp
public static class ScrollBar
```

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion, int, int, IArlecchinoColor)`](#draw-surfaceregion-int-int-iarlecchinocolor) | Draws the bar down the last column of a region. The thumb is at least one cell tall however long the list is, and it only touches the ends when the list does, so "near the end" never looks the same as "at the end". |
| [`IsNeeded(int, int)`](#isneeded-int-int) | Whether a list of this length needs a bar at all, which is also whether a column has to be kept free for it. |

## Methods in detail

### `Draw(SurfaceRegion, int, int, IArlecchinoColor)` {#draw-surfaceregion-int-int-iarlecchinocolor}

```csharp
public static void Draw(SurfaceRegion region, int first, int total, IArlecchinoColor? style = null);
```

Draws the bar down the last column of a region. The thumb is at least one cell tall however long the list is, and it only touches the ends when the list does, so "near the end" never looks the same as "at the end".

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where the rows were drawn; the last column is used. |
| `first` | `int` | Index of the first item on screen. |
| `total` | `int` | How many items there are. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | Colour of the thumb. Defaults to the theme's active colour. |

### `IsNeeded(int, int)` {#isneeded-int-int}

```csharp
public static bool IsNeeded(int total, int rows);
```

Whether a list of this length needs a bar at all, which is also whether a column has to be kept free for it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `total` | `int` | How many items there are. |
| `rows` | `int` | How many rows they are drawn into. |

**Returns** `bool` — `true` when some of the list is off screen.

