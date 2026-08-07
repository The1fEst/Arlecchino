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
| `Blocks` | `0` | Cells, two pixels to each: the color of the upper half block and the background behind it. Coarse, but it needs nothing of the terminal beyond the color it already draws in, works through the ordinary frame diff, and leaves nothing behind to clean up. |
| `Kitty` | `1` | The kitty graphics protocol: the pixels themselves, sent as they are. Kitty, WezTerm and Ghostty speak it; a terminal that does not will show the escape sequence as text, which is why this is asked for rather than assumed. |
| `Sixel` | `2` | Sixel: the older protocol, and the one Windows Terminal, xterm, foot and WezTerm speak. Color comes down to a fixed cube of 216, which is what the format keeps registers for, and the picture is measured in pixels rather than cells — so [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth) and [`Glyphs.CellHeight`](../arlecchino.rendering.text/Glyphs.md#cellheight) say how large a cell is taken to be. |
| `Auto` | `3` | The best of what the terminal admitted to when it was asked — see [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan) — and [`ImageProtocol.Blocks`](../arlecchino.rendering.terminals/ImageProtocol.md) when it admitted to nothing, so a picture appears either way. Kitty is preferred over sixel where both are on offer. This is the default, because the alternative is an application choosing on a terminal it cannot see. Name a protocol instead when you would rather decide, or when the terminal is one that lies. |

