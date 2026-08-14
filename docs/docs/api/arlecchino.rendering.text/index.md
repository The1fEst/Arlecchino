---
title: Arlecchino.Rendering.Text
sidebar_label: Arlecchino.Rendering.Text
sidebar_position: 0
---

# Arlecchino.Rendering.Text

## Classes

| Type | Summary |
|---|---|
| [`Glyphs`](Glyphs.md) | The symbols in use, reachable from anywhere that draws, the way [`Theme`](../arlecchino.rendering.colors/Theme.md) is. It is written on the drawing thread and asks for a frame itself, so every graph follows on the next one. |
| [`Joinery`](Joinery.md) | Lines that know about one another. Boxes and rules are recorded first and painted at the end, so a cell two of them share becomes the glyph that joins them rather than one line drawn over the other.  ```csharp var joinery = new Joinery();  var files = joinery.Box(left, Theme.Info, "files"); var log = joinery.Box(right, Theme.Active, "log");  joinery.Draw(surface.Content, Theme.Info);  ``` |
| [`TextWidth`](TextWidth.md) | Measures text the way a terminal shows it, in columns rather than in `char` values. Use these instead of `string.Length`, `PadRight` and slicing wherever the result lands on screen. |

## Enums

| Type | Summary |
|---|---|
| [`GraphSymbols`](GraphSymbols.md) | Which characters a graph is drawn with. The choice is about the font the terminal was given rather than about taste: the denser the symbols, the more of them a font has to carry. |

