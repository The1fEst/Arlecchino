[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Rendering

`Surface` is the drawing target: a cell grid of one `char` plane and one style plane, serialized into
a single write per frame. It lives in `Arlecchino.Core` and needs nothing but an `IArlecchinoTerminal`, so it can
be used on its own, outside the hosted app.

## The frame lifecycle

`Screen` drives it at `TargetFramesPerSecond` (60 by default):

1. `StartFrame()` — reads the terminal size, reallocates the planes if it changed, clears every cell
   to a space styled `Theme.Default`, and skips `VerticalPadding` lines.
2. the current view's `Draw()`, then the output line, the hints box and any modal.
3. `Build()` — walks the grid, emits an ANSI sequence only where the style changes, and hands the
   whole frame to `IArlecchinoTerminal.Write` as one string.

Nothing is written to the terminal until `Build`, so a half-drawn frame is never visible.

`Build` compares the composed frame against the previous one and writes only what changed, jumping the
cursor to each changed run — an idle frame writes nothing at all. The first frame, a resize, a fixed
size (headless rendering) and `ForgetPreviousFrame()` all fall back to sending the whole screen.

## Frames are drawn on request

The loop does not repaint on every tick; it repaints when something asks it to. `Repaint.Request()`
is called for you by the input router after every key, by the navigator on every route change, and by
`ArlecchinoState` when `Output`, `Modal` or `FilePicker` is assigned. A resize is noticed by the loop itself.

Anything else that changes what a view draws — data loaded in the background, a timer, a view field
mutated from outside `Handle` — has to say so:

```csharp
_state.Invalidate();   // or Repaint.Request() from the service itself
```

A view that animates can call it from its own `Draw`, which effectively opts back into drawing every
tick.

## Coming back from a background task

Views, `ArlecchinoState` and the surface are touched by the render loop and the input loop; nothing about
them is thread-safe. Work that finishes on another thread hands its result back through
`UiDispatcher`, which runs queued actions on the frame loop just before the next frame is composed:

```csharp
public sealed class ModsView : IArlecchinoView
{
    private readonly UiDispatcher _dispatcher;
    private readonly ModsService _mods;
    private IReadOnlyList<Mod> _rows = [];

    public void Reload() => Task.Run(async () =>
    {
        var loaded = await _mods.LoadAsync();
        _dispatcher.Post(() => _rows = loaded);
    });
}
```

`Post` is safe from any thread, queues in order, and requests a repaint by itself. An action that
throws is logged and reported on the output line — the remaining actions still run.

If the terminal is smaller than `MinimumWidth` × `MinimumHeight`, the view is skipped entirely and a
size notice is drawn instead.

## Flow layout

Flow calls advance an internal cursor line by line. They are the default way to write a view.

| Call | Behaviour |
|---|---|
| `AppendLine(text, style, align, margin)` | One line at the cursor, honouring `Align.Left/Center/Right` inside the content width and all four margins |
| `WriteTableRow(cells, widths, style, prefix)` | A line of padded columns; a positive width right-aligns the cell, a negative one left-aligns it |
| `FillLine()` | A rule of `-` across the content width |
| `SkipLine()` | Leaves a blank line |
| `ListWindow()` | How many rows a scrolling list may use: the free lines minus room for the chrome, at least 4 |

Every flow call stops silently once the frame is full, so a view never has to bound its own output.

```csharp
_surface.AppendLine("Mods", Theme.Header, Align.Center, new Margin(0, 1, 0, 1));
_surface.WriteTableRow(["Name", "Version"], [-30, 10], Theme.TableHeader);
_surface.FillLine();
```

## Absolute layout

Absolute calls address rows directly and ignore the flow cursor — this is what the file picker and the
modal boxes are drawn with.

| Call | Behaviour |
|---|---|
| `WriteAt(row, column, text, style)` | Writes at an exact cell, clipping to the frame |
| `WriteLineAt(row, text, style)` | Restyles the whole row, then writes the text at `HorizontalPadding` |
| `FillLineAt(row, style)` | A rule of `-` on that row |

`WriteBlock(lines, style, align, margin)` sits in between: it takes a block of pre-built lines and
places it as a unit, aligned horizontally (`Left`/`Center`/`Right`) and vertically (`Top`/`Middle`/
`Bottom`) against the whole frame. `Align` is a `[Flags]` enum, so `Align.Right | Align.Bottom` is how
the hints box is anchored.

## Columns, not characters

A cell holds a whole grapheme cluster, and the surface measures text in terminal columns rather than
in `char` values — CJK and emoji take two, combining marks take none, and a surrogate pair is one
symbol. Every flow and absolute call clips, aligns and pads on that measure, so a box drawn around
`日本語` closes where it should.

`TextWidth` is the same measure, public for your own layout code:

| Call | Meaning |
|---|---|
| `Of(text)` | Width in columns |
| `Truncate(text, maxWidth)` | Cuts on a cluster boundary, never inside a symbol |
| `PadRight(text, width)` / `PadLeft(text, width)` | Pads to a column width |
| `OfCluster(span)`, `OfRune(rune)`, `NextClusterLength(text, index)` | The pieces the surface itself uses |

Use them instead of `string.Length`, `PadRight` and `text[..n]` whenever the result lands on screen.
A wide symbol occupies two cells; writing over either half clears the other, and one that would be
split by the right edge is dropped rather than half-drawn.

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
| `Inset(margin)` / `Inset(all)` | A smaller region inside this one |
| `SplitLeft(width)` / `SplitTop(height)` | Two regions; the split is clamped to what exists |
| `Rows(row, count)` | A horizontal band of the region |
| `Write(row, column, text, style)` | Writes in region coordinates, clipped to it |
| `WriteLine(row, text, style, align)` | A whole line, aligned inside the region |
| `Fill(style, character)` | Paints the region |
| `Border(style, title)` | Draws a box and returns the region inside it |
| `Contains(frameRow, frameColumn)` / `ToLocal(...)` | Hit-testing for [mouse events](commands-and-input.md) |

Both the modal boxes and the file picker are drawn this way, so the same code that positions a pane
also answers "was this click inside it".

## Geometry

| Member | Meaning |
|---|---|
| `FrameWidth` / `FrameHeight` | Size of the current frame in cells |
| `HorizontalPadding` / `VerticalPadding` | Gutters applied by the flow calls; set from `ArlecchinoOptions` |
| `SetFixedSize(width, height)` | Pins the frame size and stops the surface from asking the terminal |

`SetFixedSize` is what makes headless rendering work: pin a size, resolve `Screen`, call `DrawOnce()`,
and the frame goes to stdout as plain ANSI text — see [Hosting and options](hosting-and-options.md)
for wiring an app up without the hosted service.

Colours and styles for every call come from [Theming](theming.md).
