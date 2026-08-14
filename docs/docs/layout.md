---
title: Layout
sidebar_label: Layout
description: The ways to place things on a Surface — the flow cursor, absolute coordinates, regions with their own coordinate system and clipping, and a tree of panes for screens made of several.
---

# Layout

There is no layout engine and no component tree. A view draws where it says to draw, and the surface
offers three ways of saying it: a **flow cursor** that walks down the screen, **absolute** calls that
address a row directly, and **regions** that carve the frame into rectangles with their own
coordinates. Most views use one; a view with panes uses regions and never counts a row itself.

A screen made of several panes can describe its shape as a [tree](#panes-as-a-tree) instead of
carving it by hand. That is still not a component tree: it decides where things go and nothing else —
no lifetimes, no state, no re-render pass. Widgets stay widgets, and a view that would rather split
regions itself keeps working exactly as before.

## Flow layout

Flow calls advance an internal cursor line by line. They are the default way to write a view.

| Call | Behavior |
|---|---|
| `AppendLine(text, style, align, margin)` | One line at the cursor, honoring `Align.Left/Center/Right` inside the content width and all four margins |
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

These calls belong to the frame. Inside a [pane](#writing-line-after-line-inside-a-pane) they write at
the top of the screen and paint over its border, so a pane filled line by line asks the region for a
flow of its own.

## Absolute layout

Absolute calls address rows directly and ignore the flow cursor — this is what the file picker and the
modal boxes are drawn with.

| Call | Behavior |
|---|---|
| `WriteAt(row, column, text, style)` | Writes at an exact cell, clipping to the frame |
| `WriteLineAt(row, text, style)` | Restyles the whole row, then writes the text at `HorizontalPadding` |
| `FillLineAt(row, style)` | A rule on that row |

`WriteBlock(lines, style, align, margin)` sits in between: it takes a block of pre-built lines and
places it as a unit, aligned horizontally (`Left`/`Center`/`Right`) and vertically
(`Top`/`Middle`/`Bottom`) against the whole frame.

## Align and Margin

`Align` is a `[Flags]` enum, so the two axes combine. `Align.Right | Align.Bottom` is how the hints
box is anchored to a corner. Only the block and region calls honor the vertical flags; a flow line
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
spilled onto a neighbor:

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
| `Flow()` | A cursor that writes line after line inside this region — see [below](#writing-line-after-line-inside-a-pane) |
| `Contains(frameRow, frameColumn)` / `ToLocal(...)` | Hit-testing for [mouse events](mouse.md) |

`SurfaceRegion` is a readonly record struct, so `region with { Top = region.Top - offset }` is a valid
way to shift one, and two regions compare by value.

Both the modal boxes and the file picker are drawn this way, so the same code that positions a pane
also answers "was this click inside it".

## Panes as a tree

Regions solve placement, but a screen with four panes spreads its shape across the whole of `Draw`:
half a dozen `SplitTop` and `SplitLeft` calls interleaved with the drawing, and changing the
proportions means finding every one of them. `PaneTree` states the shape once, in one expression, and
draws it in one call.

```csharp
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

public sealed class PanesView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly PaneTree _layout;
    private readonly FocusRing _focus;

    public PanesView(Surface surface, ArlecchinoOptions options)
    {
        _surface = surface;

        var files = new ListBox<string>(options.Keymap)
        {
            Render = file => $" {file}",
            Items = Files(),
        };
        var editor = new TextView(options.Keymap) { Text = Readme() };
        var log = new ListBox<string>(options.Keymap) { Render = line => line, Items = Log() };
        var status = new StatusBar { Left = [() => "ready"], Right = [() => "Esc back"] };

        _layout = Branch(
            Rows,
            3,
            Leaf(DrawToolbar, () => "toolbar"),
            Branch(
                Rows,
                PaneSize.CellsFromEnd(2),
                Branch(
                    Columns,
                    0.25,
                    Leaf(files, () => "files"),
                    Branch(0.7, Leaf(editor, () => "editor"), Leaf(log, () => "log"))),
                Leaf(status))).Gaps(inner: 1, outer: 1);

        _focus = _layout.AsFocusRing(options.Keymap);
    }

    public void Draw() => _layout.Draw(_surface.Content);

    public ViewRoute Handle(KeyPress key) => _focus.Handle(key);
}
```

Two members build the whole thing — `Branch` and `Leaf` — and a `using static` of `PaneTree` and
`PaneSplit` is what lets them read without a prefix on every line.

```
╭─ toolbar ──────────────────────────────────────────────────╮
│                                                            │
╰────────────────────────────────────────────────────────────╯

 Program.cs      ╭─ editor ─────────────────────────────────╮
 PanesView.cs    │                                          │
 WidgetsView.cs  │                                          │
 SettingsView.cs ╰──────────────────────────────────────────╯
                 ╭─ log ────────────────────────────────────╮
                 │                                          │
                 ╰──────────────────────────────────────────╯
 ready                                               Esc back
```

### How to read one

Every node is either a **branch** or a **leaf**. A branch has exactly two halves; the size it carries
says how much the *first* of them takes, and the second takes what is left. Three bands stacked is
therefore a branch inside a branch, which is what the nesting above is: the toolbar, then everything
else, and inside that everything-else the body and the status row.

`Rows` cuts top from bottom, `Columns` cuts left from right, and the size always applies to the first
half — `Branch(Rows, 3, header, body)` gives the header three rows, `Branch(Columns, 0.25, side, main)`
gives the sidebar a quarter of the width.

Only the two halves are ever required. Say the direction, or the size, or both, or neither:

| Call | Means |
|---|---|
| `Branch(Rows, 3, a, b)` | Cut into rows, three of them for `a` |
| `Branch(Rows, a, b)` | Cut into rows, half each |
| `Branch(0.25, a, b)` | A quarter for `a`, cut along whichever side is longer |
| `Branch(a, b)` | Half each, along whichever side is longer |

"The longer side" is measured in what the eye sees rather than in cells: a terminal cell is about
twice as tall as it is wide, so an 80×24 region is a wide one and gets two columns, while 40×24 gets
two rows. It is worked out per frame, so a branch left to decide can turn from columns into rows when
the window is resized — which is what you want for panes of equal standing, and not what you want for
chrome. Pin a toolbar with `Rows` and a sidebar with `Columns`; leave the rest to the tree.

Nothing about a frame is kept in the tree. Sizes are worked out on every `Draw`, so one tree fits
every terminal and a resize needs no bookkeeping.

### Sizes

`PaneSize` is three measures, and a layout survives a resize by picking the right one per split:

| Size | Means | For |
|---|---|---|
| `0.25` — any `double` | A share of what there is | Panes that grow with the window: a sidebar, two halves of an editor |
| `3` — any `int` | Exactly that many cells | Chrome of a fixed height: a toolbar, a title, a one-line prompt |
| `PaneSize.CellsFromEnd(2)` | Everything except that many cells | Chrome anchored to the far edge: a status bar at the bottom, a gutter on the right |

**The unit is the literal, not the number.** `double` and `int` both convert on their own, so what a
size means is decided by whether it has a decimal point:

```csharp
Branch(Rows, 3, header, body);      // three rows
Branch(Rows, 0.3, header, body);    // three tenths of the height
Branch(Columns, 3, side, main);     // three columns — a count follows the direction of the cut
```

A count is in the units of the cut: rows for `Rows`, columns for `Columns`. `PaneSize.Fraction(0.25)`
and `PaneSize.Cells(3)` are the same two things spelled out, for where the literal is not obvious
enough.

Two edges are worth knowing. `1` and `1.0` are different sizes — one row against all of them — so a
missing point is a real bug rather than a rounding difference. And a bare `0` does not compile: it
fits a `PaneSplit` and a size equally well, and the compiler says so instead of guessing, so write
`PaneSize.Fraction(0)` or `PaneSize.Cells(0)` when nothing is what you mean.

A share is clamped to `0..1`, and a count larger than the region gives the first half everything and
the second half nothing.

`CellsFromEnd` is what a status bar wants. Written as a share, a one-row bar is `0.96` on one terminal
and wrong on the next; written as `Rows(PaneSize.CellsFromEnd(1), body, status)` it is the last row on
all of them.

### What goes in a pane

| Leaf | Use |
|---|---|
| `Leaf(widget)` | Any [widget](widgets.md) — a list, a table, a tree, a status bar |
| `Leaf(widget, () => "files")` | The same, in a box with that title |
| `Leaf(region => ...)` | Drawing the view does itself: a title, a box, a row of readouts |
| `Leaf(region => ..., () => "log")` | The same, in a box |
| `Leaf()` | Space deliberately left blank |

A title is a `Func<string>` rather than a string, like every other piece of user-visible text in the
framework, so a translated application translates the panes too. The box is drawn for you and the
pane is handed the room left inside it — which is the whole of what a `region.Border(...)` call in
every pane used to do.

A boxed widget also shows where the focus is: the border is `Theme.Active` while that widget holds it
and `Theme.Info` while it does not, so the view says nothing about focus and the screen still shows
it.

The same widget instance cannot be two panes. A widget remembers the region it was drawn into — that
is how it answers clicks — so one in two places would draw twice and hit-test for one of them only.
The tree rejects it as it is built rather than letting the screen misbehave.

A widget pane calls the widget's own `Draw` with the region and ignores the region it hands back,
since the tree has already decided where everything goes. Both leaf kinds are checked for `null` as
the tree is built, so a mistake surfaces at construction rather than on the first frame.

Because the tree holds what it draws, it is built where the widgets are — in the view's constructor —
and lives as long as the view does. It is not a `static readonly` shared between views: two views
sharing one tree would share its widgets, and therefore their state.

### Tab walks the panes

A screen of panes wants `Tab` to move between them in the order they are drawn, and the tree already
knows that order. `AsFocusRing` builds the [focus ring](views-and-navigation.md) out of the layout —
every pane of it that takes the focus, left before right and top before bottom — so there is no second
list to keep in step by hand:

```csharp
_focus = _layout.AsFocusRing(options.Keymap);

public ViewRoute Handle(KeyPress key) => _focus.Handle(key);
public ViewRoute HandleMouse(MouseEvent mouse) => _layout.HandleMouse(mouse);
```

Widgets that cannot take the focus — a status bar, a pane the view draws with a delegate — are simply
left out. Rearranging the tree rearranges the tab order with it, which is the point: the two cannot
drift apart, because there is only one of them.

What comes back is an ordinary ring, so anything focusable that lives outside the tree is added to it
afterward and lands at the end of the walk.

`HandleMouse` on the tree is the other half. The tree already worked out which pane owns which cells
in order to draw them, and the same knowledge says where a click goes: it reaches the pane it landed
in rather than being offered to every widget on the screen in turn, and no widget is asked to guess
whether the point was its own. The pane that claims it takes the focus with it, for a tree whose ring
came from `AsFocusRing`. A click in the gap between panes, in the space around them, or before the
first frame was drawn belongs to no pane and is left alone.

### Writing line after line inside a pane

Flow calls belong to the **frame**, not to a region. Reaching for `region.Surface.AppendLine(...)`
inside a pane therefore writes at the top of the screen and paints straight over the pane's border and
its neighbors — the region is not involved at all:

```
PLAYERS             ╮╭ right ───────────╮     ← the flow cursor is the frame's
│                  ││right              │
```

A region has a flow of its own for exactly this, and it stays where it was given:

```csharp
var flow = region.Flow();

flow.AppendLine("PLAYERS", Theme.TableHeader);
flow.FillLine();

foreach (var player in players)
{
    flow.AppendLine(player.Name, Theme.Default);
}
```

Everything is written in the region's coordinates and clipped to it, and once the pane is full the
calls stop doing anything — a loop over more rows than fit needs no bound of its own.

| Member | Meaning |
|---|---|
| `AppendLine(text, style, align)` | The next line, aligned inside the region |
| `SkipLine()` / `Skip(rows)` | Leaves rows blank |
| `FillLine(style)` | A rule across the region |
| `Rewind()` | Back to the first row |
| `Rest()` | What the cursor has not reached yet, as a region — for handing the space below to a widget |
| `Row`, `FreeLines`, `IsFull`, `Region` | Where the cursor is and how much room is left |

`PaneFlow` is a class, so passing it to a helper that writes a few more lines carries the cursor
along. Two flows over the same region are independent: the second starts at its first row again.

### Gaps, and panes that do not fit

Spacing belongs to the tree rather than to a call or to a branch, so a screen is loosened or tightened
in one place. `Gaps(inner, outer)` is named the way a tiling window manager names it:

```csharp
_layout = Branch(...).Gaps(inner: 1, outer: 1);
```

`inner` is left empty between the two halves of every branch; `outer` is left empty around everything,
inside the region `Draw` is handed. Both default to nothing, which packs panes edge to edge — what a
screen of bordered boxes wants, since the borders already separate them. `Gaps` returns the tree it
was called on, so it finishes the expression that built it.

With no inner gap, panes in a box **share** the line between them rather than each drawing one of
their own — the tree records its boxes in a [`Joinery`](#borders-that-join) and paints them together:

```text
├─ files ────────────┬─ authors ─────────────┬─ log ────────────┤
│ Program.cs         │ fEst                  │ the rest of it   │
╰────────────────────┴───────────────────────┴──────────────────╯
```

A pane without a box keeps the room it was given — it would lose a column of what it draws to a
neighbor's border — and a tree with a gap is drawn as it always was. The pane holding the focus wins
the color of the edges it shares, so `Tab` still moves a highlight around the screen.

A region too small for what it holds does not overflow. Each split is clamped to the space that
exists, so the first half takes what it can and the panes that did not fit are handed **empty**
regions; drawing into one of those writes nothing, exactly as writing outside a region does. A view
needs no `if (Height > 10)` guards — a terminal too small for the screen is the application's business
through `MinimumWidth`/`MinimumHeight`, not the layout's.

One row is worth remembering: with `ShowOutputLine` on, the framework draws the output line over the
last row of the frame. A status bar of your own belongs one row above it — `CellsFromEnd(2)` rather
than `CellsFromEnd(1)` — or it is drawn and then covered.

### Members

| Member | Meaning |
|---|---|
| `Branch(split, size, first, second)` | A branch; either of `split` and `size` may be left out |
| `Leaf(widget)` / `Leaf(draw)` / `Leaf()` | A pane holding a widget, one the view draws, or nothing |
| `Leaf(widget, title)` / `Leaf(draw, title)` | The same, in a box with a title |
| `Gaps(inner, outer)` | Spacing for the whole tree; returns the tree |
| `Draw(region)` | Draws every pane where the branches put it |
| `AsFocusRing(keymap)` | The focus ring of the screen, panes in layout order |
| `Count`, `InnerGap`, `OuterGap` | How many panes it holds, and the spacing it was given |

### When not to reach for it

A tree earns its keep from about three panes up. A view that draws a list under a title is shorter
with flow calls, and two panes side by side are clearer as one `SplitLeft`. The tree is for screens
whose shape is worth naming — and where changing `0.25` to `0.3` should be a one-character edit
rather than a hunt through `Draw`.

## Borders that join

`region.Border(...)` draws a box that knows nothing about its neighbors. That is right for a box
standing on its own and wrong for panes that touch: two of them side by side put two verticals where
the eye expects one.

`Joinery` records boxes and rules instead of drawing them, and paints at the end — so a shared cell
becomes the glyph that joins them:

```csharp
var joinery = new Joinery();

var files = joinery.Box(left, Theme.Info, "files");
var log = joinery.Box(right, Theme.Active, "log");

joinery.Draw(surface.Content, Theme.Info);
```

```text
╭─ one ───────────────┬─ three ──────────────╮
│                     │                      │
├─ two ───────────────┼─ four ───────────────┤
│                     │                      │
╰─────────────────────┴──────────────────────╯
```

| Member | Meaning |
|---|---|
| `Box(region, style, title)` | Records four edges and hands back the room inside, as `Border` does |
| `Across(region, row)` | A rule across the region, joining whatever it meets |
| `Down(region, column)` | A rule down it |
| `Draw(into, style)` | Paints everything, then the titles. `style` covers what was recorded without one |
| `Count` | How many cells carry a line so far |

Coordinates are the surface's own, so regions from anywhere on the frame are recorded together, and
anything falling outside `into` is left undrawn rather than clamped into it. A cell takes the style of
the last thing recorded over it, which is how the pane holding the focus wins the edges it shares —
record it last.

## Clipping a whole stretch of drawing

A region clips writes to its own bounds, which is enough while the coordinates belong to it. Scrolling
breaks that: the content is drawn shifted, so it reaches outside the window on purpose and must not
land on a neighbor. `Surface.Clip` confines every write to a rectangle until the scope is disposed,
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
| Two panes, a bordered dialog | Regions |
| Three panes or more, chrome around a body | A `PaneTree` built in the constructor |
| Content longer than its pane | A region plus [`ScrollPane`](scrolling.md) |
| Anything that has to answer a click | Regions — `Contains` is the hit test |
