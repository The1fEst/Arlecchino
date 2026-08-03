---
title: Arlecchino.Rendering.Colors
sidebar_label: Arlecchino.Rendering.Colors
sidebar_position: 0
---

# Arlecchino.Rendering.Colors

## Classes

| Type | Summary |
|---|---|
| [`RgbTermColor`](RgbTermColor.md) | A style built from exact colours. Use it where the colour itself is the point — a swatch, a chart, syntax highlighting — and keep chrome on [`Theme`](../arlecchino.rendering.colors/Theme.md), which follows the terminal theme. Falls back to the nearest palette colour when the terminal cannot do 24-bit. |
| [`TermColor`](TermColor.md) | A style built from the sixteen-colour palette. This is what the roles on [`Theme`](../arlecchino.rendering.colors/Theme.md) are made of and what chrome should use, because those colours follow the terminal's own theme. |
| [`Theme`](Theme.md) | The palette in use, reachable from anywhere that draws. Views pick a role here rather than a colour, so swapping [`Theme.Palette`](../arlecchino.rendering.colors/Theme.md#palette) restyles the whole application, chrome included. |
| [`ThemePalette`](ThemePalette.md) | The colours behind the roles in [`Theme`](../arlecchino.rendering.colors/Theme.md). Every role has a default, so a palette that overrides two of them is a valid palette — and what it does not override is the framework's own colours, described on [`ThemePalette.Arlecchino`](../arlecchino.rendering.colors/ThemePalette.md#arlecchino). |

## Structs

| Type | Summary |
|---|---|
| [`Rgb`](Rgb.md) | A 24-bit colour. Shown exactly only where the terminal supports true colour; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md). |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoColor`](IArlecchinoColor.md) | Anything that can style a cell. The frame writer only ever asks for [`IArlecchinoColor.Ansi`](../arlecchino.rendering.colors/IArlecchinoColor.md#ansi) and compares styles by reference, so hold on to instances instead of building one per cell. |

## Enums

| Type | Summary |
|---|---|
| [`ColorSupport`](ColorSupport.md) | How much colour the terminal can show. Detected once at startup by [`TerminalCapabilities.DetectColor`](../arlecchino.rendering.terminals/TerminalCapabilities.md#detectcolor) and used by every style when it builds its escape sequence. |
| [`TerminalColor`](TerminalColor.md) | The sixteen ANSI colours plus the terminal's own default. Exact shades belong to the terminal theme, which is why chrome should pick a role from [`Theme`](../arlecchino.rendering.colors/Theme.md) rather than a colour here. |
| [`TextStyle`](TextStyle.md) | Text attributes a style carries on top of its colours. Combine them with `\|`; a terminal that does not support one simply ignores it. |

