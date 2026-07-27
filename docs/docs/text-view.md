---
title: Text view
sidebar_label: TextView
description: TextView — a block of text wrapped to the width it is given, cached until the text or the width changes.
---

# TextView

A block of text to read: wrapped to the width it is given, scrolled the same way a
[`ScrollPane`](scrolling.md) is — because it is one inside.

```csharp
_readme = new TextView(options.Keymap) { Text = File.ReadAllText(path) };

_readme.Draw(region);
```

| Member | Meaning |
|---|---|
| `Text` | What to show |
| `Style` | Colours it |
| `Offset` | The first wrapped line shown |
| `LineCount` | How many rows it takes once wrapped |
| `IsFocused` | Set by the [focus ring](focus.md) |

## Wrapping

Line breaks in the text are kept, long lines break on spaces, and a word wider than the pane is split
rather than lost — the same rules as [`TextWidth.Wrap`](text.md#cutting), which is what does it.

The wrap is cached and redone only when the text or the width changed, so resizing the terminal
reflows the text instead of cutting it off, and a frame that changed neither costs nothing.

## Keys

`↑↓` move a row, `PgUp`/`PgDn` a page, `Home`/`End` go to the ends, and the wheel scrolls while the
pointer is over it — all from the [keymap](keyboard.md#the-keymap), through the pane inside.

## Reading rather than editing

`TextView` shows text; it does not edit it. Editing several lines is the
[multi-line dialog](modals.md#several-lines-of-text), which is a modal rather than a widget because it
has a caret, a validator and a submit key of its own.

## Measuring

`LineCount` is how many lines the text takes once wrapped **to the last width it was drawn at**, so it
answers questions about the frame that has already been composed rather than the one being composed
now. It is what a status bar reports a position against:

```csharp
new StatusBar
{
    Right = [() => $"{_readme.Offset + 1}/{_readme.LineCount}"],
}.Draw(region.Rows(region.Height - 1, 1));
```

To split a pane between the text and something else, decide the split from the region rather than from
the wrap:

```csharp
private readonly TextView _readme;
private readonly ListBox<string> _files;

var (text, rest) = region.SplitTop(region.Height / 2);

_readme.Draw(text);
_files.Draw(rest);
```
