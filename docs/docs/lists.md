---
title: Lists
sidebar_label: ListBox
description: ListBox — a scrolling, selectable, clickable list of anything, and the widget the table and the tree are built on.
---

# ListBox

```csharp
private readonly ListBox<string> _authors;
private readonly string _mine = "fEst";

_authors = new ListBox<string>(options.Keymap)
{
    Render = author => $" {author}",
    ItemStyle = author => author == _mine ? Theme.Active : Theme.Default,
    OnActivate = author => ViewKind.Author,
    Items = authors,
};

_authors.Draw(region);
```

| Member | Meaning |
|---|---|
| `Items` | The rows, read fresh every frame |
| `Render` | Turns an item into the line that is drawn |
| `ItemStyle` | Optional per-row style |
| `OnActivate` | Runs on `Confirm` or a second click; returns a route |
| `Selected` / `SelectedIndex` | Where the cursor is |
| `IsFocused` | Set by the [focus ring](focus.md) |

## Keys and clicks

| Input | Does |
|---|---|
| `↑` / `↓` | Move one row |
| `PgUp` / `PgDn` | Jump ten rows |
| `Home` / `End` | Go to the ends |
| `Confirm` | Activates the selected row |
| Wheel | Scrolls |
| Click | Selects the row |
| Second click on the selected row | Activates it |

All of them come from the [keymap](keyboard.md#the-keymap).

## Focused and unfocused

The selected row is drawn `ActiveSelected` while the list has the focus and `Selected` while it does
not, so a list beside another pane still shows where its cursor is. Those two
[roles](theming.md#roles) are chosen so that neither reads as an error.

## Scrolling

Only the visible slice is rendered — [`ScrollWindow.Around`](scrolling.md#scrollwindow) is the same
helper the widget uses, available for lists you draw yourself.

A list with more items than rows grows a [scroll bar](scrolling.md#scrollbar) down its last column, and
the rows are truncated one cell earlier to make room rather than being covered by it. A list that fits
keeps its full width and shows nothing.

[Choice and multi-choice modals](modals.md#choice) get the same treatment: a bar beside the options and
a `3/40` readout on the filter line, worded by `Strings.ListPosition`.

## Where the rows come from

`Items` is read while the frame is drawn, so the collection behind it belongs to the drawing thread.
Change it from a view, a command or a callback, and hand changes that arrive from anywhere else to
[`FrameThread.Post`](frame-loop.md#coming-back-from-a-background-task).

## What is built on it

[`Table<T>`](table.md) and [`Tree<T>`](tree.md) both hold a `ListBox` and hand it the rows they
computed, so movement, clicks, activation and the scroll bar are the same in all three.
