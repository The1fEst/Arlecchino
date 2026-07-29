---
title: Tabs
sidebar_label: Tabs
description: Tabs — a strip of titles across the top of a pane, switched with the arrows or a click.
---

# Tabs

```csharp
private readonly Tabs _tabs;
private int _view;

_tabs = new Tabs(options.Keymap)
{
    Titles = [() => Loc(LocString.Installed), () => Loc(LocString.Available)],
    OnSelected = index => _view = index,
};
```

Widgets are built in the view's constructor, so `options` is the `ArlecchinoOptions` the container
hands it, and `region` is the [region](layout.md) the view draws the widget into.

| Member | Meaning |
|---|---|
| `Titles` | One `Func<string>` per tab, so the strip follows the [language](localization.md) |
| `Selected` | The index currently shown |
| `OnSelected` | Fires only when the selection actually changes |
| `IsFocused` | Set by the [focus ring](focus.md) |

`←→` switch, a click picks the tab under the cursor.

## Drawing what is behind them

`Tabs` owns one row and hands the rest of the region back, which is the point of
[`Draw` returning a region](widgets.md#draw-returns-what-is-left):

```csharp
private readonly Surface _surface;
private readonly ListBox<Mod> _installed;
private readonly ListBox<Mod> _available;

public void Draw()
{
    var rest = _tabs.Draw(_surface.Content);

    if (_tabs.Selected == 0)
    {
        _installed.Draw(rest);
    }
    else
    {
        _available.Draw(rest);
    }
}
```

Nothing counts rows, and the same code works whatever the terminal size is.

## Tabs and focus

Put the strip and the pane behind it in one [ring](focus.md) and `Tab` moves between them, while
`←→` stay with whichever has the cursor:

```csharp
private readonly FocusRing _focus;

_focus.Add(_tabs);
_focus.Add(_installed);
```

`OnSelected` is the place to swap what the second element of the ring is, if the tabs show genuinely
different widgets rather than different rows of the same one.

## A worked example

`samples/Arlecchino.Sample` puts its widget gallery behind tabs, one page each:

```bash
dotnet run --project samples/Arlecchino.Sample -- --frame widgets 100x24
```
