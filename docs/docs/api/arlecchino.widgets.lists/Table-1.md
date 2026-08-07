---
title: "Table<T>"
sidebar_label: "Table<T>"
---

# Table&lt;T&gt; class

**Namespace:** `Arlecchino.Widgets.Lists` &middot; **Assembly:** `Arlecchino`

Rows in aligned columns, with a heading and optional sorting. Selection and scrolling are a list box underneath, so a table behaves exactly like a list that happens to draw more per row. Sorting reorders a copy, leaving whatever was assigned to [`Table.Rows`](../arlecchino.widgets.lists/Table-1.md#rows) untouched.

```csharp
public sealed class Table<T> : IArlecchinoInteractiveWidget, IArlecchinoWidget, IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`Table(ArlecchinoKeymap)`](#table-arlecchinokeymap) | Creates the table. |

## Properties

| Member | Summary |
|---|---|
| [`Columns`](#columns) | The columns, left to right. |
| [`IsFocused`](#isfocused) | Whether the table has focus, which decides how strongly the selection is drawn. |
| [`ItemStyle`](#itemstyle) | Colors a whole row. Ignored for the selected one. |
| [`OnActivate`](#onactivate) | What confirming a row does. Returning a route navigates. |
| [`Rows`](#rows) | What to show. Assigning re-applies the current sort. |
| [`Selected`](#selected) | Index of the selected row within the sorted order, not within what was assigned. |
| [`SelectedRow`](#selectedrow) | The selected row, or the type's default when the table is empty. |
| [`SortedBy`](#sortedby) | Index of the column being sorted by, or `-1` when the rows are in their original order. |
| [`SortsDescending`](#sortsdescending) | Which way the sort runs. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Works out the column widths for the space available, then draws the heading on the first row and the rows below it. The table fills whatever it is given, so nothing is left underneath it. |
| [`Handle(KeyPress)`](#handle-keypress) | Moves the selection or confirms it. Sorting is not bound to a key; call [`Table.SortBy`](../arlecchino.widgets.lists/Table-1.md#sortby-int). |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Scrolls and selects. Clicks on the heading are not routed here. |
| [`SortBy(int)`](#sortby-int) | Sorts by a column, or flips the direction when it is already the one being sorted by. Columns without a comparison, and indexes outside the table, are ignored. |

## Constructors in detail

### `Table(ArlecchinoKeymap)` {#table-arlecchinokeymap}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public Table(ArlecchinoKeymap keymap);
```

Creates the table.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Keys to obey, so the table follows the application's bindings. |

## Properties in detail

### `Columns` {#columns}

```csharp
public IReadOnlyList<TableColumn<T>> Columns { get; init; }
```

The columns, left to right.

**Type** `IReadOnlyList<T>`&lt;[`TableColumn`](../arlecchino.widgets.lists/TableColumn-1.md)&lt;`T`&gt;&gt;

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether the table has focus, which decides how strongly the selection is drawn.

**Type** `bool`

### `ItemStyle` {#itemstyle}

```csharp
public Func<T, IArlecchinoColor> ItemStyle { get; init; }
```

Colors a whole row. Ignored for the selected one.

**Type** `Func<T, TResult>`&lt;`T`, [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)&gt;

### `OnActivate` {#onactivate}

```csharp
public Func<T, ViewRoute> OnActivate { get; init; }
```

What confirming a row does. Returning a route navigates.

**Type** `Func<T, TResult>`&lt;`T`, [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

### `Rows` {#rows}

```csharp
public IReadOnlyList<T> Rows { get; set; }
```

What to show. Assigning re-applies the current sort.

**Type** `IReadOnlyList<T>`&lt;`T`&gt;

### `Selected` {#selected}

```csharp
public int Selected { get; set; }
```

Index of the selected row within the sorted order, not within what was assigned.

**Type** `int`

### `SelectedRow` {#selectedrow}

```csharp
public T SelectedRow { get; }
```

The selected row, or the type's default when the table is empty.

**Type** `T`

### `SortedBy` {#sortedby}

```csharp
public int SortedBy { get; }
```

Index of the column being sorted by, or `-1` when the rows are in their original order.

**Type** `int`

### `SortsDescending` {#sortsdescending}

```csharp
public bool SortsDescending { get; }
```

Which way the sort runs.

**Type** `bool`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Works out the column widths for the space available, then draws the heading on the first row and the rows below it. The table fills whatever it is given, so nothing is left underneath it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw, heading included. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region: the table uses every row it is handed.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public FocusResult Handle(KeyPress key);
```

Moves the selection or confirms it. Sorting is not bound to a key; call [`Table.SortBy`](../arlecchino.widgets.lists/Table-1.md#sortby-int).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the key.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Scrolls and selects. Clicks on the heading are not routed here.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the event.

### `SortBy(int)` {#sortby-int}

```csharp
public void SortBy(int column);
```

Sorts by a column, or flips the direction when it is already the one being sorted by. Columns without a comparison, and indexes outside the table, are ignored.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `column` | `int` | Index of the column to sort by. |

