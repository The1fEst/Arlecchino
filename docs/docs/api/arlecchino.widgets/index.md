---
title: Arlecchino.Widgets
sidebar_label: Arlecchino.Widgets
sidebar_position: 0
---

# Arlecchino.Widgets

## Classes

| Type | Summary |
|---|---|
| [`AreaChart`](AreaChart.md) | A series drawn as a filled area over as many rows as it is given — the shape a system monitor shows. Where [`Sparkline`](../arlecchino.widgets/Sparkline.md) fits a row and reads at a glance, this one fills a pane and is meant to be looked at: the newest value is at the right, the fill climbs with the value, and the colour comes from how high it climbed rather than from anything the view works out. A series with no spread at all — every number the same — draws as the lowest level along the bottom rather than as nothing, the way a [`Sparkline`](../arlecchino.widgets/Sparkline.md) does. The resolution is in the characters. A cell carries two samples side by side and several levels of height, so a chart eight rows tall has thirty-two levels between empty and full and holds twice the history a row of blocks would — see [`GraphSymbols`](../arlecchino.rendering/GraphSymbols.md) for what each set costs in font support. |
| [`BarChart<T>`](BarChart-1.md) | One bar per item, laid out down the region: the label in front, the bar across the middle, the readout behind. Bars are measured against the largest item unless told otherwise, so a chart of things that are all small still fills the pane instead of drawing four invisible stubs. |
| [`Gauge`](Gauge.md) | One value against a range that means something, drawn as a bar whose colour changes as it crosses the bands it was given: the fill turns amber where the load is worth watching and red where it is not, and each part keeps the colour of the band it lies in, so the tail of the bar shows how long it has been past the line. A [`ProgressBar`](../arlecchino.widgets/ProgressBar.md) answers "how far along", and this answers "how bad is it now" — the difference being the bands, and a range that need not start at zero. |
| [`ListBox<T>`](ListBox-1.md) | A scrolling list of items, one per row. It keeps only the selected index, never a copy of the items, so replacing [`ListBox.Items`](../arlecchino.widgets/ListBox-1.md#items) between frames is a normal thing to do. |
| [`Picture`](Picture.md) | An image drawn in cells. Each cell carries two pixels — the upper half block is painted in the colour of the pixel above and its background in the colour of the pixel below — so a cell, which is about twice as tall as it is wide, comes out roughly square per pixel. That is the default because it needs nothing of the terminal but the colour it already draws in: no protocol, no state left behind, nothing to clean up when the picture goes away. Where the terminal speaks a graphics protocol, [`Picture.Protocol`](../arlecchino.widgets/Picture.md#protocol) sends the pixels themselves instead and the picture is as sharp as the screen allows. The pixels are handed over rather than read from a file: decoding PNG or JPEG belongs to the application, which knows what it wants to depend on, while the framework only draws what it is given.  ```csharp private readonly Picture _preview = new();  _preview.Show(pixels, width, height); _preview.Draw(region);  ``` |
| [`ProgressBar`](ProgressBar.md) | A filled bar showing how far along something is, with an optional readout beside it. |
| [`ScrollBar`](ScrollBar.md) | The bar down the side of a list that shows how much of it is on screen and where. Drawn only when there is more than fits, so a short list keeps its full width. |
| [`ScrollPane`](ScrollPane.md) | A window onto content taller than the space it has. Lists scroll themselves, but a block of text, a long form or a pane of anything at all does not — this is the widget for those: it draws the content shifted up by the offset, confines it to its own rectangle, and answers the movement keys and the wheel. The content is drawn by a delegate rather than owned, so whatever can paint a region can live in here, including other widgets. |
| [`Sparkline`](Sparkline.md) | A series of numbers as one row of blocks, tallest for the largest of them. It says nothing about what the numbers are — no axis, no scale, no grid — which is what lets it sit in a status bar, a table cell or a corner of a pane and still be read at a glance: the shape of the line is the point. The newest value is the rightmost, and only the last of them fit the row, so a widening terminal shows more history rather than a wider drawing of the same history. |
| [`Spinner`](Spinner.md) | A one-cell animation for work of unknown length. It does not run on its own: something has to step it, which keeps the framework free of timers the application did not ask for. |
| [`StatusBar`](StatusBar.md) | A line of short readouts pinned to an edge. Items are delegates because a status line is redrawn every frame and is expected to show what is true now, not what was true when it was built. |
| [`TableColumn<T>`](TableColumn-1.md) | One column of a table: its heading, what it shows and how it behaves. |
| [`Table<T>`](Table-1.md) | Rows in aligned columns, with a heading and optional sorting. Selection and scrolling are a list box underneath, so a table behaves exactly like a list that happens to draw more per row. Sorting reorders a copy, leaving whatever was assigned to [`Table.Rows`](../arlecchino.widgets/Table-1.md#rows) untouched. |
| [`Tabs`](Tabs.md) | A row of labels where one is current. The widget only tracks which that is; what each tab shows is left to the view, which draws whatever fits the selection. |
| [`TextView`](TextView.md) | A block of text to read: wrapped to the width it is given, scrolled with the movement keys and the wheel. This is the widget for a description, a log, the output of something that ran — anything longer than the space available and not meant to be edited. The text is re-wrapped whenever the width changes, so resizing the terminal reflows it rather than cutting it off. |
| [`TreeNode<T>`](TreeNode-1.md) | One node of a tree. Children are settable so a branch can be filled in when it is first opened rather than up front. |
| [`Tree<T>`](Tree-1.md) | A hierarchy drawn as indented rows. Only the expanded parts are laid out, and that layout is recomputed on demand rather than cached, so nodes may be added or expanded between frames. |

## Structs

| Type | Summary |
|---|---|
| [`GaugeBand`](GaugeBand.md) | Where a band of a [`Gauge`](../arlecchino.widgets/Gauge.md) starts and how it is drawn. A band runs from [`GaugeBand.From`](../arlecchino.widgets/GaugeBand.md#from) up to the start of the next one, so the bands are given in order and the first of them decides the colour of everything below it. |
| [`ScrollWindow`](ScrollWindow.md) | The slice of a long list that fits on screen. Every scrolling widget works this out the same way, so the arithmetic lives here rather than in each of them. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoInteractiveWidget`](IArlecchinoInteractiveWidget.md) | A widget that answers keys and the mouse as well as drawing: a list, a table, a set of tabs, a form. Adding one to a [`FocusRing`](../arlecchino.focus/FocusRing.md) is the whole integration — the ring cycles the focus with `Tab`, hands keys to whichever widget holds it, and moves the focus to the widget that claims a click. The members come from [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md): `IsFocused` for drawing the difference, and `Handle` / `HandleMouse` returning a [`FocusResult`](../arlecchino.focus/FocusResult.md) that says whether the event was claimed and whether it navigates. |
| [`IArlecchinoWidget`](IArlecchinoWidget.md) | A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of its own, so the same widget works in a pane, in a column or across the whole frame. This is the contract every built-in widget answers, and the one to implement for a widget of your own. A widget that also takes keys or the mouse implements [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md) instead. |

