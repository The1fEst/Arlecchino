---
title: Arlecchino.Widgets.Readouts
sidebar_label: Arlecchino.Widgets.Readouts
sidebar_position: 0
---

# Arlecchino.Widgets.Readouts

## Classes

| Type | Summary |
|---|---|
| [`AreaChart`](AreaChart.md) | A series drawn as a filled area over as many rows as it is given — the shape a system monitor shows. Where [`Sparkline`](../arlecchino.widgets.readouts/Sparkline.md) fits a row and reads at a glance, this one fills a pane and is meant to be looked at: the newest value is at the right, the fill climbs with the value, and the color comes from how high it climbed rather than from anything the view works out. A series with no spread at all — every number the same — draws as the lowest level along the bottom rather than as nothing, the way a [`Sparkline`](../arlecchino.widgets.readouts/Sparkline.md) does. The resolution is in the characters. A cell carries two samples side by side and several levels of height, so a chart eight rows tall has thirty-two levels between empty and full and holds twice the history a row of blocks would — see [`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md) for what each set costs in font support. |
| [`BarChart<T>`](BarChart-1.md) | One bar per item, laid out down the region: the label in front, the bar across the middle, the readout behind. Bars are measured against the largest item unless told otherwise, so a chart of things that are all small still fills the pane instead of drawing four invisible stubs. |
| [`Gauge`](Gauge.md) | One value against a range that means something, drawn as a bar whose color changes as it crosses the bands it was given: the fill turns amber where the load is worth watching and red where it is not, and each part keeps the color of the band it lies in, so the tail of the bar shows how long it has been past the line. A [`ProgressBar`](../arlecchino.widgets.readouts/ProgressBar.md) answers "how far along", and this answers "how bad is it now" — the difference being the bands, and a range that need not start at zero. |
| [`ProgressBar`](ProgressBar.md) | A filled bar showing how far along something is, with an optional readout beside it. |
| [`Sparkline`](Sparkline.md) | A series of numbers as one row of blocks, tallest for the largest of them. It says nothing about what the numbers are — no axis, no scale, no grid — which is what lets it sit in a status bar, a table cell or a corner of a pane and still be read at a glance: the shape of the line is the point. The newest value is the rightmost, and only the last of them fit the row, so a widening terminal shows more history rather than a wider drawing of the same history. |
| [`Spinner`](Spinner.md) | A one-cell animation for work of unknown length. It does not run on its own: something has to step it, which keeps the framework free of timers the application did not ask for. |
| [`StatusBar`](StatusBar.md) | A line of short readouts pinned to an edge. Items are delegates because a status line is redrawn every frame and is expected to show what is true now, not what was true when it was built. |
| [`TextView`](TextView.md) | A block of text to read: wrapped to the width it is given, scrolled with the movement keys and the wheel. This is the widget for a description, a log, the output of something that ran — anything longer than the space available and not meant to be edited. The text is re-wrapped whenever the width changes, so resizing the terminal reflows it rather than cutting it off. |

## Structs

| Type | Summary |
|---|---|
| [`GaugeBand`](GaugeBand.md) | Where a band of a [`Gauge`](../arlecchino.widgets.readouts/Gauge.md) starts and how it is drawn. A band runs from [`GaugeBand.From`](../arlecchino.widgets.readouts/GaugeBand.md#from) up to the start of the next one, so the bands are given in order and the first of them decides the color of everything below it. |

