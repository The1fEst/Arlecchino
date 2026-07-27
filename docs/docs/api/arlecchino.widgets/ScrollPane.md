---
title: ScrollPane
sidebar_label: ScrollPane
---

# ScrollPane class

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

A window onto content taller than the space it has. Lists scroll themselves, but a block of text, a long form or a pane of anything at all does not — this is the widget for those: it draws the content shifted up by the offset, confines it to its own rectangle, and answers the movement keys and the wheel. The content is drawn by a delegate rather than owned, so whatever can paint a region can live in here, including other widgets.

```csharp
public sealed class ScrollPane : IArlecchinoInteractiveWidget, IArlecchinoWidget, IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`ScrollPane(ArlecchinoKeymap)`](#scrollpane-arlecchinokeymap) | Creates the pane. |

## Properties

| Member | Summary |
|---|---|
| [`Content`](#content) | Draws the content. The region it is handed is as tall as [`ScrollPane.ContentHeight`](../arlecchino.widgets/ScrollPane.md#contentheight) and moved up by the offset, so the delegate always writes at row zero for the first line and never has to know where the window is. |
| [`ContentHeight`](#contentheight) | How many rows the content occupies, asked once per frame. |
| [`IsFocused`](#isfocused) | Whether the pane has focus. Only a focused pane answers keys. |
| [`Offset`](#offset) | First content row shown, clamped to what there is on every frame. |
| [`ShowScrollBar`](#showscrollbar) | Whether a scroll bar is drawn down the last column when the content does not fit. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the visible slice of the content, and the scroll bar when one is needed. The pane is a window onto content taller than itself, so nothing is ever left underneath it. |
| [`Handle(ConsoleKeyInfo)`](#handle-consolekeyinfo) | Moves the window a row, a page, or to either end. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Scrolls with the wheel while the pointer is over the pane. |

## Constructors in detail

### `ScrollPane(ArlecchinoKeymap)` {#scrollpane-arlecchinokeymap}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public ScrollPane(ArlecchinoKeymap keymap);
```

Creates the pane.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Keys to obey, so the pane follows the application's bindings. |

## Properties in detail

### `Content` {#content}

```csharp
public Action<SurfaceRegion> Content { get; init; }
```

Draws the content. The region it is handed is as tall as [`ScrollPane.ContentHeight`](../arlecchino.widgets/ScrollPane.md#contentheight) and moved up by the offset, so the delegate always writes at row zero for the first line and never has to know where the window is.

**Type** `Action<T>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt;

### `ContentHeight` {#contentheight}

```csharp
public Func<int> ContentHeight { get; init; }
```

How many rows the content occupies, asked once per frame.

**Type** `Func<TResult>`&lt;`int`&gt;

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether the pane has focus. Only a focused pane answers keys.

**Type** `bool`

### `Offset` {#offset}

```csharp
public int Offset { get; set; }
```

First content row shown, clamped to what there is on every frame.

**Type** `int`

### `ShowScrollBar` {#showscrollbar}

```csharp
public bool ShowScrollBar { get; set; }
```

Whether a scroll bar is drawn down the last column when the content does not fit.

**Type** `bool`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the visible slice of the content, and the scroll bar when one is needed. The pane is a window onto content taller than itself, so nothing is ever left underneath it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region: the pane uses every row it is handed.

### `Handle(ConsoleKeyInfo)` {#handle-consolekeyinfo}

```csharp
public FocusResult Handle(ConsoleKeyInfo key);
```

Moves the window a row, a page, or to either end.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — Whether the pane took it.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Scrolls with the wheel while the pointer is over the pane.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — Whether the pane took it.

