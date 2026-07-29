---
title: Charts
sidebar_label: Charts
description: Sparkline, BarChart and Gauge — a series as blocks, a set of things as bars, and one value against a banded range.
---

# Charts

Three widgets that draw numbers instead of text. Like the [indicators](status-bar.md), they only
draw: none of them takes the focus, all of them implement `IArlecchinoWidget`, and each hands back the
rows below what it drew.

All three measure in `decimal`, the same type [`ProgressBar`](status-bar.md#progressbar) and the
[value modals](modals.md) use.

## Sparkline

A series as one row of blocks — the shape of the line, with no axis, no scale and no grid, which is
what lets it sit in a status bar or a corner of a pane:

```csharp
private readonly List<decimal> _history = [];

private readonly Sparkline _downloads = new()
{
    Values = _history,
    Caption = static value => $"{value:0}/s",
};

_downloads.Draw(region.Rows(0, 1));
```

| Member | Meaning |
|---|---|
| `Values` | The series, oldest first. Held, not copied |
| `Minimum` / `Maximum` | What the lowest and tallest block stand for. The drawn values themselves when left alone |
| `Caption` | Turns the newest value into the text after the line |
| `Style` | Colours the line |

The newest value is the rightmost, and only the last of them fit the row, so a wider terminal shows
**more history** rather than a wider drawing of the same history. Nothing is copied out of `Values`,
so a ring buffer the application appends to between frames is exactly the right thing to hand over.

Leaving the range alone makes the line fill the row, which answers *how does it move*. Pinning it
answers *how big is it* instead, and keeps a line still when the numbers barely change:

```csharp
private readonly Sparkline _failures = new() { Values = _errors, Minimum = 0 };
```

A series with no spread at all — every number the same, or one number on its own — draws as the lowest
block rather than as a full row.

## BarChart

One bar per item, laid out down the region: the label in front, the bar across the middle, the readout
behind.

```csharp
private sealed record Mirror(string Name, decimal Megabytes);

private readonly Mirror[] _mirrors = [...];

private readonly BarChart<Mirror> _traffic = new()
{
    Render = static mirror => mirror.Name,
    Value = static mirror => mirror.Megabytes,
    Items = _mirrors,
    Caption = static value => $"{value:0}",
    ItemStyle = static mirror => mirror.Megabytes < 100m ? Theme.Muted : Theme.Active,
};

var rest = _traffic.Draw(region);
```

```text
europe-west  ████████████████████████████████████ 812
us-east      █████████████████████████░░░░░░░░░░░ 640
asia-south   █████████░░░░░░░░░░░░░░░░░░░░░░░░░░░ 227
cdn-fallback ██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  58
```

| Member | Meaning |
|---|---|
| `Render` | Turns an item into its label. Required |
| `Value` | The number the length of the bar stands for. Required |
| `Items` | What to chart, one bar per row |
| `Maximum` | The value at which a bar is full. The largest item when left alone |
| `Caption` | Turns a value into the readout after its bar |
| `ItemStyle` | Colours one bar |
| `LabelWidth` | Columns kept for labels. The widest label when left alone, up to a third of the region |

Bars are measured against the largest item, so a chart of things that are all small still fills the
pane. Pin `Maximum` when the point is to compare one frame against the next, or to keep a percentage
chart honest before anything has reached 100.

The readouts share one column, as wide as the longest of them, so the numbers line up under one
another. Labels longer than the label column are truncated by column, never by character, so a wide
character is not cut in half.

The chart does not scroll: items past the bottom of the region are simply not drawn, which is what
keeps it readable without the focus. Put it in a [`ScrollPane`](scrolling.md) when there are more
things than rows — or, more usually, chart the top few and list the rest.

## Gauge

One value against a range that means something, coloured by the bands it crosses:

```csharp
private readonly Gauge _disk = new()
{
    Value = 91,
    Caption = static value => $"{value:0}%",
    Bands = [new(0m, Theme.Active), new(70m, Theme.Warning), new(90m, Theme.Error)],
};

_disk.Draw(region.Rows(0, 1));
```

| Member | Meaning |
|---|---|
| `Value` | What it reads now. Outside the range it draws empty or full |
| `Minimum` / `Maximum` | The ends of the range. Default to `0` and `100` |
| `Bands` | Where the colours change, in ascending order |
| `Caption` | Turns the value into the text after the track |
| `Style` | Colours the fill outside every band |
| `Fraction` | How full it is, `0` to `1` |
| `StyleAt(value)` | The style that value is drawn in |

A `GaugeBand` is a value and a style, and it runs up to the start of the next band, so the list is
given in ascending order and the first band decides the colour of everything below it. Each part of
the fill keeps the colour of the band it lies in, so the tail of the bar shows how far past the line
the value has gone — and the caption takes the colour of the band the value itself is in.

`StyleAt` is the same lookup the fill uses, which is how a label beside the gauge is coloured to match
it:

```csharp
region.Write(0, 0, "disk", _disk.StyleAt(_disk.Value));
```

Without bands the whole fill takes `Style`, which makes the gauge a progress bar with a range of its
own.

:::note[Gauge or ProgressBar?]

A [`ProgressBar`](status-bar.md#progressbar) answers *how far along*, and a `Gauge` answers *how bad is
it now*. The difference is the bands, and a range that need not start at zero. Neither is a subset of
the other, so pick by the question the screen is asking.

:::

## What they do not do

There is no plot with axes, ticks and several series — no line chart. These three are meant to be read
at a glance next to the text they belong to, which is why they carry no chrome of their own. Put one
in a titled pane when it needs a caption:

```csharp
_layout = Branch(
    Rows,
    0.5,
    Leaf(_traffic, static () => "downloads by mirror, MB"),
    Leaf(_downloads, static () => "last 20 minutes")).Gaps(inner: 1, outer: 1);
```

See [Layout](layout.md#panes-as-a-tree) for what `Branch` and `Leaf` are.
