---
title: "SurfaceRegion"
sidebar_label: "SurfaceRegion"
---

# SurfaceRegion struct

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

A rectangle on a [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) with its own coordinates and clipping, so writing outside it is dropped. The same geometry answers where a click landed.

```csharp
public readonly struct SurfaceRegion : IEquatable<SurfaceRegion>
```

**Implements** `IEquatable<T>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`SurfaceRegion(Surface, int, int, int, int)`](#surfaceregion-surface-int-int-int-int) | A rectangle on a [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) with its own coordinates and clipping, so writing outside it is dropped. The same geometry answers where a click landed. |

## Properties

| Member | Summary |
|---|---|
| [`Bottom`](#bottom) | Frame row just past the bottom edge. |
| [`Height`](#height) | Height in rows. |
| [`IsEmpty`](#isempty) | Whether the region has no room to draw in. |
| [`Left`](#left) | Frame column of the left edge. |
| [`Right`](#right) | Frame column just past the right edge. |
| [`Surface`](#surface) | The surface drawn into. |
| [`Top`](#top) | Frame row of the top edge. |
| [`Width`](#width) | Width in cells. |

## Methods

| Member | Summary |
|---|---|
| [`Border(IArlecchinoColor, string)`](#border-iarlecchinocolor-string) | Draws a box around the region and hands back the space inside it, so panes and dialogs are one call rather than four loops. |
| [`Contains(int, int)`](#contains-int-int) | Whether a frame cell falls inside this region — the hit test for mouse events. |
| [`Deconstruct(out Surface, out int, out int, out int, out int)`](#deconstruct-out-surface-out-int-out-int-out-int-out-int) |  |
| [`Fill(IArlecchinoColor, char)`](#fill-iarlecchinocolor-char) | Paints every cell of the region. |
| [`Flow()`](#flow) | A cursor that writes the next line of this region and remembers where the one after it goes. The flow calls on [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) belong to the whole frame; this one stays inside the region. |
| [`Inset(Margin)`](#inset-margin) | A smaller region inside this one. |
| [`Inset(int)`](#inset-int) | A smaller region with the same space kept free on every side. |
| [`Rows(int, int)`](#rows-int-int) | A horizontal band of this region, clamped to its bounds. |
| [`SplitLeft(int)`](#splitleft-int) | Cuts a column off the left. The split is clamped to what the region actually has. |
| [`SplitTop(int)`](#splittop-int) | Cuts a band off the top. The split is clamped to what the region actually has. |
| [`ToLocal(int, int)`](#tolocal-int-int) | Converts a frame cell into coordinates local to this region. |
| [`Write(int, int, string, IArlecchinoColor)`](#write-int-int-string-iarlecchinocolor) | Writes text in region coordinates, clipped to the region. A negative column starts the text off the left edge and shows what fits. |
| [`WriteLine(int, string, IArlecchinoColor, Align)`](#writeline-int-string-iarlecchinocolor-align) | Writes a whole line, aligned inside the region and clipped to its width. |

## Constructors in detail

### `SurfaceRegion(Surface, int, int, int, int)` {#surfaceregion-surface-int-int-int-int}

```csharp
public SurfaceRegion(Surface Surface, int Left, int Top, int Width, int Height);
```

A rectangle on a [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) with its own coordinates and clipping, so writing outside it is dropped. The same geometry answers where a click landed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Surface` | [`Surface`](../arlecchino.rendering/Surface.md) | The surface drawn into. |
| `Left` | `int` | Frame column of the left edge. |
| `Top` | `int` | Frame row of the top edge. |
| `Width` | `int` | Width in cells. |
| `Height` | `int` | Height in rows. |

## Properties in detail

### `Bottom` {#bottom}

```csharp
public int Bottom { get; }
```

Frame row just past the bottom edge.

**Type** `int`

### `Height` {#height}

```csharp
public int Height { get; init; }
```

Height in rows.

**Type** `int`

### `IsEmpty` {#isempty}

```csharp
public bool IsEmpty { get; }
```

Whether the region has no room to draw in.

**Type** `bool`

### `Left` {#left}

```csharp
public int Left { get; init; }
```

Frame column of the left edge.

**Type** `int`

### `Right` {#right}

```csharp
public int Right { get; }
```

Frame column just past the right edge.

**Type** `int`

### `Surface` {#surface}

```csharp
public Surface Surface { get; init; }
```

The surface drawn into.

**Type** [`Surface`](../arlecchino.rendering/Surface.md)

### `Top` {#top}

```csharp
public int Top { get; init; }
```

Frame row of the top edge.

**Type** `int`

### `Width` {#width}

```csharp
public int Width { get; init; }
```

Width in cells.

