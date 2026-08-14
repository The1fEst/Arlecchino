---
title: Arlecchino.Widgets.Readouts
sidebar_label: Arlecchino.Widgets.Readouts
sidebar_position: 0
---

# Arlecchino.Widgets.Readouts

## Classes

| Type | Summary |
|---|---|
| [`AreaChart`](AreaChart.md) | A series drawn as a filled area over as many rows as it is given, where [`Sparkline`](../arlecchino.widgets.readouts/Sparkline.md) fits one. A cell carries two samples side by side, so it holds twice the history a row of blocks would. |
| [`BarChart<T>`](BarChart-1.md) | One bar per item, laid out down the region: the label in front, the bar across the middle, the readout behind. Bars are measured against the largest item unless told otherwise. |
| [`Gauge`](Gauge.md) | One value against a range, drawn as a bar whose color changes as it crosses the bands it was given. Each part of the fill keeps the color of its own band. |
| [`ProgressBar`](ProgressBar.md) | A filled bar showing how far along something is, with an optional readout beside it. |
| [`Sparkline`](Sparkline.md) | A series of numbers as one row of blocks, tallest for the largest of them, with no axis, scale or grid. The newest value is the rightmost, and only as many as fit the row are drawn. |
| [`Spinner`](Spinner.md) | A one-cell animation for work of unknown length. It does not run on its own: something has to step it, which keeps the framework free of timers the application did not ask for. |
| [`StatusBar`](StatusBar.md) | A line of short readouts pinned to an edge. Items are delegates because a status line is redrawn every frame and is expected to show what is true now, not what was true when it was built. |
| [`TextView`](TextView.md) | A block of text to read: wrapped to the width it is given, scrolled with the movement keys and the wheel. It is re-wrapped whenever the width changes, so resizing the terminal reflows it. |

## Structs

| Type | Summary |
|---|---|
| [`GaugeBand`](GaugeBand.md) | Where a band of a [`Gauge`](../arlecchino.widgets.readouts/Gauge.md) starts and how it is drawn. A band runs from [`GaugeBand.From`](../arlecchino.widgets.readouts/GaugeBand.md#from) up to the start of the next one, so bands are given in ascending order. |

