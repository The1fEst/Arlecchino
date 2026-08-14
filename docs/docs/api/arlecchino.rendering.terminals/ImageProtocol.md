---
title: "ImageProtocol"
sidebar_label: "ImageProtocol"
---

# ImageProtocol enum

**Namespace:** `Arlecchino.Rendering.Terminals` &middot; **Assembly:** `Arlecchino.Core`

How a picture reaches the terminal. Like [`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md), this is a question of what the terminal can do rather than of taste.

```csharp
public enum ImageProtocol
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Blocks` | `0` | Cells, two pixels to each: the color of the upper half block and the background behind it. It needs nothing of the terminal beyond color and goes through the ordinary frame diff. |
| `Kitty` | `1` | The kitty graphics protocol: the pixels themselves, spoken by kitty, WezTerm and Ghostty. A terminal that does not speak it shows the escape sequence as text. |
| `Sixel` | `2` | Sixel: the older protocol, spoken by Windows Terminal, xterm, foot and WezTerm. Color comes down to a cube of 216, and [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth) says how large a cell is taken to be. |
| `Auto` | `3` | The best of what the terminal admitted to when [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan) asked, preferring kitty over sixel, and [`ImageProtocol.Blocks`](../arlecchino.rendering.terminals/ImageProtocol.md) when it admitted to nothing. It is the default. |