**Type** `int`

## Methods in detail

### `Border(IArlecchinoColor, string)` {#border-iarlecchinocolor-string}

```csharp
public SurfaceRegion Border(IArlecchinoColor style, string title = "");
```

Draws a box around the region and hands back the space inside it, so panes and dialogs are one call rather than four loops.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the frame. |
| `title` | `string` | Optional title written into the top edge. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region inside the frame, or this region when it is too small for one.

### `Contains(int, int)` {#contains-int-int}

```csharp
public bool Contains(int frameRow, int frameColumn);
```

Whether a frame cell falls inside this region — the hit test for mouse events.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frameRow` | `int` | Row in frame coordinates. |
| `frameColumn` | `int` | Column in frame coordinates. |

**Returns** `bool` — `true` when the cell is inside.

### `Deconstruct(out Surface, out int, out int, out int, out int)` {#deconstruct-out-surface-out-int-out-int-out-int-out-int}

```csharp
public void Deconstruct(
    out Surface Surface,
    out int Left,
    out int Top,
    out int Width,
    out int Height);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Surface` | [`Surface`](../arlecchino.rendering/Surface.md) |  |
| `Left` | `int` |  |
| `Top` | `int` |  |
| `Width` | `int` |  |
| `Height` | `int` |  |

### `Fill(IArlecchinoColor, char)` {#fill-iarlecchinocolor-char}

```csharp
public void Fill(IArlecchinoColor style, char character = ' ');
```

Paints every cell of the region.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style to paint with. |
| `character` | `char` | Character to fill with; a space by default. |

### `Flow()` {#flow}

```csharp
public PaneFlow Flow();
```

A cursor that writes the next line of this region and remembers where the one after it goes. The flow calls on [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) belong to the whole frame; this one stays inside the region.

**Returns** [`PaneFlow`](../arlecchino.rendering/PaneFlow.md) — A flow starting at the first row of the region.

### `Inset(Margin)` {#inset-margin}

```csharp
public SurfaceRegion Inset(Margin margin);
```

A smaller region inside this one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `margin` | [`Margin`](../arlecchino.rendering/Margin.md) | Space to keep free on each side. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region that is left.

### `Inset(int)` {#inset-int}

```csharp
public SurfaceRegion Inset(int size);
```

A smaller region with the same space kept free on every side.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `size` | `int` | Cells to keep free. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region that is left.

### `Rows(int, int)` {#rows-int-int}

```csharp
public SurfaceRegion Rows(int row, int count);
```

A horizontal band of this region, clamped to its bounds.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | First row, local to this region. |
| `count` | `int` | How many rows to take. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The band.

### `SplitLeft(int)` {#splitleft-int}

```csharp
public ValueTuple<SurfaceRegion, SurfaceRegion> SplitLeft(int width);
```

Cuts a column off the left. The split is clamped to what the region actually has.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `width` | `int` | Cells to give to the left part. |

**Returns** `ValueTuple<T1, T2>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md), [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt; — The left part and the rest.

### `SplitTop(int)` {#splittop-int}

```csharp
public ValueTuple<SurfaceRegion, SurfaceRegion> SplitTop(int height);
```

Cuts a band off the top. The split is clamped to what the region actually has.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `height` | `int` | Rows to give to the top part. |

**Returns** `ValueTuple<T1, T2>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md), [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt; — The top part and the rest.

### `ToLocal(int, int)` {#tolocal-int-int}

```csharp
public ValueTuple<int, int> ToLocal(int frameRow, int frameColumn);
```

Converts a frame cell into coordinates local to this region.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frameRow` | `int` | Row in frame coordinates. |
| `frameColumn` | `int` | Column in frame coordinates. |

**Returns** `ValueTuple<T1, T2>`&lt;`int`, `int`&gt; — The same cell as a row and column inside the region.

### `Write(int, int, string, IArlecchinoColor)` {#write-int-int-string-iarlecchinocolor}

```csharp
public void Write(int row, int column, string text, IArlecchinoColor style);
```

Writes text in region coordinates, clipped to the region. A negative column starts the text off the left edge and shows what fits.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row inside the region. |
| `column` | `int` | Column inside the region. |
| `text` | `string` | Text to draw. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the text. |

### `WriteLine(int, string, IArlecchinoColor, Align)` {#writeline-int-string-iarlecchinocolor-align}

```csharp
public void WriteLine(int row, string text, IArlecchinoColor style, Align align = Left);
```

Writes a whole line, aligned inside the region and clipped to its width.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row inside the region. |
| `text` | `string` | Text to draw. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the text. |
| `align` | [`Align`](../arlecchino.rendering/Align.md) | Horizontal alignment inside the region. |

