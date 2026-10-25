---
title: "EntryRow"
sidebar_label: "EntryRow"
---

# EntryRow class

**Namespace:** `Arlecchino.Widgets.Text` &middot; **Assembly:** `Arlecchino`

A line being typed into, drawn on one row of the screen: a filter, a search, a field an application draws for itself. Text longer than the room it is given scrolls, so the caret is always on the screen.

```csharp
public static class EntryRow
```

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion, int, int, int, ITextEntry, EntryLook)`](#draw-surfaceregion-int-int-int-itextentry-entrylook) | Draws the line, scrolled to keep the caret in view. |
| [`Draw(SurfaceRegion, int, int, int, string, int, ValueTuple<int, int>, EntryLook)`](#draw-surfaceregion-int-int-int-string-int-valuetuple-int-int-entrylook) | Draws a line that is written as something other than itself, which is what a secret comes to: the dots, the caret and the selection are all counted in what is shown rather than in what was typed. |

## Methods in detail

### `Draw(SurfaceRegion, int, int, int, ITextEntry, EntryLook)` {#draw-surfaceregion-int-int-int-itextentry-entrylook}

```csharp
public static int Draw(
    SurfaceRegion region,
    int row,
    int column,
    int width,
    ITextEntry entry,
    EntryLook look);
```

Draws the line, scrolled to keep the caret in view.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | The region to draw on. |
| `row` | `int` | Which row of it. |
| `column` | `int` | Which column the text starts at. |
| `width` | `int` | How many columns it is drawn in, the caret included. |
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `look` | [`EntryLook`](../arlecchino.widgets.text/EntryLook.md) | The colors to write it in. |

**Returns** `int` — How many columns were written.

### `Draw(SurfaceRegion, int, int, int, string, int, ValueTuple<int, int>, EntryLook)` {#draw-surfaceregion-int-int-int-string-int-valuetuple-int-int-entrylook}

```csharp
public static int Draw(
    SurfaceRegion region,
    int row,
    int column,
    int width,
    string text,
    int caret,
    ValueTuple<int, int> selection,
    EntryLook look);
```

Draws a line that is written as something other than itself, which is what a secret comes to: the dots, the caret and the selection are all counted in what is shown rather than in what was typed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | The region to draw on. |
| `row` | `int` | Which row of it. |
| `column` | `int` | Which column the text starts at. |
| `width` | `int` | How many columns it is drawn in, the caret included. |
| `text` | `string` | What to write. |
| `caret` | `int` | Where the caret is in it. |
| `selection` | `ValueTuple<T1, T2>`&lt;`int`, `int`&gt; | Where the selection starts and ends in it. |
| `look` | [`EntryLook`](../arlecchino.widgets.text/EntryLook.md) | The colors to write it in. |

**Returns** `int` — How many columns were written.

