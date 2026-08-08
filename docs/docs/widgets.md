---
title: Widgets
sidebar_label: Widgets overview
description: The two widget interfaces, how Draw hands back what is left of a region, and how to write one of your own.
---

# Widgets

Reusable pieces a view draws into a [region](layout.md#regions). Two interfaces say which is which,
and they are the contract a widget of your own implements as well:

```csharp
public interface IArlecchinoWidget
{
    SurfaceRegion Draw(SurfaceRegion region);
}

public interface IArlecchinoInteractiveWidget : IArlecchinoWidget, IArlecchinoFocusable;
```

| Widget | Contract |
|---|---|
| [`ListBox<T>`](lists.md), [`Table<T>`](table.md), [`Tree<T>`](tree.md), [`Tabs`](tabs.md), [`ScrollPane`](scrolling.md), [`TextView`](text-view.md), [`Form`](forms.md) | `IArlecchinoInteractiveWidget` |
| [`ProgressBar`](status-bar.md#progressbar), [`StatusBar`](status-bar.md), [`Spinner`](status-bar.md#spinner), [`Sparkline`](charts.md#sparkline), [`BarChart<T>`](charts.md#barchart), [`Gauge`](charts.md#gauge) | `IArlecchinoWidget` |

## Draw returns what is left

`Draw` paints the widget and answers what is left of the region underneath it, so a view can stack one
thing after another without counting rows by hand:

```csharp
private readonly StatusBar _header;
private readonly Tabs _tabs;
private readonly ListBox<Mod> _list;
private readonly Surface surface;

var rest = _header.Draw(surface.Content);
var below = _tabs.Draw(rest);

_list.Draw(below);
```

A widget that fills whatever it is given — a list, a pane, a tree — returns an empty region. One that
owns a known number of rows returns the rest, which is what replaces a hand-counted `SplitTop`.

## No coordinates, no text, no color arguments

A widget holds no coordinates of its own — it paints the region it is handed, so the same one works in
a pane, in a column or across the whole frame.

None of them holds user-visible text of their own: labels are `Func<string>` supplied by the
application, which is what keeps [localization](localization.md) working. Color is a `Style` or
`ItemStyle` property rather than an argument to `Draw`, so the call is the same for every widget.

An interactive one adds what [`IArlecchinoFocusable`](focus.md) asks for — `IsFocused`, `Handle`,
`HandleMouse` — which is what lets it drop straight into a `FocusRing` and answer keys and clicks with
the view routing nothing by hand.

## Widgets from the container

A widget of your own can also come from the container: `.AddGeneratedWidgets()` registers every one
declared in the project as a singleton, and `.AddWidget<T>()` does a single one — see
[Source generator](source-generator.md#widgets).

A registered widget is shared by every screen that resolves it, state and focus included, so it fits a
panel the application has one of. The built-in widgets keep being constructed in the view, since a
`Render` or a `Columns` belongs to the screen using them.

## Data belongs to the drawing thread

`Items`, `Rows` and `Roots` are read while the frame is drawn, so the collection behind them belongs to
the [drawing thread](frame-loop.md#which-thread-draws) like everything else: change it from a view, a
command or a callback, and hand changes that arrive from anywhere else to `FrameThread.Post`.

A collection that empties in the middle of a frame no longer throws — the frame ends early and a
warning names the route — but the frame it cut short was still a frame nobody asked for.

## Putting them together

The sample has a screen wired exactly this way — tabs, a sortable table, a list, a progress bar and a
status bar in one `FocusRing`:

```bash
dotnet run --project samples/Arlecchino.Sample -- --frame widgets 100x24
```

## Writing your own

Implement `IArlecchinoInteractiveWidget` — or `IArlecchinoWidget` for something that only draws. There
is nothing to register and nothing to inherit; the widgets above are written against the same public
API an application has:

```csharp
public sealed class Badge : IArlecchinoInteractiveWidget
{
    private const int BorderedRows = 3;

    private readonly ArlecchinoKeymap _keymap;
    private SurfaceRegion _drawn;

    public Badge(ArlecchinoKeymap keymap) => _keymap = keymap;

    public required Func<string> Label { get; init; }
    public Func<ViewRoute>? OnActivate { get; init; }
    public bool IsFocused { get; set; }

    public SurfaceRegion Draw(SurfaceRegion region)
    {
        _drawn = region;
        var inner = region.Border(IsFocused ? Theme.Active : Theme.Muted);
        inner.WriteLine(0, Label(), IsFocused ? Theme.ActiveSelected : Theme.Default, Align.Center);

        return region.Rows(BorderedRows, region.Height - BorderedRows);
    }

    public FocusResult Handle(KeyPress key) =>
        _keymap.Confirm.Matches(key) && OnActivate is not null
            ? FocusResult.Navigate(OnActivate())
            : FocusResult.Ignored;

    public FocusResult HandleMouse(MouseEvent mouse) =>
        mouse.IsLeftClick && _drawn.Contains(mouse.Row, mouse.Column)
            ? FocusResult.Handled
            : FocusResult.Ignored;
}
```

`_focus.Add(_badge)` is the whole integration: cycling, focus on click and key routing come from the
[ring](focus.md).

### Five conventions

| Convention | Why |
|---|---|
| Remember the region you were given in `Draw` | It is what resolves a click afterward — `Contains` and `ToLocal` work in frame coordinates |
| Take keys from `ArlecchinoKeymap`, never `ConsoleKey` directly | A rebound key relabels and reroutes itself everywhere |
| Measure with [`TextWidth`](text.md), not `string.Length` | A cell holds a grapheme cluster; CJK and emoji are two columns wide |
| Color with roles from [`Theme`](theming.md) | Swapping the palette restyles the widget with everything else |
| Take user-visible text as `Func<string>` | The application may translate it and switch language at runtime — see [Localization](localization.md) |

[`ScrollWindow.Around`](scrolling.md#scrollwindow) and [`ScrollBar`](scrolling.md#scrollbar) are public
for the same reason: a list of your own scrolls exactly as `ListBox` does.

:::note[What a widget cannot do yet]

A widget cannot contain another focusable widget — a `FocusRing` does not nest, so a composite lays its
parts out itself and routes to them by hand.

:::
