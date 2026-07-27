---
title: Arlecchino.Widgets
sidebar_label: Arlecchino.Widgets
sidebar_position: 0
---

# Arlecchino.Widgets

## Classes

| Type | Summary |
|---|---|
| [`ListBox<T>`](ListBox-1.md) | A scrolling list of items, one per row. It keeps only the selected index, never a copy of the items, so replacing [`ListBox.Items`](../arlecchino.widgets/ListBox-1.md#items) between frames is a normal thing to do. |
| [`ProgressBar`](ProgressBar.md) | A filled bar showing how far along something is, with an optional readout beside it. |
| [`ScrollBar`](ScrollBar.md) | The bar down the side of a list that shows how much of it is on screen and where. Drawn only when there is more than fits, so a short list keeps its full width. |
| [`ScrollPane`](ScrollPane.md) | A window onto content taller than the space it has. Lists scroll themselves, but a block of text, a long form or a pane of anything at all does not — this is the widget for those: it draws the content shifted up by the offset, confines it to its own rectangle, and answers the movement keys and the wheel. The content is drawn by a delegate rather than owned, so whatever can paint a region can live in here, including other widgets. |
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
| [`ScrollWindow`](ScrollWindow.md) | The slice of a long list that fits on screen. Every scrolling widget works this out the same way, so the arithmetic lives here rather than in each of them. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoInteractiveWidget`](IArlecchinoInteractiveWidget.md) | A widget that answers keys and the mouse as well as drawing: a list, a table, a set of tabs, a form. Adding one to a [`FocusRing`](../arlecchino.focus/FocusRing.md) is the whole integration — the ring cycles the focus with `Tab`, hands keys to whichever widget holds it, and moves the focus to the widget that claims a click. The members come from [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md): `IsFocused` for drawing the difference, and `Handle` / `HandleMouse` returning a [`FocusResult`](../arlecchino.focus/FocusResult.md) that says whether the event was claimed and whether it navigates. |
| [`IArlecchinoWidget`](IArlecchinoWidget.md) | A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of its own, so the same widget works in a pane, in a column or across the whole frame. This is the contract every built-in widget answers, and the one to implement for a widget of your own. A widget that also takes keys or the mouse implements [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md) instead. |

