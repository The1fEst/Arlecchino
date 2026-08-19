---
title: "ListBox<T>"
sidebar_label: "ListBox<T>"
---

# ListBox&lt;T&gt; class

**Namespace:** `Arlecchino.Widgets.Lists` &middot; **Assembly:** `Arlecchino`

A scrolling list of items, one per row. It keeps only the selected index, never a copy of the items, so replacing [`ListBox.Items`](../arlecchino.widgets.lists/ListBox-1.md#items) between frames is a normal thing to do.

```csharp
public sealed class ListBox<T> :
    IArlecchinoInteractiveWidget,
    IArlecchinoWidget,
    IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`ListBox(ArlecchinoKeymap)`](#listbox-arlecchinokeymap) | Creates the list. |

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Whether the list has focus, which decides how strongly the selection is drawn. |
| [`ItemStyle`](#itemstyle) | Colors an item. Ignored for the selected row, which has to stand out. |
| [`Items`](#items) | What to show. Replacing this pulls the selection back into range on the next frame. |
| [`OnActivate`](#onactivate) | What confirming an item does. Returning a route navigates; without this the list simply reports the key as handled. |
| [`PaintRow`](#paintrow) | Draws a row itself, for a list whose rows are not one color. It is given one row to fill and told whether the cursor is on it, and [`ListBox.Render`](../arlecchino.widgets.lists/ListBox-1.md#render) goes unread while it is set. |
| [`Render`](#render) | Turns an item into its row of text. Longer text is truncated by column, not by character. |
| [`SelectedIndex`](#selectedindex) | Index of the selected row. |
| [`SelectedItem`](#selecteditem) | The selected item, or the type's default when the list is empty. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the rows around the selection and remembers where they landed, which is what lets clicks and wheel events be resolved afterward. The list fills whatever it is given, so nothing is left underneath it. |
| [`Handle(KeyPress)`](#handle-keypress) | Moves the selection or confirms it. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Scrolls with the wheel and selects with a click. Clicking the already selected row confirms it, so a double click reads as select-then-activate without the widget timing anything. |

## Constructors in detail

### `ListBox(ArlecchinoKeymap)` {#listbox-arlecchinokeymap}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public ListBox(ArlecchinoKeymap keymap);
```

Creates the list.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Keys to obey, so the list follows the application's bindings. |

## Properties in detail

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether the list has focus, which decides how strongly the selection is drawn.

**Type** `bool`

### `ItemStyle` {#itemstyle}

```csharp
public Func<T, IArlecchinoColor> ItemStyle { get; set; }
```

Colors an item. Ignored for the selected row, which has to stand out.

**Type** `Func<T, TResult>`&lt;`T`, [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)&gt;

### `Items` {#items}

```csharp
public IReadOnlyList<T> Items { get; set; }
```

What to show. Replacing this pulls the selection back into range on the next frame.

**Type** `IReadOnlyList<T>`&lt;`T`&gt;

### `OnActivate` {#onactivate}

```csharp
public Func<T, ViewRoute> OnActivate { get; init; }
```

What confirming an item does. Returning a route navigates; without this the list simply reports the key as handled.

**Type** `Func<T, TResult>`&lt;`T`, [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

### `PaintRow` {#paintrow}

```csharp
public Action<SurfaceRegion, T, bool> PaintRow { get; init; }
```

Draws a row itself, for a list whose rows are not one color. It is given one row to fill and told whether the cursor is on it, and [`ListBox.Render`](../arlecchino.widgets.lists/ListBox-1.md#render) goes unread while it is set.

**Type** `Action<T1, T2, T3>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md), `T`, `bool`&gt;

### `Render` {#render}

```csharp
public Func<T, string> Render { get; init; }
```

Turns an item into its row of text. Longer text is truncated by column, not by character.

**Type** `Func<T, TResult>`&lt;`T`, `string`&gt;

### `SelectedIndex` {#selectedindex}

```csharp
public int SelectedIndex { get; set; }
```

Index of the selected row.

**Type** `int`

### `SelectedItem` {#selecteditem}

```csharp
public T SelectedItem { get; }
```

The selected item, or the type's default when the list is empty.

**Type** `T`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the rows around the selection and remembers where they landed, which is what lets clicks and wheel events be resolved afterward. The list fills whatever it is given, so nothing is left underneath it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region: the list uses every row it is handed.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public FocusResult Handle(KeyPress key);
```

Moves the selection or confirms it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the key, including a route when confirming navigates.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Scrolls with the wheel and selects with a click. Clicking the already selected row confirms it, so a double click reads as select-then-activate without the widget timing anything.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the event, including a route when a row was activated.

