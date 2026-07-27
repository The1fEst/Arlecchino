---
title: Arlecchino.Rendering
sidebar_label: Arlecchino.Rendering
sidebar_position: 0
---

# Arlecchino.Rendering

## Classes

| Type | Summary |
|---|---|
| [`RgbTermColor`](RgbTermColor.md) | A style built from exact colours. Use it where the colour itself is the point — a swatch, a chart, syntax highlighting — and keep chrome on [`Theme`](../arlecchino.rendering/Theme.md), which follows the terminal theme. Falls back to the nearest palette colour when the terminal cannot do 24-bit. |
| [`Surface`](Surface.md) | The drawing target: a grid of cells, each holding one symbol and one style, serialized into a single write per frame. Needs nothing but an [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md), so it works outside a hosted application too. |
| [`TermColor`](TermColor.md) | A style built from the sixteen-colour palette. This is what the roles on [`Theme`](../arlecchino.rendering/Theme.md) are made of and what chrome should use, because those colours follow the terminal's own theme. |
| [`TerminalCapabilities`](TerminalCapabilities.md) | What the terminal can actually show. Detected once at startup and consulted by every style when it builds its escape sequence; assign [`TerminalCapabilities.Color`](../arlecchino.rendering/TerminalCapabilities.md#color) to override the guess. |
| [`TextWidth`](TextWidth.md) | Measures text the way a terminal shows it: in columns, not in `char` values. CJK and emoji take two columns, combining marks take none, and a surrogate pair is one symbol. Use these instead of `string.Length`, `PadRight` and slicing whenever the result lands on screen. |
| [`Theme`](Theme.md) | The palette in use, reachable from anywhere that draws. Views pick a role here rather than a colour, so swapping [`Theme.Palette`](../arlecchino.rendering/Theme.md#palette) restyles the whole application, chrome included. |
| [`ThemePalette`](ThemePalette.md) | The colours behind the roles in [`Theme`](../arlecchino.rendering/Theme.md). Every role has a default, so a palette that overrides two of them is a valid palette — and what it does not override is the framework's own colours, described on [`ThemePalette.Arlecchino`](../arlecchino.rendering/ThemePalette.md#arlecchino). |

## Structs

| Type | Summary |
|---|---|
| [`Margin`](Margin.md) | Blank space around something, measured in cells. Used both by the flow calls on [`Surface`](../arlecchino.rendering/Surface.md) and by [`SurfaceRegion.Inset`](../arlecchino.rendering/SurfaceRegion.md#inset-margin). |
| [`Rgb`](Rgb.md) | A 24-bit colour. Shown exactly only where the terminal supports true colour; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering/TerminalCapabilities.md). |
| [`SurfaceRegion`](SurfaceRegion.md) | A rectangle on a [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) with its own coordinates and its own clipping: writing outside it is dropped rather than spilled onto a neighbour. Split a frame into regions instead of counting columns by hand, and the same geometry answers "was this click inside". |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoColor`](IArlecchinoColor.md) | Anything that can style a cell. The frame writer only ever asks for [`IArlecchinoColor.Ansi`](../arlecchino.rendering/IArlecchinoColor.md#ansi) and compares styles by reference, so hold on to instances instead of building one per cell. |

## Enums

| Type | Summary |
|---|---|
| [`Align`](Align.md) | Where text or a block sits inside the space it is drawn into. Horizontal and vertical flags combine, so `Align.Right \| Align.Bottom` anchors to a corner. |
| [`ColorSupport`](ColorSupport.md) | How much colour the terminal can show. Detected once at startup by [`TerminalCapabilities.DetectColor`](../arlecchino.rendering/TerminalCapabilities.md#detectcolor) and used by every style when it builds its escape sequence. |
| [`TerminalColor`](TerminalColor.md) | The sixteen ANSI colours plus the terminal's own default. Exact shades belong to the terminal theme, which is why chrome should pick a role from [`Theme`](../arlecchino.rendering/Theme.md) rather than a colour here. |
| [`TextStyle`](TextStyle.md) | Text attributes a style carries on top of its colours. Combine them with `\|`; a terminal that does not support one simply ignores it. |

