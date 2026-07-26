[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Widgets

Reusable pieces a view draws into a [region](rendering.md). Two interfaces say which is which, and
they are the contract a widget of your own implements as well:

```csharp
public interface IArlecchinoWidget
{
    void Draw(SurfaceRegion region);
}

public interface IArlecchinoInteractiveWidget : IArlecchinoWidget, IFocusable;
```

A widget holds no coordinates of its own — it paints the region it is handed, so the same one works
in a pane, in a column or across the whole frame. An interactive one adds what
[`IFocusable`](views-and-navigation.md) asks for (`IsFocused`, `Handle`, `HandleMouse`), which is what
lets it drop straight into a `FocusRing` and answer keys and clicks with the view routing nothing by
hand.

| Widget | Contract |
|---|---|
| `ListBox<T>`, `Table<T>`, `Tree<T>`, `Tabs`, [`Form`](state-and-forms.md) | `IArlecchinoInteractiveWidget` |
| `ProgressBar`, `StatusBar`, `Spinner` | `IArlecchinoWidget` |

None of them holds user-visible text of their own: labels are `Func<string>` supplied by the
application, which is what keeps [localization](localization.md) working. Colour is a `Style`
property rather than an argument to `Draw`, so the call is the same for every widget.

A widget of your own can also come from the container: `.AddGeneratedWidgets()` registers every one
declared in the project as a singleton, and `.AddWidget<T>()` does a single one — see
[Source generator](source-generator.md#widgets). Shared by every screen that resolves it, state and
focus included, so it fits a panel the application has one of; the built-in widgets keep being
constructed in the view, since a `Render` or a `Columns` belongs to the screen using them.

## ListBox

```csharp
_authors = new ListBox<string>(options.Keymap)
{
    Render = author => $" {author}",
    Style = author => author == _mine ? Theme.Active : Theme.Default,
    OnActivate = author => ViewKind.Author,
    Items = authors,
};

_authors.Draw(region);
```

Arrows move, `PgUp`/`PgDn` jump ten rows, `Home`/`End` go to the ends, `Confirm` activates. The wheel
scrolls, a click selects a row and a click on the selected row activates it. The selected row is drawn
`ActiveSelected` while focused and `Selected` while not, so a list beside another pane still shows
where the cursor is.

Only the visible slice is rendered — `ScrollWindow.Around(selected, count, rows)` is the same helper
the widget uses, available for lists you draw yourself.

A list with more items than rows grows a scroll bar down its last column, and the rows are truncated
one cell earlier to make room rather than being covered by it. A list that fits keeps its full width
and shows nothing. The thumb is at least one cell tall and only touches an end when the list does, so
"near the end" never looks like "at the end". `ScrollBar.IsNeeded(total, rows)` and
`ScrollBar.Draw(region, first, total)` are public, for panes you lay out yourself.

Choice and multi-choice modals get the same treatment: a bar beside the options and a `3/40` readout
on the filter line, worded by `Strings.ListPosition`.

## Table

```csharp
_mods = new Table<Mod>(options.Keymap)
{
    Columns =
    [
        new() { Header = () => Loc(LocString.Name), Cell = mod => mod.Name,
                Sort = (first, second) => string.CompareOrdinal(first.Name, second.Name) },
        new() { Header = () => Loc(LocString.Author), Cell = mod => mod.Author, Width = 12 },
        new() { Header = () => Loc(LocString.Files), Cell = mod => mod.Files.ToString(),
                Width = 6, AlignRight = true, Sort = (first, second) => first.Files.CompareTo(second.Files) },
    ],
    Style = mod => mod.Enabled ? Theme.Default : Theme.Muted,
    Rows = catalog,
};
```

A column with `Width = 0` takes an equal share of what is left after the fixed ones; `AlignRight`
pads on the left instead. `SortBy(column)` sorts by that column and flips the direction when called
again — the header of the sorted column shows `↑` or `↓`. Columns without a `Sort` comparison are
never sorted, so `SortBy` on them does nothing.

Rows, movement, clicks and activation come from the `ListBox` inside, so everything above applies.

## Tree

```csharp
_nodes = new Tree<VltNode>(options.Keymap)
{
    Render = node => node.Name,
    OnExpanding = node => node.Children = _vlt.LoadChildren(node.Value),
    OnActivate = node => ViewKind.Node,
    Roots = roots,
};
```

Rows are indented two columns per level and prefixed with `▾` / `▸`; leaves get no marker. `→`
expands the node under the cursor and then steps into it, `←` collapses it and then walks up to the
parent, `Confirm` toggles a branch or activates a leaf through `OnActivate`. Clicking the marker
toggles that node without activating it; clicking elsewhere selects the row, and a second click
activates it.

`OnExpanding` fires just before a node opens, which is where children are filled in for a tree that
loads level by level. `ExpandAll()` / `CollapseAll()` walk the whole thing.

## Tabs

```csharp
_tabs = new Tabs(options.Keymap)
{
    Titles = [() => Loc(LocString.Installed), () => Loc(LocString.Available)],
    OnSelected = index => _view = index,
};
```

`←→` switch, a click picks the tab under the cursor, `OnSelected` fires only when the selection
actually changes.

## ProgressBar and Spinner

```csharp
var progress = new ProgressBar { Value = 68, Caption = value => $"{value:0}%" };
progress.Draw(region.Rows(0, 1));

_spinner.Advance();                       // once per frame or per tick
_spinner.Draw(region.SplitLeft(region.Width - 1).Right);
```

`ProgressBar` fills the region width minus the caption; `Minimum`/`Maximum` default to `0`/`100`.
`Spinner` cycles a set of frames — brail dots by default, replaceable through `Frames` — and paints
the top-left cell of whatever region it is given, so hand it the one cell it belongs in.

## StatusBar

```csharp
new StatusBar
{
    Left = [() => Loc(LocString.ItemCount, count), () => _spinner.Current],
    Right = [() => $"{keymap.NextField} {Loc(LocString.Panes)}", () => $"{keymap.Cancel} {Loc(LocString.Back)}"],
}.Draw(region.Rows(region.Height - 1, 1));
```

Left and right groups joined with three spaces; the right side is dropped when it would collide with
the left instead of overwriting it. Empty entries are skipped, so a part that is only sometimes
relevant can return `""`.

## Putting them together

The sample has a screen wired exactly this way — tabs, a sortable table, a list, a progress bar and a
status bar in one `FocusRing`:

```
dotnet run --project samples/Arlecchino.Sample -- --frame widgets 100x24
```

## Writing your own

Implement `IArlecchinoInteractiveWidget` — or `IArlecchinoWidget` for something that only draws.
There is nothing to register and nothing to inherit; the widgets above are written against the same
public API an application has:

```csharp
public sealed class Badge : IArlecchinoInteractiveWidget
{
    private readonly ArlecchinoKeymap _keymap;
    private SurfaceRegion _drawn;

    public Badge(ArlecchinoKeymap keymap) => _keymap = keymap;

    public required Func<string> Label { get; init; }
    public Func<ViewRoute>? OnActivate { get; init; }
    public bool IsFocused { get; set; }

    public void Draw(SurfaceRegion region)
    {
        _drawn = region;
        var inner = region.Border(IsFocused ? Theme.Active : Theme.Muted);
        inner.WriteLine(0, Label(), IsFocused ? Theme.ActiveSelected : Theme.Default, Align.Center);
    }

    public FocusResult Handle(ConsoleKeyInfo key) =>
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
ring. Five conventions keep a widget behaving like the built-in ones:

| Convention | Why |
|---|---|
| Remember the region you were given in `Draw` | It is what resolves a click afterwards — `Contains` and `ToLocal` work in frame coordinates |
| Take keys from `ArlecchinoKeymap`, never `ConsoleKey` directly | A rebound key relabels and reroutes itself everywhere |
| Measure with `TextWidth`, not `string.Length` | A cell holds a grapheme cluster; CJK and emoji are two columns wide |
| Colour with roles from `Theme` | Swapping the palette restyles the widget with everything else |
| Take user-visible text as `Func<string>` | The application may translate it and switch language at runtime — see [Localization](localization.md) |

[`ScrollWindow.Around`](rendering.md) and `ScrollBar` are public for the same reason: a list of your
own scrolls exactly as `ListBox` does. What a widget cannot do yet is contain another focusable
widget — a `FocusRing` does not nest, so a composite lays its parts out itself and routes to them by
hand.
