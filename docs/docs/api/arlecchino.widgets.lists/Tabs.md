---
title: "Tabs"
sidebar_label: "Tabs"
---

# Tabs class

**Namespace:** `Arlecchino.Widgets.Lists` &middot; **Assembly:** `Arlecchino`

A row of labels where one is current. The widget only tracks which that is; what each tab shows is left to the view, which draws whatever fits the selection.

```csharp
public sealed class Tabs : IArlecchinoInteractiveWidget, IArlecchinoWidget, IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`Tabs(ArlecchinoKeymap)`](#tabs-arlecchinokeymap) | Creates the strip. |

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Whether the strip has focus, which decides how strongly the current tab is drawn. |
| [`OnSelected`](#onselected) | Called when the selection actually changes, not on every attempt to move. |
| [`Selected`](#selected) | Index of the current tab. |
| [`Titles`](#titles) | The labels, as delegates so a tab can show a count or a marker that changes. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the labels side by side and remembers where each starts, which is what lets a click be resolved to a tab. Returns the rows below the strip, which is where the current tab's content belongs. |
| [`Handle(KeyPress)`](#handle-keypress) | Switches tabs with the horizontal arrows, leaving everything else alone. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Switches to the tab that was clicked. A click in the gap between labels lands on the tab to its left, so the strip has no dead columns. |
| [`Select(int)`](#select-int) | Switches tabs, ignoring indexes outside the strip and moves that change nothing. |

## Constructors in detail

### `Tabs(ArlecchinoKeymap)` {#tabs-arlecchinokeymap}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public Tabs(ArlecchinoKeymap keymap);
```

Creates the strip.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Keys to obey, so the strip follows the application's bindings. |

## Properties in detail

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether the strip has focus, which decides how strongly the current tab is drawn.

**Type** `bool`

### `OnSelected` {#onselected}

```csharp
public Action<int>? OnSelected { get; init; }
```

Called when the selection actually changes, not on every attempt to move.

**Type** `Action<T>`&lt;`int`&gt;

### `Selected` {#selected}

```csharp
public int Selected { get; }
```

Index of the current tab.

**Type** `int`

### `Titles` {#titles}

```csharp
public IReadOnlyList<Func<string>> Titles { get; init; }
```

The labels, as delegates so a tab can show a count or a marker that changes.

**Type** `IReadOnlyList<T>`&lt;`Func<TResult>`&lt;`string`&gt;&gt;

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the labels side by side and remembers where each starts, which is what lets a click be resolved to a tab. Returns the rows below the strip, which is where the current tab's content belongs.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw; only its first row is used. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the strip.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public FocusResult Handle(KeyPress key);
```

Switches tabs with the horizontal arrows, leaving everything else alone.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the key.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Switches to the tab that was clicked. A click in the gap between labels lands on the tab to its left, so the strip has no dead columns.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the event.

### `Select(int)` {#select-int}

```csharp
public void Select(int index);
```

Switches tabs, ignoring indexes outside the strip and moves that change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Tab to switch to. |

