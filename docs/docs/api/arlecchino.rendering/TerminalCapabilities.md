---
title: TerminalCapabilities
sidebar_label: TerminalCapabilities
---

# TerminalCapabilities class

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

What the terminal can actually show. Detected once at startup and consulted by every style when it builds its escape sequence; assign [`TerminalCapabilities.Color`](../arlecchino.rendering/TerminalCapabilities.md#color) to override the guess.

```csharp
public static class TerminalCapabilities
```

## Properties

| Member | Summary |
|---|---|
| [`Color`](#color) | How much colour styles may emit. Detected on first use; a terminal that refuses virtual terminal mode lowers it to [`ColorSupport.None`](../arlecchino.rendering/ColorSupport.md) at startup. Process-wide, like [`Theme.Palette`](../arlecchino.rendering/Theme.md#palette): one terminal per process is the assumption the framework makes, and tests that change this share it with everything else running. |

## Methods

| Member | Summary |
|---|---|
| [`DetectColor()`](#detectcolor) | Reads the environment and decides what the terminal can show. |
| [`DetectColor(string, string, string, string)`](#detectcolor-string-string-string-string) | The same decision made from explicit values, which is what makes it testable. `NO_COLOR` or `TERM=dumb` mean no colour at all; `truecolor`, `24bit` or a Windows Terminal session mean 24-bit; everything else falls back to the palette. |
| [`NearestPaletteColor(Rgb)`](#nearestpalettecolor-rgb) | Picks the palette colour closest to an exact one. This is the conversion [`RgbTermColor`](../arlecchino.rendering/RgbTermColor.md) uses when the terminal cannot do 24-bit, available for your own rendering. |

## Properties in detail

### `Color` {#color}

```csharp
public static ColorSupport Color { get; set; }
```

How much colour styles may emit. Detected on first use; a terminal that refuses virtual terminal mode lowers it to [`ColorSupport.None`](../arlecchino.rendering/ColorSupport.md) at startup. Process-wide, like [`Theme.Palette`](../arlecchino.rendering/Theme.md#palette): one terminal per process is the assumption the framework makes, and tests that change this share it with everything else running.

**Type** [`ColorSupport`](../arlecchino.rendering/ColorSupport.md)

## Methods in detail

### `DetectColor()` {#detectcolor}

```csharp
public static ColorSupport DetectColor();
```

Reads the environment and decides what the terminal can show.

**Returns** [`ColorSupport`](../arlecchino.rendering/ColorSupport.md) — The detected level of colour support.

### `DetectColor(string, string, string, string)` {#detectcolor-string-string-string-string}

```csharp
public static ColorSupport DetectColor(
    string? noColor,
    string? term,
    string? colorTerm,
    string? windowsTerminalSession);
```

The same decision made from explicit values, which is what makes it testable. `NO_COLOR` or `TERM=dumb` mean no colour at all; `truecolor`, `24bit` or a Windows Terminal session mean 24-bit; everything else falls back to the palette.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `noColor` | `string` | Value of `NO_COLOR`. |
| `term` | `string` | Value of `TERM`. |
| `colorTerm` | `string` | Value of `COLORTERM`. |
| `windowsTerminalSession` | `string` | Value of `WT_SESSION`. |

**Returns** [`ColorSupport`](../arlecchino.rendering/ColorSupport.md) — The level of colour support those values imply.

### `NearestPaletteColor(Rgb)` {#nearestpalettecolor-rgb}

```csharp
public static TerminalColor NearestPaletteColor(Rgb color);
```

Picks the palette colour closest to an exact one. This is the conversion [`RgbTermColor`](../arlecchino.rendering/RgbTermColor.md) uses when the terminal cannot do 24-bit, available for your own rendering.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `color` | [`Rgb`](../arlecchino.rendering/Rgb.md) | The colour to approximate. |

**Returns** [`TerminalColor`](../arlecchino.rendering/TerminalColor.md) — The nearest of the sixteen ANSI colours.

