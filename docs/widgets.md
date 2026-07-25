[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Widgets

Reusable pieces a view draws into a [region](rendering.md). The interactive ones implement
[`IFocusable`](views-and-navigation.md), so they drop straight into a `FocusRing` and answer keys and
clicks without the view routing anything by hand.

None of them holds user-visible text of their own: labels are `Func<string>` supplied by the
application, which is what keeps [localization](localization.md) working.

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
_spinner.Draw(region, row: 0, column: 0);
```

`ProgressBar` fills the region width minus the caption; `Minimum`/`Maximum` default to `0`/`100`.
`Spinner` cycles a set of frames — brail dots by default, replaceable through `Frames`.

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
