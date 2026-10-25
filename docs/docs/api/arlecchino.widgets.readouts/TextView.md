---
title: "TextView"
sidebar_label: "TextView"
---

# TextView class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

A block of text to read: wrapped to the width it is given, scrolled with the movement keys and the wheel. It is re-wrapped whenever the width changes, so resizing the terminal reflows it.

```csharp
public sealed class TextView : IArlecchinoInteractiveWidget, IArlecchinoWidget, IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`TextView(ArlecchinoKeymap)`](#textview-arlecchinokeymap) | Creates the view. |

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Whether the view has focus. Only a focused view answers keys. |
| [`LineCount`](#linecount) | How many lines the text takes once wrapped to the last width it was drawn at. |
| [`Offset`](#offset) | First wrapped line shown. |
| [`ShowScrollBar`](#showscrollbar) | Whether a scroll bar is drawn when the text does not fit. |
| [`Style`](#style) | Color of the text. The theme's default when left alone. |
| [`Text`](#text) | The text shown. Line breaks are kept; long lines wrap on spaces. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Wraps the text to the region and draws the part that fits. The view fills whatever it is given, so nothing is left underneath it. |
| [`Handle(KeyPress)`](#handle-keypress) | Scrolls the text. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Scrolls with the wheel while the pointer is over the text. |

## Constructors in detail

### `TextView(ArlecchinoKeymap)` {#textview-arlecchinokeymap}

```csharp
public TextView(ArlecchinoKeymap keymap);
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

How many lines the text takes once wrapped to the last width it was drawn at.

**Type** `int`

### `Offset` {#offset}

```csharp
public int Offset { get; set; }
```

First wrapped line shown.

**Type** `int`

### `ShowScrollBar` {#showscrollbar}

```csharp
public bool ShowScrollBar { get; init; }
```

Whether a scroll bar is drawn when the text does not fit.

**Type** `bool`

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; set; }
```

Color of the text. The theme's default when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Text` {#text}

```csharp
public string Text { get; set; }
```

The text shown. Line breaks are kept; long lines wrap on spaces.

**Type** `string`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Wraps the text to the region and draws the part that fits. The view fills whatever it is given, so nothing is left underneath it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region: the view uses every row it is handed.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public FocusResult Handle(KeyPress key);
```

Scrolls the text.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — Whether the view took it.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Scrolls with the wheel while the pointer is over the text.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — Whether the view took it.

