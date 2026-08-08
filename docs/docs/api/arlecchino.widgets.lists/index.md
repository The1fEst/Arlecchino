---
title: Arlecchino.Widgets.Lists
sidebar_label: Arlecchino.Widgets.Lists
sidebar_position: 0
---

# Arlecchino.Widgets.Lists

## Classes

| Type | Summary |
|---|---|
| [`ListBox<T>`](ListBox-1.md) | A scrolling list of items, one per row. It keeps only the selected index, never a copy of the items, so replacing [`ListBox.Items`](../arlecchino.widgets.lists/ListBox-1.md#items) between frames is a normal thing to do. |
| [`ScrollBar`](ScrollBar.md) | The bar down the side of a list that shows how much of it is in view and where. Drawn only when there is more than fits, so a short list keeps its full width. |
| [`ScrollPane`](ScrollPane.md) | A window onto content taller than the space it has. Lists scroll themselves, but a block of text, a long form or a pane of anything at all does not. This is the widget for those: it draws the content shifted up by the offset, confines it to its own rectangle, and answers the movement keys and the wheel. The content is drawn by a delegate rather than owned, so whatever can paint a region can live in here, including other widgets. |
| [`TableColumn<T>`](TableColumn-1.md) | One column of a table: its heading, what it shows and how it behaves. |
| [`Table<T>`](Table-1.md) | Rows in aligned columns, with a heading and optional sorting. Selection and scrolling are a list box underneath, so a table behaves exactly like a list that happens to draw more per row. Sorting reorders a copy, leaving whatever was assigned to [`Table.Rows`](../arlecchino.widgets.lists/Table-1.md#rows) untouched. |
| [`Tabs`](Tabs.md) | A row of labels where one is current. The widget only tracks which that is; what each tab shows is left to the view, which draws whatever fits the selection. |
| [`TreeNode<T>`](TreeNode-1.md) | One node of a tree. Children are settable, so a branch can be filled in when it is first opened rather than up front. |
| [`Tree<T>`](Tree-1.md) | A hierarchy drawn as indented rows. Only the expanded parts are laid out, and that layout is recomputed on demand rather than cached, so nodes may be added or expanded between frames. |

## Structs

| Type | Summary |
|---|---|
| [`ScrollWindow`](ScrollWindow.md) | The slice of a long list that fits on screen. Every scrolling widget works this out the same way, so the arithmetic lives here rather than in each of them. |

