---
title: Surface
sidebar_label: Surface
---

# Surface class

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

The drawing target: a grid of cells, each holding one symbol and one style, serialized into a single write per frame. Needs nothing but an [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md), so it works outside a hosted application too.

```csharp
public class Surface
```

## Constructors

| Member | Summary |
|---|---|
| [`Surface(IArlecchinoTerminal)`](#surface-iarlecchinoterminal) | Creates a surface that draws to a terminal. |

## Properties

| Member | Summary |
|---|---|
| [`Content`](#content) | Where a view draws: the frame minus the configured padding, or the room a layout left it while one is drawing the view inside itself. A view asks for this and gets what it has been given, which is what lets a layout be added to an application without a single view knowing. |
| [`Frame`](#frame) | The whole frame as a region. |
| [`FrameHeight`](#frameheight) | Height of the current frame in rows. |
| [`FrameWidth`](#framewidth) | Width of the current frame in cells. |
| [`HorizontalPadding`](#horizontalpadding) | Cells kept free on the left and right by the flow calls. |
| [`VerticalPadding`](#verticalpadding) | Rows kept free above and below by the flow calls. |

## Methods

| Member | Summary |
|---|---|
| [`AppendLine(string, IArlecchinoColor, Align, Margin)`](#appendline-string-iarlecchinocolor-align-margin) | Writes one line at the flow cursor and moves it down. Stops silently once the frame is full, so a view never has to bound its own output. |
| [`Build()`](#build) | Sends the composed frame to the terminal, writing only what changed since the last one — an idle frame writes nothing at all. The first frame, a resize and a fixed size send everything. |
| [`Clip(SurfaceRegion)`](#clip-surfaceregion) | Confines every write to a rectangle until the returned scope is disposed, whatever coordinates the writing code uses. This is what makes a scrolling pane possible: the content is drawn at an offset that reaches outside the pane, and the parts that fall outside are dropped instead of landing on a neighbour. Scopes nest, and the innermost one wins — a clip inside a clip is their intersection. |
| [`FillLine()`](#fillline) | Draws a rule across the content width at the flow cursor. |
| [`FillLineAt(int, IArlecchinoColor)`](#filllineat-int-iarlecchinocolor) | Draws a rule across the content width on a given row. |
| [`ForgetPreviousFrame()`](#forgetpreviousframe) | Drops the memory of the last frame, so the next [`Surface.Build`](../arlecchino.rendering/Surface.md#build) sends the whole screen instead of the difference. Use it after something else wrote to the terminal. |
| [`ListWindow()`](#listwindow) | How many rows a scrolling list may use: what is left of the frame minus room for the chrome, never fewer than four. |
| [`Passthrough(int, int, string, string)`](#passthrough-int-int-string-string) | Hands the terminal something the cell grid cannot express — an image in one of the graphics protocols, most of all — to be written verbatim at a cell, after everything the frame drew. It goes out last on purpose: the cells are written first, so whatever was under or around the payload last time is repainted before it lands. Repainting the cells is not enough to remove it, though, which is what `undraw` is for. A payload that was handed over last frame and is not handed over this one — the widget moved, or shrank, or is not on screen at all any more — has its undraw written at the place it used to be, and written **first**, before a single cell of the new frame. An undraw paints over what it removes, so whatever the frame draws lands on top of it; the other way round it would erase the frame instead of the picture. A frame that undraws anything is written whole rather than diffed, since the cells the undraw painted over have to be put back whether they changed or not. Whoever hands over pixels says how to take them back, because only they know: kitty deletes an image by number, a sixel has to be painted over. Nothing is re-sent while it stays the same. A frame is only composed when something asked for one, so a picture that has not changed costs nothing between frames — but a payload measured in kilobytes is still worth handing over only when it has to be. |
| [`SetFixedSize(int, int)`](#setfixedsize-int-int) | Pins the frame size instead of asking the terminal, which is what makes headless rendering possible. A fixed-size surface always sends whole frames. |
| [`SkipLine()`](#skipline) | Leaves a blank line at the flow cursor. |
| [`StartFrame()`](#startframe) | Begins a frame: reads the terminal size, reallocates if it changed, clears every cell and skips the vertical padding. Nothing reaches the terminal until [`Surface.Build`](../arlecchino.rendering/Surface.md#build). |
| [`WriteAt(int, int, string, IArlecchinoColor)`](#writeat-int-int-string-iarlecchinocolor) | Writes at an exact cell, clipped to the frame. A wide symbol takes two cells; writing over either half clears the other, and one that would be split by the right edge is dropped. |
| [`WriteBlock(IReadOnlyList<string>, IArlecchinoColor, Align, Margin)`](#writeblock-ireadonlylist-string-iarlecchinocolor-align-margin) | Places a block of prepared lines as a unit, ignoring the flow cursor. Vertical alignment flags work here, which is how the hints box is anchored to a corner. |
| [`WriteLineAt(int, string, IArlecchinoColor)`](#writelineat-int-string-iarlecchinocolor) | Restyles a whole row and writes text at the horizontal padding, ignoring the flow cursor. |
| [`WriteTableRow(string[], int[], IArlecchinoColor, string)`](#writetablerow-string-int-iarlecchinocolor-string) | Writes a row of padded columns at the flow cursor. |

## Constructors in detail

### `Surface(IArlecchinoTerminal)` {#surface-iarlecchinoterminal}

```csharp
public Surface(IArlecchinoTerminal terminal);
```

Creates a surface that draws to a terminal.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `terminal` | [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md) | Where composed frames are written. |

## Properties in detail

### `Content` {#content}

```csharp
public SurfaceRegion Content { get; }
```

Where a view draws: the frame minus the configured padding, or the room a layout left it while one is drawing the view inside itself. A view asks for this and gets what it has been given, which is what lets a layout be added to an application without a single view knowing.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Frame` {#frame}

```csharp
public SurfaceRegion Frame { get; }
```

The whole frame as a region.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `FrameHeight` {#frameheight}

```csharp
public int FrameHeight { get; }
```

Height of the current frame in rows.

**Type** `int`

### `FrameWidth` {#framewidth}

```csharp
public int FrameWidth { get; }
```

Width of the current frame in cells.

**Type** `int`

### `HorizontalPadding` {#horizontalpadding}

```csharp
public int HorizontalPadding { get; set; }
```

Cells kept free on the left and right by the flow calls.

**Type** `int`

### `VerticalPadding` {#verticalpadding}

```csharp
public int VerticalPadding { get; set; }
```

Rows kept free above and below by the flow calls.

**Type** `int`

## Methods in detail

### `AppendLine(string, IArlecchinoColor, Align, Margin)` {#appendline-string-iarlecchinocolor-align-margin}

```csharp
public void AppendLine(string line, IArlecchinoColor style, Align align, Margin margin);
```

Writes one line at the flow cursor and moves it down. Stops silently once the frame is full, so a view never has to bound its own output.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `line` | `string` | Text to write. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the line; the default role when omitted. |
| `align` | [`Align`](../arlecchino.rendering/Align.md) | Horizontal alignment inside the content width. |
| `margin` | [`Margin`](../arlecchino.rendering/Margin.md) | Extra space around the line. |

### `Build()` {#build}

```csharp
public void Build();
```

Sends the composed frame to the terminal, writing only what changed since the last one — an idle frame writes nothing at all. The first frame, a resize and a fixed size send everything.

### `Clip(SurfaceRegion)` {#clip-surfaceregion}

```csharp
public IDisposable Clip(SurfaceRegion region);
```

Confines every write to a rectangle until the returned scope is disposed, whatever coordinates the writing code uses. This is what makes a scrolling pane possible: the content is drawn at an offset that reaches outside the pane, and the parts that fall outside are dropped instead of landing on a neighbour. Scopes nest, and the innermost one wins — a clip inside a clip is their intersection.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | The only part of the frame writes may reach. |

**Returns** `IDisposable` — Dispose it to go back to the clip that was in force before.

### `FillLine()` {#fillline}

```csharp
public void FillLine();
```

Draws a rule across the content width at the flow cursor.

### `FillLineAt(int, IArlecchinoColor)` {#filllineat-int-iarlecchinocolor}

```csharp
public void FillLineAt(int row, IArlecchinoColor? style = null);
```

Draws a rule across the content width on a given row.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row in frame coordinates. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the rule; the default role when omitted. |

### `ForgetPreviousFrame()` {#forgetpreviousframe}

```csharp
public void ForgetPreviousFrame();
```

Drops the memory of the last frame, so the next [`Surface.Build`](../arlecchino.rendering/Surface.md#build) sends the whole screen instead of the difference. Use it after something else wrote to the terminal.

### `ListWindow()` {#listwindow}

```csharp
public int ListWindow();
```

How many rows a scrolling list may use: what is left of the frame minus room for the chrome, never fewer than four.

**Returns** `int` — Rows available for list content.

### `Passthrough(int, int, string, string)` {#passthrough-int-int-string-string}

```csharp
public void Passthrough(int row, int column, string payload, string undraw = "");
```

Hands the terminal something the cell grid cannot express — an image in one of the graphics protocols, most of all — to be written verbatim at a cell, after everything the frame drew. It goes out last on purpose: the cells are written first, so whatever was under or around the payload last time is repainted before it lands. Repainting the cells is not enough to remove it, though, which is what `undraw` is for. A payload that was handed over last frame and is not handed over this one — the widget moved, or shrank, or is not on screen at all any more — has its undraw written at the place it used to be, and written **first**, before a single cell of the new frame. An undraw paints over what it removes, so whatever the frame draws lands on top of it; the other way round it would erase the frame instead of the picture. A frame that undraws anything is written whole rather than diffed, since the cells the undraw painted over have to be put back whether they changed or not. Whoever hands over pixels says how to take them back, because only they know: kitty deletes an image by number, a sixel has to be painted over. Nothing is re-sent while it stays the same. A frame is only composed when something asked for one, so a picture that has not changed costs nothing between frames — but a payload measured in kilobytes is still worth handing over only when it has to be.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row of the cell it starts at, counted from the top of the frame. |
| `column` | `int` | Column of that cell. |
| `payload` | `string` | The bytes to write, escapes and all. |
| `undraw` | `string` | What removes it again, written where the payload was. Empty when nothing can: a sixel on a terminal that will not say what colour is behind its text has to be left where it is. |

### `SetFixedSize(int, int)` {#setfixedsize-int-int}

```csharp
public void SetFixedSize(int width, int height);
```

Pins the frame size instead of asking the terminal, which is what makes headless rendering possible. A fixed-size surface always sends whole frames.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `width` | `int` | Width in cells. |
| `height` | `int` | Height in rows. |

### `SkipLine()` {#skipline}

```csharp
public void SkipLine();
```

Leaves a blank line at the flow cursor.

### `StartFrame()` {#startframe}

```csharp
public void StartFrame();
```

Begins a frame: reads the terminal size, reallocates if it changed, clears every cell and skips the vertical padding. Nothing reaches the terminal until [`Surface.Build`](../arlecchino.rendering/Surface.md#build).

### `WriteAt(int, int, string, IArlecchinoColor)` {#writeat-int-int-string-iarlecchinocolor}

```csharp
public void WriteAt(int row, int column, string text, IArlecchinoColor style);
```

Writes at an exact cell, clipped to the frame. A wide symbol takes two cells; writing over either half clears the other, and one that would be split by the right edge is dropped.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row in frame coordinates. |
| `column` | `int` | Column in frame coordinates. |
| `text` | `string` | Text to write. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the text. |

### `WriteBlock(IReadOnlyList<string>, IArlecchinoColor, Align, Margin)` {#writeblock-ireadonlylist-string-iarlecchinocolor-align-margin}

```csharp
public void WriteBlock(
    IReadOnlyList<string> lines,
    IArlecchinoColor style,
    Align align,
    Margin margin);
```

Places a block of prepared lines as a unit, ignoring the flow cursor. Vertical alignment flags work here, which is how the hints box is anchored to a corner.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `lines` | `IReadOnlyList<T>`&lt;`string`&gt; | Lines of the block. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the block. |
| `align` | [`Align`](../arlecchino.rendering/Align.md) | Horizontal and vertical alignment against the frame. |
| `margin` | [`Margin`](../arlecchino.rendering/Margin.md) | Space kept free from the edges it is aligned to. |

### `WriteLineAt(int, string, IArlecchinoColor)` {#writelineat-int-string-iarlecchinocolor}

```csharp
public void WriteLineAt(int row, string line, IArlecchinoColor? style = null);
```

Restyles a whole row and writes text at the horizontal padding, ignoring the flow cursor.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row in frame coordinates. |
| `line` | `string` | Text to write. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the row; the default role when omitted. |

### `WriteTableRow(string[], int[], IArlecchinoColor, string)` {#writetablerow-string-int-iarlecchinocolor-string}

```csharp
public void WriteTableRow(
    string[] strings,
    int[] widths,
    IArlecchinoColor style,
    string prefix = "");
```

Writes a row of padded columns at the flow cursor.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `strings` | `string`\[\] | Cell texts, in column order. |
| `widths` | `int`\[\] | Column widths: a positive width right-aligns the cell, a negative one left-aligns it. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | Style for the row. |
| `prefix` | `string` | Text placed before the first column, such as a marker. |

