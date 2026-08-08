---
title: Scrolling
sidebar_label: Scrolling
description: ScrollPane for content taller than its space, and the ScrollWindow and ScrollBar every scrolling widget shares.
---

# Scrolling

Lists scroll themselves. A block of text, a long form or a pane of anything else does not — and the
arithmetic for "which slice is on screen" is the same everywhere, so it lives in one place.

## ScrollPane

A window onto content taller than the space it has:

```csharp
private readonly ScrollPane _notes;
private readonly IReadOnlyList<string> _lines = [];

_notes = new ScrollPane(options.Keymap)
{
    ContentHeight = () => _lines.Count,
    Content = region =>
    {
        for (var row = 0; row < _lines.Count; row++)
        {
            region.WriteLine(row, _lines[row], Theme.Default);
        }
    },
};
```

Widgets are built in the view's constructor, so `options` is the `ArlecchinoOptions` the container
hands it, and `region` is the [region](layout.md) the view draws the widget into.

The delegate is handed a region as tall as `ContentHeight()` and **already moved up by the offset**, so
it always writes the first line at row zero and never has to know where the window is. Anything that
can paint a region fits inside, other widgets included.

| Member | Meaning |
|---|---|
| `ContentHeight` | How many rows the content wants |
| `Content` | Paints it into the region it is handed |
| `Offset` | The first visible row; clamped every frame |
| `ShowScrollBar` | Whether the bar may appear at all |
| `IsFocused` | Set by the [focus ring](focus.md) |

`↑↓` move a row, `PgUp`/`PgDn` a page, `Home`/`End` go to the ends, and the wheel scrolls while the
pointer is over the pane.

### Why it is safe

The content is drawn at an offset that reaches **outside** the pane on purpose.
[`Surface.Clip`](layout.md#clipping-a-whole-stretch-of-drawing) is what stops the parts that fall
outside from landing on a neighbor:

```csharp
using (region.Surface.Clip(region))
{
    Content(region with { Top = region.Top - offset, Height = contentHeight });
}
```

That is the pattern to copy when writing a widget that scrolls something of its own.

## ScrollWindow

The slice of a long list that fits on screen. Every scrolling widget works it out the same way, so the
arithmetic lives here rather than in each of them:

```csharp
var window = ScrollWindow.Around(selected, items.Count, rows);

for (var offset = 0; offset < window.Count; offset++)
{
    var item = items[window.First + offset];
    region.WriteLine(offset, Render(item), Style(item));
}
```

| Member | Meaning |
|---|---|
| `ScrollWindow.Around(selected, total, rows)` | The window that keeps the selection visible |
| `First` | Index of the first item shown |
| `Count` | How many are shown |
| `Last` | Index of the last; reads as one before `First` when nothing fits |

## ScrollBar

The bar down the side of a list that shows how much of it is on screen and where:

```csharp
if (ScrollBar.IsNeeded(items.Count, rows))
{
    ScrollBar.Draw(region, window.First, items.Count);
}
```

| Member | Meaning |
|---|---|
| `IsNeeded(total, rows)` | Whether a bar is needed — also whether a column has to be kept free for it |
| `Draw(region, first, total, style)` | Draws it down the last column of the region |

Drawn only when there is more than fits, so a short list keeps its full width. The thumb is at least
one cell tall however long the list is, and it only touches the ends when the list does — so "near the
end" never looks the same as "at the end".

`IsNeeded` is separate from `Draw` on purpose: a list has to reserve the column **before** it renders
its rows, or the bar would cover the last cell of every one of them. That is why
[`ListBox`](lists.md#scrolling) truncates one cell earlier when a bar is coming.

## What already uses them

| Widget | Uses |
|---|---|
| [`ListBox`](lists.md), [`Table`](table.md), [`Tree`](tree.md) | `ScrollWindow` and `ScrollBar` |
| [`TextView`](text-view.md) | A `ScrollPane` inside |
| [Choice and multi-choice modals](modals.md#choice) | `ScrollBar`, plus a `3/40` readout on the filter line |
| The [multi-line text dialog](modals.md#several-lines-of-text) | `ScrollWindow.Around` on the caret row |
