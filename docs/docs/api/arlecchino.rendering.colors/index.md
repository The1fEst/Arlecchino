---
title: Arlecchino.Rendering.Colors
sidebar_label: Arlecchino.Rendering.Colors
sidebar_position: 0
---

# Arlecchino.Rendering.Colors

## Classes

| Type | Summary |
|---|---|
| [`RgbTermColor`](RgbTermColor.md) | A style built from exact colors, for where the color itself is the point rather than a role in [`Theme`](../arlecchino.rendering.colors/Theme.md). It falls back to the nearest palette color where the terminal cannot do 24-bit. |
| [`TermColor`](TermColor.md) | A style built from the sixteen-color palette. This is what the roles on [`Theme`](../arlecchino.rendering.colors/Theme.md) are made of and what chrome should use, because those colors follow the terminal's own theme. |
| [`Theme`](Theme.md) | The palette in use, reachable from anywhere that draws. Views pick a role here rather than a color, so swapping [`Theme.Palette`](../arlecchino.rendering.colors/Theme.md#palette) restyles the whole application, chrome included. |
| [`ThemePalette`](ThemePalette.md) | The colors behind the roles in [`Theme`](../arlecchino.rendering.colors/Theme.md). Every role has a default, so a palette that overrides two of them is a valid palette — and what it does not override is the framework's own colors, described on [`ThemePalette.Arlecchino`](../arlecchino.rendering.colors/ThemePalette.md#arlecchino). |

## Structs

| Type | Summary |
|---|---|
| [`Rgb`](Rgb.md) | A 24-bit color. Shown exactly only where the terminal supports true color; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md). |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoColor`](IArlecchinoColor.md) | Anything that can style a cell. The frame writer only ever asks for [`IArlecchinoColor.Ansi`](../arlecchino.rendering.colors/IArlecchinoColor.md#ansi) and compares styles by reference, so hold on to instances instead of building one per cell. |

## Enums

| Type | Summary |
|---|---|
| [`ColorSupport`](ColorSupport.md) | How much color the terminal can show. Detected once at startup by [`TerminalCapabilities.DetectColor`](../arlecchino.rendering.terminals/TerminalCapabilities.md#detectcolor) and used by every style when it builds its escape sequence. |
| [`TerminalColor`](TerminalColor.md) | The sixteen ANSI colors plus the terminal's own default. Exact shades belong to the terminal theme, so chrome picks a role from [`Theme`](../arlecchino.rendering.colors/Theme.md) rather than a color here. |
| [`TextStyle`](TextStyle.md) | Text attributes a style carries on top of its colors. Combine them with `\|`; a terminal that does not support one simply ignores it. |

