---
title: "ScrollWindow"
sidebar_label: "ScrollWindow"
---

# ScrollWindow struct

**Namespace:** `Arlecchino.Widgets.Lists` &middot; **Assembly:** `Arlecchino`

The slice of a long list that fits on screen. Every scrolling widget works this out the same way, so the arithmetic lives here rather than in each of them.

```csharp
public readonly struct ScrollWindow : IEquatable<ScrollWindow>
```

**Implements** `IEquatable<T>`&lt;[`ScrollWindow`](../arlecchino.widgets.lists/ScrollWindow.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`ScrollWindow(int, int)`](#scrollwindow-int-int) | The slice of a long list that fits on screen. Every scrolling widget works this out the same way, so the arithmetic lives here rather than in each of them. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many items are shown. |
| [`First`](#first) | Index of the first item shown. |
| [`Last`](#last) | Index of the last item shown. Reads as one before [`ScrollWindow.First`](../arlecchino.widgets.lists/ScrollWindow.md#first) when nothing fits. |

## Methods

| Member | Summary |
|---|---|
| [`Around(int, int, int)`](#around-int-int-int) | Places the window with the selection in the middle, sliding it back at the ends of the list, so the rows are always filled rather than trailing off into blanks. |
| [`Contains(int)`](#contains-int) | Whether an item is in view, which is what decides if it needs drawing. |
| [`Deconstruct(out int, out int)`](#deconstruct-out-int-out-int) |  |

## Constructors in detail

### `ScrollWindow(int, int)` {#scrollwindow-int-int}

```csharp
public ScrollWindow(int First, int Count);
```

The slice of a long list that fits on screen. Every scrolling widget works this out the same way, so the arithmetic lives here rather than in each of them.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `First` | `int` | Index of the first item shown. |
| `Count` | `int` | How many items are shown. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; init; }
```

How many items are shown.

**Type** `int`

### `First` {#first}

```csharp
public int First { get; init; }
```

Index of the first item shown.

**Type** `int`

### `Last` {#last}

```csharp
public int Last { get; }
```

Index of the last item shown. Reads as one before [`ScrollWindow.First`](../arlecchino.widgets.lists/ScrollWindow.md#first) when nothing fits.

**Type** `int`

## Methods in detail

### `Around(int, int, int)` {#around-int-int-int}

```csharp
public static ScrollWindow Around(int selected, int itemCount, int rows);
```

Places the window with the selection in the middle, sliding it back at the ends of the list, so the rows are always filled rather than trailing off into blanks.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `selected` | `int` | Index that has to stay visible. |
| `itemCount` | `int` | Length of the full list. |
| `rows` | `int` | How many rows there are to draw into. |

**Returns** [`ScrollWindow`](../arlecchino.widgets.lists/ScrollWindow.md) — The slice to draw; empty when there is nothing to show or nowhere to show it.

### `Contains(int)` {#contains-int}

```csharp
public bool Contains(int index);
```

Whether an item is in view, which is what decides if it needs drawing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Index in the full list. |

**Returns** `bool` — `true` when the item falls inside the window.

### `Deconstruct(out int, out int)` {#deconstruct-out-int-out-int}

```csharp
public void Deconstruct(out int First, out int Count);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `First` | `int` |  |
| `Count` | `int` |  |

