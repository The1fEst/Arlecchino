---
title: Rgb
sidebar_label: Rgb
---

# Rgb struct

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

A 24-bit colour. Shown exactly only where the terminal supports true colour; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering/TerminalCapabilities.md).

```csharp
public readonly struct Rgb : IEquatable<Rgb>
```

**Implements** `IEquatable<T>`&lt;[`Rgb`](../arlecchino.rendering/Rgb.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Rgb(byte, byte, byte)`](#rgb-byte-byte-byte) | A 24-bit colour. Shown exactly only where the terminal supports true colour; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering/TerminalCapabilities.md). |

## Properties

| Member | Summary |
|---|---|
| [`Blue`](#blue) | Blue channel. |
| [`Green`](#green) | Green channel. |
| [`Hex`](#hex) | The colour as `#RRGGBB`. |
| [`Red`](#red) | Red channel. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(Byte&, Byte&, Byte&)`](#deconstruct-byte-byte-byte) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(Rgb)`](#equals-rgb) |  |
| [`FromHsl(int, int, int)`](#fromhsl-int-int-int) | Builds a colour from hue, saturation and lightness — the form the colour modal edits. |
| [`GetHashCode()`](#gethashcode) |  |
| [`ToHsl()`](#tohsl) | Splits the colour back into hue, saturation and lightness. Channels are whole numbers, so a round trip through [`Rgb.FromHsl`](../arlecchino.rendering/Rgb.md#fromhsl-int-int-int) can shift a colour by a unit or two. |
| [`ToString()`](#tostring) | Returns [`Rgb.Hex`](../arlecchino.rendering/Rgb.md#hex). |
| [`TryParseHex(string, Rgb&)`](#tryparsehex-string-rgb) | Reads a colour written as `#RRGGBB` or `RRGGBB`. |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(Rgb, Rgb)`](#operator-inequality-rgb-rgb) |  |
| [`operator Equality(Rgb, Rgb)`](#operator-equality-rgb-rgb) |  |

## Constructors in detail

### `Rgb(byte, byte, byte)` {#rgb-byte-byte-byte}

```csharp
public Rgb(byte Red, byte Green, byte Blue);
```

A 24-bit colour. Shown exactly only where the terminal supports true colour; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering/TerminalCapabilities.md).

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

The colour as `#RRGGBB`.

**Type** `string`

### `Red` {#red}

```csharp
public byte Red { get; init; }
```

Red channel.

**Type** `byte`

## Methods in detail

### `Deconstruct(Byte&, Byte&, Byte&)` {#deconstruct-byte-byte-byte}

```csharp
public void Deconstruct(out byte Red, out byte Green, out byte Blue);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Red` | `byte` |  |
| `Green` | `byte` |  |
| `Blue` | `byte` |  |

### `Equals(object)` {#equals-object}

```csharp
public virtual bool Equals(object obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(Rgb)` {#equals-rgb}

```csharp
public bool Equals(Rgb other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`Rgb`](../arlecchino.rendering/Rgb.md) |  |

**Returns** `bool`

### `FromHsl(int, int, int)` {#fromhsl-int-int-int}

```csharp
public static Rgb FromHsl(int hue, int saturation, int lightness);
```

Builds a colour from hue, saturation and lightness — the form the colour modal edits.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `hue` | `int` | Degrees around the wheel; values outside 0..359 wrap. |
| `saturation` | `int` | Percent, clamped to 0..100. |
| `lightness` | `int` | Percent, clamped to 0..100. |

**Returns** [`Rgb`](../arlecchino.rendering/Rgb.md) — The matching colour.

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

**Returns** `int`

### `ToHsl()` {#tohsl}

```csharp
public ValueTuple<int, int, int> ToHsl();
```

Splits the colour back into hue, saturation and lightness. Channels are whole numbers, so a round trip through [`Rgb.FromHsl`](../arlecchino.rendering/Rgb.md#fromhsl-int-int-int) can shift a colour by a unit or two.

**Returns** `ValueTuple<T1, T2, T3>`&lt;`int`, `int`, `int`&gt; — Hue in degrees, saturation and lightness in percent.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

Returns [`Rgb.Hex`](../arlecchino.rendering/Rgb.md#hex).

**Returns** `string`

### `TryParseHex(string, Rgb&)` {#tryparsehex-string-rgb}

```csharp
public static bool TryParseHex(string text, out Rgb color);
```

Reads a colour written as `#RRGGBB` or `RRGGBB`.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to read. |
| `color` | [`Rgb`](../arlecchino.rendering/Rgb.md) | The colour, or `default` when the text is not six hex digits. |

**Returns** `bool` — `true` when the text was a colour.

## Operators in detail

### `operator Inequality(Rgb, Rgb)` {#operator-inequality-rgb-rgb}

```csharp
public static bool op_Inequality(Rgb left, Rgb right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`Rgb`](../arlecchino.rendering/Rgb.md) |  |
| `right` | [`Rgb`](../arlecchino.rendering/Rgb.md) |  |

**Returns** `bool`

### `operator Equality(Rgb, Rgb)` {#operator-equality-rgb-rgb}

```csharp
public static bool op_Equality(Rgb left, Rgb right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`Rgb`](../arlecchino.rendering/Rgb.md) |  |
| `right` | [`Rgb`](../arlecchino.rendering/Rgb.md) |  |

**Returns** `bool`

