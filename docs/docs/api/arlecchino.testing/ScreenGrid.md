---
title: "ScreenGrid"
sidebar_label: "ScreenGrid"
---

# ScreenGrid class

**Namespace:** `Arlecchino.Testing` &middot; **Assembly:** `Arlecchino.Testing`

A terminal screen as the terminal itself would hold it: a grid of cells that output is applied to rather than collected in. Where [`FrameText`](../arlecchino.testing/FrameText.md) strips escapes out of what was written, this obeys them — a cursor jump moves the cursor, a style sticks to the cells that follow, a wide symbol takes two columns. That difference is the point. Frames are written as the difference from the last one, so what reaches the terminal is a handful of jumps and runs which say nothing on their own about what the screen holds afterward. Applying them here answers that, and makes the invariant worth asserting: a screen built from diffs is the screen a whole repaint would have drawn.

```csharp
public sealed class ScreenGrid
```

## Constructors

| Member | Summary |
|---|---|
| [`ScreenGrid(int, int)`](#screengrid-int-int) | Creates a blank screen at a fixed size. |

## Properties

| Member | Summary |
|---|---|
| [`CursorColumn`](#cursorcolumn) | The column the cursor sits on, counted from the left. A symbol written into the last column while wrapping is on leaves the cursor one past the right edge, waiting to wrap. The next symbol goes to the row below, and a terminal asked where its cursor is answers the same way — tmux and kitty both do. With wrapping off they stop agreeing: the same symbol leaves the cursor in the last column for tmux and one past it for kitty. Nothing visible turns on it, since with nowhere to wrap to the next symbol lands in the last column either way; this follows tmux and says so. |
| [`CursorRow`](#cursorrow) | The row the cursor sits on, counted from the top. |
| [`Height`](#height) | Rows. |
| [`IsCursorVisible`](#iscursorvisible) | Whether the cursor was left visible. |
| [`Width`](#width) | Columns. |

## Methods

| Member | Summary |
|---|---|
| [`Apply(string)`](#apply-string) | Applies what was written to the terminal. Call it as often as there are frames — the screen carries over, which is what makes a run of diffed frames add up to a picture. |
| [`CellAt(int, int)`](#cellat-int-int) | The symbol standing on a cell, or an empty string for the second half of a wide one. |
| [`Line(int)`](#line-int) | One row as it reads, padded out to the full width. |
| [`Lines()`](#lines) | Every row as it reads. |
| [`Matches(ScreenGrid)`](#matches-screengrid) | Whether another screen holds the same symbols in the same styles. Text alone is the readable half of a frame, so a screen that matches on [`ScreenGrid.ToString`](../arlecchino.testing/ScreenGrid.md#tostring) can still differ in color. |
| [`Resize(int, int)`](#resize-int-int) | Resizes the screen, keeping what fits and dropping the rest, the way a terminal does. The cursor is pulled back inside. |
| [`StyleAt(int, int)`](#styleat-int-int) | The style sequence in force on a cell, empty where the style was reset. Compare it against [`TermColor.Ansi`](../arlecchino.rendering.colors/TermColor.md#ansi) to assert that something was drawn in the color it should be. |
| [`ToString()`](#tostring) | The whole screen as text, rows separated by line feeds. |

## Constructors in detail

### `ScreenGrid(int, int)` {#screengrid-int-int}

```csharp
public ScreenGrid(int width, int height);
```

Creates a blank screen at a fixed size.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `width` | `int` | Columns. |
| `height` | `int` | Rows. |

## Properties in detail

### `CursorColumn` {#cursorcolumn}

```csharp
public int CursorColumn { get; }
```

The column the cursor sits on, counted from the left. A symbol written into the last column while wrapping is on leaves the cursor one past the right edge, waiting to wrap. The next symbol goes to the row below, and a terminal asked where its cursor is answers the same way — tmux and kitty both do. With wrapping off they stop agreeing: the same symbol leaves the cursor in the last column for tmux and one past it for kitty. Nothing visible turns on it, since with nowhere to wrap to the next symbol lands in the last column either way; this follows tmux and says so.

**Type** `int`

### `CursorRow` {#cursorrow}

```csharp
public int CursorRow { get; }
```

The row the cursor sits on, counted from the top.

**Type** `int`

### `Height` {#height}

```csharp
public int Height { get; }
```

Rows.

**Type** `int`

### `IsCursorVisible` {#iscursorvisible}

```csharp
public bool IsCursorVisible { get; }
```

Whether the cursor was left visible.

**Type** `bool`

### `Width` {#width}

```csharp
public int Width { get; }
```

Columns.

**Type** `int`

## Methods in detail

### `Apply(string)` {#apply-string}

```csharp
public void Apply(string output);
```

Applies what was written to the terminal. Call it as often as there are frames — the screen carries over, which is what makes a run of diffed frames add up to a picture.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `output` | `string` | The bytes written, escapes and all. |

### `CellAt(int, int)` {#cellat-int-int}

```csharp
public string CellAt(int row, int column);
```

The symbol standing on a cell, or an empty string for the second half of a wide one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row, counted from the top. |
| `column` | `int` | Column, counted from the left. |

**Returns** `string` — The symbol.

### `Line(int)` {#line-int}

```csharp
public string Line(int row);
```

One row as it reads, padded out to the full width.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row, counted from the top. |

**Returns** `string` — The row as text.

### `Lines()` {#lines}

```csharp
public string[] Lines();
```

Every row as it reads.

**Returns** `string`\[\] — One string per row.

### `Matches(ScreenGrid)` {#matches-screengrid}

```csharp
public bool Matches(ScreenGrid other);
```

Whether another screen holds the same symbols in the same styles. Text alone is the readable half of a frame, so a screen that matches on [`ScreenGrid.ToString`](../arlecchino.testing/ScreenGrid.md#tostring) can still differ in color.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`ScreenGrid`](../arlecchino.testing/ScreenGrid.md) | The screen to compare against. |

**Returns** `bool` — `true` when both the symbols and the styles agree.

### `Resize(int, int)` {#resize-int-int}

```csharp
public void Resize(int width, int height);
```

Resizes the screen, keeping what fits and dropping the rest, the way a terminal does. The cursor is pulled back inside.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `width` | `int` | Columns. |
| `height` | `int` | Rows. |

### `StyleAt(int, int)` {#styleat-int-int}

```csharp
public string StyleAt(int row, int column);
```

The style sequence in force on a cell, empty where the style was reset. Compare it against [`TermColor.Ansi`](../arlecchino.rendering.colors/TermColor.md#ansi) to assert that something was drawn in the color it should be.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row, counted from the top. |
| `column` | `int` | Column, counted from the left. |

**Returns** `string` — The sequence.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

The whole screen as text, rows separated by line feeds.

**Returns** `string` — The screen as it reads.

