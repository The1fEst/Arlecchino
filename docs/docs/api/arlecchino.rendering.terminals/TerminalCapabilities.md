---
title: "TerminalCapabilities"
sidebar_label: "TerminalCapabilities"
---

# TerminalCapabilities class

**Namespace:** `Arlecchino.Rendering.Terminals` &middot; **Assembly:** `Arlecchino.Core`

What the terminal can actually show. Detected once at startup and consulted by every style when it builds its escape sequence; assign [`TerminalCapabilities.Color`](../arlecchino.rendering.terminals/TerminalCapabilities.md#color) to override the guess.

```csharp
public static class TerminalCapabilities
```

## Properties

| Member | Summary |
|---|---|
| [`Background`](#background) | The color behind the text, as the terminal reported it, or `null` when it did not say. Undrawing a sixel means painting over it, so a picture stays where it is until this is known. |
| [`CellSizeKnown`](#cellsizeknown) | Whether [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth) and [`Glyphs.CellHeight`](../arlecchino.rendering.text/Glyphs.md#cellheight) came from the terminal rather than from the standing guess. Sixel sizing rests on them. |
| [`Color`](#color) | How much color styles may emit, detected on first use. It is process-wide, like [`Theme.Palette`](../arlecchino.rendering.colors/Theme.md#palette), since the framework assumes one terminal per process. |
| [`Kitty`](#kitty) | Whether the terminal answered the kitty graphics query. Set by [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan); assign it to answer for a terminal that will not. |
| [`Sixel`](#sixel) | Whether the terminal said it speaks sixel, as set by [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan). Assign it to answer for a terminal that will not. |

## Methods

| Member | Summary |
|---|---|
| [`DetectColor()`](#detectcolor) | Reads the environment and decides what the terminal can show. |
| [`DetectColor(string, string, string, string)`](#detectcolor-string-string-string-string) | The same decision made from explicit values, which is what makes it testable. `NO_COLOR` or `TERM=dumb` mean no color at all; `truecolor`, `24bit` or a Windows Terminal session mean 24-bit; everything else falls back to the palette. |
| [`NearestPaletteColor(Rgb)`](#nearestpalettecolor-rgb) | Picks the palette color closest to an exact one. This is the conversion [`RgbTermColor`](../arlecchino.rendering.colors/RgbTermColor.md) uses when the terminal cannot do 24-bit, available for your own rendering. |
| [`Resolve(ImageProtocol)`](#resolve-imageprotocol) | Turns [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) into the best of what the terminal admitted to, kitty first, and hands anything else back unchanged. With nothing detected it answers [`ImageProtocol.Blocks`](../arlecchino.rendering.terminals/ImageProtocol.md). |

## Properties in detail

### `Background` {#background}

```csharp
public static Nullable<Rgb> Background { get; set; }
```

The color behind the text, as the terminal reported it, or `null` when it did not say. Undrawing a sixel means painting over it, so a picture stays where it is until this is known.

**Type** `Nullable<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt;

### `CellSizeKnown` {#cellsizeknown}

```csharp
public static bool CellSizeKnown { get; set; }
```

Whether [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth) and [`Glyphs.CellHeight`](../arlecchino.rendering.text/Glyphs.md#cellheight) came from the terminal rather than from the standing guess. Sixel sizing rests on them.

**Type** `bool`

### `Color` {#color}

```csharp
public static ColorSupport Color { get; set; }
```

How much color styles may emit, detected on first use. It is process-wide, like [`Theme.Palette`](../arlecchino.rendering.colors/Theme.md#palette), since the framework assumes one terminal per process.

**Type** [`ColorSupport`](../arlecchino.rendering.colors/ColorSupport.md)

### `Kitty` {#kitty}

```csharp
public static bool Kitty { get; set; }
```

Whether the terminal answered the kitty graphics query. Set by [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan); assign it to answer for a terminal that will not.

**Type** `bool`

### `Sixel` {#sixel}

```csharp
public static bool Sixel { get; set; }
```

Whether the terminal said it speaks sixel, as set by [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan). Assign it to answer for a terminal that will not.

**Type** `bool`

## Methods in detail

### `DetectColor()` {#detectcolor}

```csharp
public static ColorSupport DetectColor();
```

Reads the environment and decides what the terminal can show.

**Returns** [`ColorSupport`](../arlecchino.rendering.colors/ColorSupport.md) — The detected level of color support.

### `DetectColor(string, string, string, string)` {#detectcolor-string-string-string-string}

```csharp
public static ColorSupport DetectColor(
    string? noColor,
    string? term,
    string? colorTerm,
    string? windowsTerminalSession);
```

The same decision made from explicit values, which is what makes it testable. `NO_COLOR` or `TERM=dumb` mean no color at all; `truecolor`, `24bit` or a Windows Terminal session mean 24-bit; everything else falls back to the palette.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `noColor` | `string` | Value of `NO_COLOR`. |
| `term` | `string` | Value of `TERM`. |
| `colorTerm` | `string` | Value of `COLORTERM`. |
| `windowsTerminalSession` | `string` | Value of `WT_SESSION`. |

**Returns** [`ColorSupport`](../arlecchino.rendering.colors/ColorSupport.md) — How much color those values imply the terminal supports.

### `NearestPaletteColor(Rgb)` {#nearestpalettecolor-rgb}

```csharp
public static TerminalColor NearestPaletteColor(Rgb color);
```

Picks the palette color closest to an exact one. This is the conversion [`RgbTermColor`](../arlecchino.rendering.colors/RgbTermColor.md) uses when the terminal cannot do 24-bit, available for your own rendering.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `color` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color to approximate. |

**Returns** [`TerminalColor`](../arlecchino.rendering.colors/TerminalColor.md) — The nearest of the sixteen ANSI colors.

### `Resolve(ImageProtocol)` {#resolve-imageprotocol}

```csharp
public static ImageProtocol Resolve(ImageProtocol protocol);
```

Turns [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) into the best of what the terminal admitted to, kitty first, and hands anything else back unchanged. With nothing detected it answers [`ImageProtocol.Blocks`](../arlecchino.rendering.terminals/ImageProtocol.md).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `protocol` | [`ImageProtocol`](../arlecchino.rendering.terminals/ImageProtocol.md) | What was asked for. |

**Returns** [`ImageProtocol`](../arlecchino.rendering.terminals/ImageProtocol.md) — What to actually draw with.

