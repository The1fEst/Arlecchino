---
title: "MarkdownView"
sidebar_label: "MarkdownView"
---

# MarkdownView class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

A Markdown document to read: headings, lists, quotes, code blocks, tables and links drawn as what they are. It wraps to the width it is given and scrolls the way [`TextView`](../arlecchino.widgets.readouts/TextView.md) does.

```csharp
public sealed class MarkdownView :
    IArlecchinoInteractiveWidget,
    IArlecchinoWidget,
    IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`MarkdownView(ArlecchinoKeymap)`](#markdownview-arlecchinokeymap) | Creates the view. |

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Whether the view has focus. Only a focused view answers keys. |
| [`LineCount`](#linecount) | How many rows the document takes once laid out for the last width it was drawn at. |
| [`Offset`](#offset) | First row shown. |
| [`PictureRows`](#picturerows) | How many rows a picture may take. A picture is drawn no larger than it is, and no taller than this, so that what surrounds it is still on the screen with it. |
| [`Pictures`](#pictures) | The picture at an address the document names, which the application reads and holds. It is asked while a frame is drawn, so it answers at once, with `null` for a picture that is not there. |
| [`ShowScrollBar`](#showscrollbar) | Whether a scroll bar is drawn when the document does not fit. |
| [`Styles`](#styles) | What each part of it is written in. |
| [`Text`](#text) | The document, as it is written in the file. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Lays the document out for the region and draws the part that fits. The view fills whatever it is given, so nothing is left underneath it. |
| [`Handle(KeyPress)`](#handle-keypress) | Scrolls the document. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Scrolls with the wheel while the pointer is over the document. |

## Constructors in detail

### `MarkdownView(ArlecchinoKeymap)` {#markdownview-arlecchinokeymap}

```csharp
public MarkdownView(ArlecchinoKeymap keymap);
```

Creates the view.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Keys to obey, so scrolling follows the application's bindings. |

## Properties in detail

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether the view has focus. Only a focused view answers keys.

**Type** `bool`

### `LineCount` {#linecount}

```csharp
public int LineCount { get; }
```

How many rows the document takes once laid out for the last width it was drawn at.

**Type** `int`

### `Offset` {#offset}

```csharp
public int Offset { get; set; }
```

First row shown.

**Type** `int`

### `PictureRows` {#picturerows}

```csharp
public int PictureRows { get; set; }
```

How many rows a picture may take. A picture is drawn no larger than it is, and no taller than this, so that what surrounds it is still on the screen with it.

**Type** `int`

### `Pictures` {#pictures}

```csharp
public Func<string, Picture?>? Pictures { get; set; }
```

The picture at an address the document names, which the application reads and holds. It is asked while a frame is drawn, so it answers at once, with `null` for a picture that is not there.

**Type** `Func<T, TResult>`&lt;`string`, [`Picture`](../arlecchino.widgets.pictures/Picture.md)&gt;

### `ShowScrollBar` {#showscrollbar}

```csharp
public bool ShowScrollBar { get; init; }
```

Whether a scroll bar is drawn when the document does not fit.

**Type** `bool`

### `Styles` {#styles}

```csharp
public MarkdownStyles Styles { get; set; }
```

What each part of it is written in.

**Type** [`MarkdownStyles`](../arlecchino.widgets.readouts/MarkdownStyles.md)

### `Text` {#text}

```csharp
public string Text { get; set; }
```

The document, as it is written in the file.

**Type** `string`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Lays the document out for the region and draws the part that fits. The view fills whatever it is given, so nothing is left underneath it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region: the view uses every row it is handed.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public FocusResult Handle(KeyPress key);
```

Scrolls the document.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — Whether the view took it.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Scrolls with the wheel while the pointer is over the document.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — Whether the view took it.

