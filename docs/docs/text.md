---
title: Text and width
sidebar_label: Text and width
description: Why the surface measures text in terminal columns rather than in char values, and the TextWidth calls that do it.
---

# Text and width

A terminal cell is not a `char`. CJK and emoji occupy two columns, combining marks occupy none, a
surrogate pair is one symbol made of two `char` values, and a flag emoji is one symbol made of four.
`string.Length` answers none of the questions a layout asks.

So a cell on the [surface](rendering.md) holds a whole **grapheme cluster** — one symbol as a reader
would count it — and every measurement the framework makes is in **terminal columns**. Every flow and
absolute call clips, aligns and pads on that measure, which is why a box drawn around `日本語` closes
where it should.

## TextWidth

`TextWidth` is that measure, public for your own layout code.

### Measuring

| Call | Meaning |
|---|---|
| `Of(text)` | Width in columns |
| `CountClusters(text)` | How many symbols the text is made of, as a reader would count them |
| `OfCluster(span)` | Width of one grapheme cluster |
| `OfRune(rune)` | Width of one code point, before combining marks are taken into account |

### Cutting

| Call | Meaning |
|---|---|
| `Truncate(text, width)` | Cuts to a column width on a symbol boundary |
| `TruncateStart(text, width)` | The same from the other end, keeping the tail — what a field scrolled to the right shows |
| `Wrap(text, width)` | Breaks into lines that fit, at spaces where there is one and mid-word only when a single word is wider than the space; existing line breaks are kept |

### Padding

| Call | Meaning |
|---|---|
| `PadRight(text, width)` | Pads on the right, left-aligning in that width |
| `PadLeft(text, width)` | Pads on the left, right-aligning in that width |

### Walking

These are what a text field uses to move a caret without cutting a symbol in half.

| Call | Meaning |
|---|---|
| `NextClusterLength(text, index)` | How many `char` values the next symbol occupies |
| `NextClusterEnd(text, index)` | Where the symbol at an index ends — what a forward delete removes |
| `PreviousClusterStart(text, index)` | Where the symbol before an index starts — what a backspace removes |
| `SnapToCluster(text, index)` | Pulls a position back to the start of the symbol it lands in |

## Use them instead of the obvious thing

| Instead of | Use |
|---|---|
| `text.Length` | `TextWidth.Of(text)` |
| `text.PadRight(n)` | `TextWidth.PadRight(text, n)` |
| `text[..n]` | `TextWidth.Truncate(text, n)` |
| `caret - 1` on backspace | `TextWidth.PreviousClusterStart(text, caret)` |

Anywhere the result lands on screen, the column measure is the one that matters. The framework's own
widgets, modals and text editing are written against these calls, and
[`TextEditing`](modals.md#editing-a-line) is the shared implementation they share.

## What the surface does with a wide symbol

A wide symbol occupies two cells. Writing over either half clears the other, so a narrow character
cannot be left sitting beside an orphaned half. One that would be split by the right edge is dropped
rather than half-drawn.

## Styles

`TextStyle` is a `[Flags]` enum carried by every [style](colours.md):

| Flag | Note |
|---|---|
| `None` | No attributes |
| `Bold` | Some terminals render it as a brighter colour instead |
| `Italic` | The least widely supported of the four |
| `Underline` | |
| `Dim` | The opposite of `Bold` |

They combine: `TextStyle.Bold | TextStyle.Underline`.
