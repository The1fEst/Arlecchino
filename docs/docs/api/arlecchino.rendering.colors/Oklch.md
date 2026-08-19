---
title: "Oklch"
sidebar_label: "Oklch"
---

# Oklch struct

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

A color as lightness, chroma and hue, in the space where those three come apart. Moving the lightness of an [`Rgb`](../arlecchino.rendering.colors/Rgb.md) here leaves the color recognizably itself, which moving it in HSL does not.

```csharp
public readonly struct Oklch : IEquatable<Oklch>
```

**Implements** `IEquatable<T>`&lt;[`Oklch`](../arlecchino.rendering.colors/Oklch.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Oklch(double, double, double)`](#oklch-double-double-double) | A color as lightness, chroma and hue, in the space where those three come apart. Moving the lightness of an [`Rgb`](../arlecchino.rendering.colors/Rgb.md) here leaves the color recognizably itself, which moving it in HSL does not. |

## Properties

| Member | Summary |
|---|---|
| [`Chroma`](#chroma) | How far the color stands from gray, up to about 0.4 within `sRGB`. |
| [`FitsScreen`](#fitsscreen) | Whether `sRGB` holds this color, or whether showing it would want channels it has not got. |
| [`Hue`](#hue) | Degrees around the wheel. |
| [`Lightness`](#lightness) | Perceived lightness, 0 for black and 1 for white. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out double, out double, out double)`](#deconstruct-out-double-out-double-out-double) |  |
| [`Of(Rgb)`](#of-rgb) | Splits a color into the three. |
| [`ToRgb()`](#torgb) | The color as the screen shows it, trimmed to `sRGB` on the way. |
| [`ToString()`](#tostring) | Writes the three parts out, which is what a log of a derived palette holds. |
| [`Trimmed()`](#trimmed) | The same lightness and hue with the chroma cut back to the most `sRGB` holds. Cutting the chroma keeps the hue, where clamping the channels turns a color that is too vivid into a different color. |

## Constructors in detail

### `Oklch(double, double, double)` {#oklch-double-double-double}

```csharp
public Oklch(double Lightness, double Chroma, double Hue);
```

A color as lightness, chroma and hue, in the space where those three come apart. Moving the lightness of an [`Rgb`](../arlecchino.rendering.colors/Rgb.md) here leaves the color recognizably itself, which moving it in HSL does not.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Lightness` | `double` | Perceived lightness, 0 for black and 1 for white. |
| `Chroma` | `double` | How far the color stands from gray, up to about 0.4 within `sRGB`. |
| `Hue` | `double` | Degrees around the wheel. |

## Properties in detail

### `Chroma` {#chroma}

```csharp
public double Chroma { get; init; }
```

How far the color stands from gray, up to about 0.4 within `sRGB`.

**Type** `double`

### `FitsScreen` {#fitsscreen}

```csharp
public bool FitsScreen { get; }
```

Whether `sRGB` holds this color, or whether showing it would want channels it has not got.

**Type** `bool`

### `Hue` {#hue}

```csharp
public double Hue { get; init; }
```

Degrees around the wheel.

**Type** `double`

### `Lightness` {#lightness}

```csharp
public double Lightness { get; init; }
```

Perceived lightness, 0 for black and 1 for white.

**Type** `double`

## Methods in detail

### `Deconstruct(out double, out double, out double)` {#deconstruct-out-double-out-double-out-double}

```csharp
public void Deconstruct(out double Lightness, out double Chroma, out double Hue);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Lightness` | `double` |  |
| `Chroma` | `double` |  |
| `Hue` | `double` |  |

### `Of(Rgb)` {#of-rgb}

```csharp
public static Oklch Of(Rgb color);
```

Splits a color into the three.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `color` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color to read. |

**Returns** [`Oklch`](../arlecchino.rendering.colors/Oklch.md) — The same color, said the other way round.

### `ToRgb()` {#torgb}

```csharp
public Rgb ToRgb();
```

The color as the screen shows it, trimmed to `sRGB` on the way.

**Returns** [`Rgb`](../arlecchino.rendering.colors/Rgb.md) — The 24-bit color.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

Writes the three parts out, which is what a log of a derived palette holds.

**Returns** `string` — Lightness, chroma and hue.

### `Trimmed()` {#trimmed}

```csharp
public Oklch Trimmed();
```

The same lightness and hue with the chroma cut back to the most `sRGB` holds. Cutting the chroma keeps the hue, where clamping the channels turns a color that is too vivid into a different color.

**Returns** [`Oklch`](../arlecchino.rendering.colors/Oklch.md) — A color the screen can show.

