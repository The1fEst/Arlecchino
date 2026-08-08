---
title: Table
sidebar_label: Table
description: Table and TableColumn — columns that size themselves, sorting that flips on the second press, and the list underneath.
---

# Table

```csharp
private readonly Table<Mod> _mods;

_mods = new Table<Mod>(options.Keymap)
{
    Columns =
    [
        new() { Header = () => Loc(LocString.Name), Cell = mod => mod.Name,
                Sort = (first, second) => string.CompareOrdinal(first.Name, second.Name) },
        new() { Header = () => Loc(LocString.Author), Cell = mod => mod.Author, Width = 12 },
        new() { Header = () => Loc(LocString.Files), Cell = mod => mod.Files.ToString(),
                Width = 6, AlignRight = true,
                Sort = (first, second) => first.Files.CompareTo(second.Files) },
    ],
    ItemStyle = mod => mod.Enabled ? Theme.Default : Theme.Muted,
    Rows = catalog,
};
```

Widgets are built in the view's constructor, so `options` is the `ArlecchinoOptions` the container
hands it, and `region` is the [region](layout.md) the view draws the widget into.

## Columns

`TableColumn<T>` is the shape of one column:

| Member | Meaning |
|---|---|
| `Header` | `Func<string>`, so the heading follows the [language](localization.md) |
| `Cell` | Turns a row into the text of this column |
| `Width` | Fixed width in cells; `0` takes an equal share of what is left after the fixed ones |
| `AlignRight` | Pads on the left instead of the right |
| `Sort` | A comparison; a column without one is never sorted |

## Sorting

`SortBy(column)` sorts by that column and flips the direction when called again — the header of the
sorted column shows `↑` or `↓`. Columns without a `Sort` comparison are never sorted, so `SortBy` on
them does nothing.

That makes a sort a [view command](commands.md#commands-of-a-view) rather than a key buried in
`Handle`:

```csharp
public IReadOnlyList<ViewCommand> Commands() =>
[
    ViewCommand.For(ConsoleKey.M, () => Loc(LocString.SortByName), () => _mods.SortBy(0)),
    ViewCommand.For(ConsoleKey.N, () => Loc(LocString.SortByFiles), () => _mods.SortBy(2)),
];
```

## Everything else comes from the list

Rows, movement, clicks, activation, the focused and unfocused cursor styles and the scroll bar come
from the [`ListBox`](lists.md) inside, so everything on that page applies here.

## A worked example

Each panel of [`Arlecchino.Commander`](https://github.com/The1fEst/Arlecchino.Commander) is a sortable
table of one folder, colored by what each row is — a folder, a marked file, a hidden one:

```bash
dotnet run --project src/Arlecchino.Commander -- --frame 132x26 --left . --right src
```

See [Showcase](showcase.md).
