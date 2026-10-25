---
title: "Contrast"
sidebar_label: "Contrast"
---

# Contrast class

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

How far apart two colors read, by the ratio the accessibility guidelines are written in. It runs from 1 for a color on itself to 21 for black on white.

```csharp
public static class Contrast
```

## Fields

| Member | Summary |
|---|---|
| [`Pivot`](#pivot) | The luminance at which a background is as far from white as it is from black. Below it a background is dark and wants light text; above it the other way. |

## Methods

| Member | Summary |
|---|---|
| [`Between(RGB, RGB)`](#between-rgb-rgb) | The ratio between two colors, whichever way round they are given. |
| [`IsDark(RGB)`](#isdark-rgb) | Whether a background wants light text on it. |
| [`Luminance(RGB)`](#luminance-rgb) | How much light a color sends back, weighted the way an eye weights the three channels. |
| [`Reach(RGB)`](#reach-rgb) | The most contrast this background can give in the one direction text goes on it. A mid-gray background reaches about 5, so a ladder written for black cannot be had on it at any lightness. |

## Fields in detail

### `Pivot` {#pivot}

```csharp
public static readonly double Pivot { get; }
```

The luminance at which a background is as far from white as it is from black. Below it a background is dark and wants light text; above it the other way.

**Type** `double`

## Methods in detail

### `Between(RGB, RGB)` {#between-rgb-rgb}

```csharp
public static double Between(RGB one, RGB other);
```

The ratio between two colors, whichever way round they are given.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `one` | [`RGB`](../arlecchino.rendering.colors/RGB.md) | The first color. |
| `other` | [`RGB`](../arlecchino.rendering.colors/RGB.md) | The second color. |

**Returns** `double` — The ratio, from 1 to 21.

### `IsDark(RGB)` {#isdark-rgb}

```csharp
public static bool IsDark(RGB background);
```

Whether a background wants light text on it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`RGB`](../arlecchino.rendering.colors/RGB.md) | The color behind the text. |

**Returns** `bool` — `true` when the background is the darker side of [`Contrast.Pivot`](../arlecchino.rendering.colors/Contrast.md#pivot).

### `Luminance(RGB)` {#luminance-rgb}

```csharp
public static double Luminance(RGB color);
```

How much light a color sends back, weighted the way an eye weights the three channels.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `color` | [`RGB`](../arlecchino.rendering.colors/RGB.md) | The color to measure. |

**Returns** `double` — Luminance, from 0 for black to 1 for white.

### `Reach(RGB)` {#reach-rgb}

```csharp
public static double Reach(RGB background);
```

The most contrast this background can give in the one direction text goes on it. A mid-gray background reaches about 5, so a ladder written for black cannot be had on it at any lightness.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`RGB`](../arlecchino.rendering.colors/RGB.md) | The color behind the text. |

**Returns** `double` — The ratio against white or black, whichever this background is further from.

