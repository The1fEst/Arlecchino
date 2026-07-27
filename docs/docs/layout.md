---
title: Layout
sidebar_label: Layout
description: The three ways to place things on a Surface — the flow cursor, absolute coordinates, and regions that carry their own coordinate system and clipping.
---

# Layout

There is no layout engine and no component tree. A view draws where it says to draw, and the surface
offers three ways of saying it: a **flow cursor** that walks down the screen, **absolute** calls that
address a row directly, and **regions** that carve the frame into rectangles with their own
coordinates. Most views use one; a view with panes uses regions and never counts a row itself.

## Flow layout

Flow calls advance an internal cursor line by line. They are the default way to write a view.

| Call | Behaviour |
|---|---|
| `AppendLine(text, style, align, margin)` | One line at the cursor, honouring `Align.Left/Center/Right` inside the content width and all four margins |
| `WriteTableRow(cells, widths, style, prefix)` | A line of padded columns; a positive width right-aligns the cell, a negative one left-aligns it |
| `FillLine()` | A rule across the content width |
| `SkipLine()` | Leaves a blank line |
| `ListWindow()` | How many rows a scrolling list may use: the free lines minus room for the chrome, at least four |

```csharp
_surface.AppendLine("Mods", Theme.Header, Align.Center, new Margin(0, 1, 0, 1));
_surface.WriteTableRow(["Name", "Version"], [-30, 10], Theme.TableHeader);
_surface.FillLine();
```

Every flow call stops silently once the frame is full, so a view never has to bound its own output.
The content width is the frame minus `HorizontalPadding` on both sides, so a flow view sits inside the
gutters the application configured.

## Absolute layout

Absolute calls address rows directly and ignore the flow cursor — this is what the file picker and the
modal boxes are drawn with.

| Call | Behaviour |
|---|---|
| `WriteAt(row, column, text, style)` | Writes at an exact cell, clipping to the frame |
| `WriteLineAt(row, text, style)` | Restyles the whole row, then writes the text at `HorizontalPadding` |
| `FillLineAt(row, style)` | A rule on that row |

`WriteBlock(lines, style, align, margin)` sits in between: it takes a block of pre-built lines and
places it as a unit, aligned horizontally (`Left`/`Center`/`Right`) and vertically
(`Top`/`Middle`/`Bottom`) against the whole frame.

## Align and Margin

`Align` is a `[Flags]` enum, so the two axes combine. `Align.Right | Align.Bottom` is how the hints
box is anchored to a corner. Only the block and region calls honour the vertical flags; a flow line
has already decided which row it is on.

`Margin` is `(Left, Top, Right, Bottom)`. On a flow call the top and bottom margins are blank lines
around the text; on `Inset` they are the space taken off each side.

```csharp
new Margin(2, 1, 3, 2)   // 2 left, 1 top, 3 right, 2 bottom
new Margin(1)            // the same on every side
```

## Regions

Absolute coordinates get unwieldy the moment a view has panes. A `SurfaceRegion` is a rectangle on the
surface with its own coordinate system and its own clipping — writing outside it is dropped, not
spilled onto a neighbour:

```csharp
var frame = _surface.Frame.Inset(new Margin(2, 1, 3, 2));
var (toolbar, rest) = frame.SplitTop(2);
var (browser, status) = rest.SplitTop(rest.Height - 2);
var (sidebar, list) = browser.Border(Theme.Muted).SplitLeft(22);

sidebar.Write(0, 0, "Favorites", Theme.Muted);
list.WriteLine(0, "Name", Theme.TableHeader);
```

| Member | Meaning |
|---|---|
| `Surface.Frame` / `Surface.Content` | The whole frame, and the frame minus the configured padding |
| `Left` / `Top` / `Right` / `Bottom` | The edges in frame coordinates; `Right` and `Bottom` are one past the edge |
| `Width` / `Height` / `IsEmpty` | The size, and whether there is any room to draw at all |
| `Inset(margin)` / `Inset(all)` | A smaller region inside this one |
| `SplitLeft(width)` / `SplitTop(height)` | Two regions; the split is clamped to what the region actually has |
| `Rows(row, count)` | A horizontal band of the region, clamped to its bounds |
| `Write(row, column, text, style)` | Writes in region coordinates, clipped to it — a negative column starts the text off the left edge and shows what fits |
| `WriteLine(row, text, style, align)` | A whole line, aligned inside the region |
| `Fill(style, character)` | Paints every cell of the region |
| `Border(style, title)` | Draws a box and returns the region inside it |
| `Contains(frameRow, frameColumn)` / `ToLocal(...)` | Hit-testing for [mouse events](mouse.md) |

`SurfaceRegion` is a readonly record struct, so `region with { Top = region.Top - offset }` is a valid
way to shift one, and two regions compare by value.

Both the modal boxes and the file picker are drawn this way, so the same code that positions a pane
also answers "was this click inside it".

## Clipping a whole stretch of drawing

A region clips writes to its own bounds, which is enough while the coordinates belong to it. Scrolling
breaks that: the content is drawn shifted, so it reaches outside the window on purpose and must not
land on a neighbour. `Surface.Clip` confines every write to a rectangle until the scope is disposed,
whatever coordinates the writing code uses:

```csharp
using (region.Surface.Clip(region))
{
    Content(region with { Top = region.Top - offset, Height = contentHeight });
}
```

Scopes nest and the inner one is the intersection, so a clipped pane inside a clipped pane stays
inside both. [`ScrollPane`](scrolling.md) is built on this, and it is what to reach for when writing a
widget that scrolls something of its own.

## Choosing between them

| Shape of the screen | What to reach for |
|---|---|
| A list, a form, a page of text | Flow calls |
| A box anchored to a corner | `WriteBlock` with the alignment flags |
| Two or more panes, a bordered dialog | Regions |
| Content longer than its pane | A region plus [`ScrollPane`](scrolling.md) |
| Anything that has to answer a click | Regions — `Contains` is the hit test |
