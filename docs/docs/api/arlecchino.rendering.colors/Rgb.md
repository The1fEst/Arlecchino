---
title: "Rgb"
sidebar_label: "Rgb"
---

# Rgb struct

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

A 24-bit color. Shown exactly only where the terminal supports true color; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md).

```csharp
public readonly struct Rgb : IEquatable<Rgb>
```

**Implements** `IEquatable<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Rgb(byte, byte, byte)`](#rgb-byte-byte-byte) | A 24-bit color. Shown exactly only where the terminal supports true color; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md). |

## Properties

| Member | Summary |
|---|---|
| [`Blue`](#blue) | Blue channel. |
| [`Green`](#green) | Green channel. |
| [`Hex`](#hex) | The color as `#RRGGBB`. |
| [`Red`](#red) | Red channel. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out byte, out byte, out byte)`](#deconstruct-out-byte-out-byte-out-byte) |  |
| [`FromHsl(int, int, int)`](#fromhsl-int-int-int) | Builds a color from hue, saturation and lightness — the form the color modal edits. |
| [`ToHsl()`](#tohsl) | Splits the color back into hue, saturation and lightness. Channels are whole numbers, so a round trip through [`Rgb.FromHsl`](../arlecchino.rendering.colors/Rgb.md#fromhsl-int-int-int) can shift a color by a unit or two. |
| [`ToString()`](#tostring) | Writes the color as its hexadecimal form. |
| [`TryParseHex(string, out Rgb)`](#tryparsehex-string-out-rgb) | Reads a color written as `#RRGGBB` or `RRGGBB`. |

## Constructors in detail

### `Rgb(byte, byte, byte)` {#rgb-byte-byte-byte}

```csharp
public Rgb(byte Red, byte Green, byte Blue);
```

A 24-bit color. Shown exactly only where the terminal supports true color; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Red` | `byte` | Red channel. |
| `Green` | `byte` | Green channel. |
| `Blue` | `byte` | Blue channel. |

## Properties in detail

### `Blue` {#blue}

```csharp
public byte Blue { get; init; }
```

Blue channel.

**Type** `byte`

### `Green` {#green}

```csharp
public byte Green { get; init; }
```

Green channel.

**Type** `byte`

### `Hex` {#hex}

```csharp
public string Hex { get; }
```

The color as `#RRGGBB`.

**Type** `string`

### `Red` {#red}

```csharp
public byte Red { get; init; }
```

Red channel.

**Type** `byte`

## Methods in detail

### `Deconstruct(out byte, out byte, out byte)` {#deconstruct-out-byte-out-byte-out-byte}

```csharp
public void Deconstruct(out byte Red, out byte Green, out byte Blue);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Red` | `byte` |  |
| `Green` | `byte` |  |
| `Blue` | `byte` |  |

### `FromHsl(int, int, int)` {#fromhsl-int-int-int}

```csharp
public static Rgb FromHsl(int hue, int saturation, int lightness);
```

Builds a color from hue, saturation and lightness — the form the color modal edits.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `hue` | `int` | Degrees around the wheel; values outside `0..359` wrap. |
| `saturation` | `int` | Percent, clamped to `0..100`. |
| `lightness` | `int` | Percent, clamped to `0..100`. |

**Returns** [`Rgb`](../arlecchino.rendering.colors/Rgb.md) — The matching color.

### `ToHsl()` {#tohsl}

```csharp
public ValueTuple<int, int, int> ToHsl();
```

Splits the color back into hue, saturation and lightness. Channels are whole numbers, so a round trip through [`Rgb.FromHsl`](../arlecchino.rendering.colors/Rgb.md#fromhsl-int-int-int) can shift a color by a unit or two.

**Returns** `ValueTuple<T1, T2, T3>`&lt;`int`, `int`, `int`&gt; — Hue in degrees, saturation and lightness in percent.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

Writes the color as its hexadecimal form.

**Returns** `string` — [`Rgb.Hex`](../arlecchino.rendering.colors/Rgb.md#hex).

### `TryParseHex(string, out Rgb)` {#tryparsehex-string-out-rgb}

```csharp
public static bool TryParseHex(string text, out Rgb color);
```

Reads a color written as `#RRGGBB` or `RRGGBB`.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to read. |
| `color` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color, or `default` when the text is not six hex digits. |

**Returns** `bool` — `true` when the text was a color.

