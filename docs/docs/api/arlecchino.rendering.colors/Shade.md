---
title: "Shade"
sidebar_label: "Shade"
---

# Shade class

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

Colors worked out against the background they will be read on, so a palette is written as how far apart things should read rather than as a list of colors that only suit one terminal.

```csharp
public static class Shade
```

## Methods

| Member | Summary |
|---|---|
| [`Against(Rgb, double, double, double)`](#against-rgb-double-double-double) | A color of this hue and chroma, as light or as dark as it must be to reach the wanted contrast against the background. The hue is kept whatever happens, and the chroma is cut only to stay in `sRGB`. |
| [`Against(Rgb, Rgb, double)`](#against-rgb-rgb-double) | The same, taking the hue and chroma off a color that already has them. |
| [`Lifted(Rgb, double)`](#lifted-rgb-double) | The background lifted off itself by a step of lightness, which is what a raised surface is. It keeps the background's own hue and chroma, being the same surface raised rather than a different one. |
| [`Pull(Rgb)`](#pull-rgb) | How much a background's own color should sway a design drawn for a gray one. It is nothing behind a terminal theme and everything behind a background someone chose a color for. |
| [`Rise(Rgb)`](#rise-rgb) | Which way surfaces are raised off this background: lighter on a dark terminal, darker on a light one, and the other way where that would take the tallest of them across [`Contrast.Pivot`](../arlecchino.rendering.colors/Contrast.md#pivot). |
| [`Scaled(double, double, double, Rgb)`](#scaled-double-double-double-rgb) | A wanted contrast brought down to what the background can actually give, keeping the order of a ladder that would otherwise flatten. A background near the middle reaches about 5 and no more. |
| [`Turn(Rgb, double, double)`](#turn-rgb-double-double) | The turn to add to every hue of a design so it sits at an angle from a colored background rather than across from it. Turning the whole design at once keeps its colors spaced as they were drawn. |

## Methods in detail

### `Against(Rgb, double, double, double)` {#against-rgb-double-double-double}

```csharp
public static Rgb Against(Rgb background, double hue, double chroma, double contrast);
```

A color of this hue and chroma, as light or as dark as it must be to reach the wanted contrast against the background. The hue is kept whatever happens, and the chroma is cut only to stay in `sRGB`.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color it will be read on. |
| `hue` | `double` | Degrees around the wheel, which the answer keeps. |
| `chroma` | `double` | How vivid to be, cut back where `sRGB` cannot hold it. |
| `contrast` | `double` | The contrast to reach, or as near as the background allows. |

**Returns** [`Rgb`](../arlecchino.rendering.colors/Rgb.md) — The color to write in.

### `Against(Rgb, Rgb, double)` {#against-rgb-rgb-double}

```csharp
public static Rgb Against(Rgb background, Rgb sample, double contrast);
```

The same, taking the hue and chroma off a color that already has them.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color it will be read on. |
| `sample` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color whose hue and chroma to keep. |
| `contrast` | `double` | The contrast to reach. |

**Returns** [`Rgb`](../arlecchino.rendering.colors/Rgb.md) — The color to write in.

### `Lifted(Rgb, double)` {#lifted-rgb-double}

```csharp
public static Rgb Lifted(Rgb background, double step);
```

The background lifted off itself by a step of lightness, which is what a raised surface is. It keeps the background's own hue and chroma, being the same surface raised rather than a different one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The surface to lift off. |
| `step` | `double` | How far, in lightness, a whole surface being about 0.07. |

**Returns** [`Rgb`](../arlecchino.rendering.colors/Rgb.md) — The raised surface.

### `Pull(Rgb)` {#pull-rgb}

```csharp
public static double Pull(Rgb background);
```

How much a background's own color should sway a design drawn for a gray one. It is nothing behind a terminal theme and everything behind a background someone chose a color for.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color behind the text. |

**Returns** `double` — How far to follow the background, from 0 to 1.

### `Rise(Rgb)` {#rise-rgb}

```csharp
public static double Rise(Rgb background);
```

Which way surfaces are raised off this background: lighter on a dark terminal, darker on a light one, and the other way where that would take the tallest of them across [`Contrast.Pivot`](../arlecchino.rendering.colors/Contrast.md#pivot).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color behind the text. |

**Returns** `double` — `1` where a raised surface is the lighter one, `-1` where it is the darker.

### `Scaled(double, double, double, Rgb)` {#scaled-double-double-double-rgb}

```csharp
public static double Scaled(double contrast, double lowest, double highest, Rgb background);
```

A wanted contrast brought down to what the background can actually give, keeping the order of a ladder that would otherwise flatten. A background near the middle reaches about 5 and no more.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `contrast` | `double` | The contrast the design asks for. |
| `lowest` | `double` | The least contrast in the ladder, which is left where it is. |
| `highest` | `double` | The most contrast in the ladder. |
| `background` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color everything is read on. |

**Returns** `double` — The contrast to ask [`Shade.Against`](../arlecchino.rendering.colors/Shade.md#against-rgb-double-double-double) for.

### `Turn(Rgb, double, double)` {#turn-rgb-double-double}

```csharp
public static double Turn(Rgb background, double anchor, double offset);
```

The turn to add to every hue of a design so it sits at an angle from a colored background rather than across from it. Turning the whole design at once keeps its colors spaced as they were drawn.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `background` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color behind the text. |
| `anchor` | `double` | The hue of the design's own accent, which the turn is measured from. |
| `offset` | `double` | How far from the background the accent is to end up. |

**Returns** `double` — Degrees to add to every hue, which is nothing behind a near-neutral terminal.

