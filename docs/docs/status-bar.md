---
title: Status bar and indicators
sidebar_label: Status bar and indicators
description: StatusBar, ProgressBar and Spinner — the three widgets that report rather than accept input.
---

# Status bar and indicators

Three widgets that only draw. None of them takes the focus, and all three implement
`IArlecchinoWidget` rather than the interactive one.

## StatusBar

```csharp
private readonly Spinner _spinner = new();

new StatusBar
{
    Left = [() => Loc(LocString.ItemCount, count), () => _spinner.Current],
    Right =
    [
        () => $"{keymap.NextField} {Loc(LocString.Panes)}",
        () => $"{keymap.Cancel} {Loc(LocString.Back)}",
    ],
}.Draw(region.Rows(region.Height - 1, 1));
```

Left and right groups joined with three spaces; the right side is **dropped** when it would collide
with the left instead of overwriting it, so a narrow terminal loses the least important half rather
than producing a mess.

Empty entries are skipped, so a part that is only sometimes relevant can return `""`:

```csharp
private string _filter = "";

Left = [() => _filter.Length > 0 ? $"filter: {_filter}" : ""],
```

Interpolating a [`KeyBinding`](keyboard.md#keybinding) rather than writing `Tab` is what keeps the bar
truthful when a key is rebound.

:::note[The status bar and the output line]

They are different things. The [output line](state.md#the-output-line) is framework chrome on the last
row, turned on and off with `options.ShowOutputLine`. A `StatusBar` is a widget a screen draws
wherever it likes — usually on the last row of its own region, which is why applications that want the
row for themselves run with the output line off.

:::

## ProgressBar

```csharp
var progress = new ProgressBar { Value = 68, Caption = value => $"{value:0}%" };

progress.Draw(region.Rows(0, 1));
```

| Member | Meaning |
|---|---|
| `Value` | Where it is |
| `Minimum` / `Maximum` | Default to `0` and `100` |
| `Caption` | Turns the value into the text beside the bar |
| `Style` | Colours it |

The bar fills the region width minus the caption, so the caption never pushes it off the edge.

## Spinner

```csharp
_spinner.Advance();                       // once per frame or per tick
_spinner.Draw(region.SplitLeft(region.Width - 1).Right);
```

`Spinner` cycles a set of frames — braille dots by default, replaceable through `Frames` — and paints
the **top-left cell** of whatever region it is given, so hand it the one cell it belongs in. `Current`
is the frame as a string, for putting it in a [status bar](#statusbar) instead.

Nothing advances it for you. Two places usually do:

```csharp
private readonly Ticker _ticker;

_ticker.Every(TimeSpan.FromMilliseconds(80), () => _spinner.Advance());
```

— which spins at a steady rate whether or not anything else is drawing, and is what to use beside an
[async atom](async-atoms.md); or a call in `Draw`, which spins once per frame and therefore only while
something else is already asking for frames.
