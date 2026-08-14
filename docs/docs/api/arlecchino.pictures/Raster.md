---
title: "Raster"
sidebar_label: "Raster"
---

# Raster class

**Namespace:** `Arlecchino.Pictures` &middot; **Assembly:** `Arlecchino.Pictures`

What a picture turned out to hold, ready to be handed to a widget that draws pixels.

```csharp
public sealed class Raster : IEquatable<Raster>
```

**Implements** `IEquatable<T>`&lt;[`Raster`](../arlecchino.pictures/Raster.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Raster(Rgb[], int, int)`](#raster-rgb-int-int) | What a picture turned out to hold, ready to be handed to a widget that draws pixels. |

## Properties

| Member | Summary |
|---|---|
| [`Height`](#height) | How tall it is. |
| [`Pixels`](#pixels) | The pixels, row by row from the top left. |
| [`Width`](#width) | How wide it is. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out Rgb[], out int, out int)`](#deconstruct-out-rgb-out-int-out-int) |  |

## Constructors in detail

### `Raster(Rgb[], int, int)` {#raster-rgb-int-int}

```csharp
public Raster(Rgb[] Pixels, int Width, int Height);
```

What a picture turned out to hold, ready to be handed to a widget that draws pixels.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Pixels` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md)\[\] | The pixels, row by row from the top left. |
| `Width` | `int` | How wide it is. |
| `Height` | `int` | How tall it is. |

## Properties in detail

### `Height` {#height}

```csharp
public int Height { get; init; }
```

How tall it is.

**Type** `int`

### `Pixels` {#pixels}

```csharp
public Rgb[] Pixels { get; init; }
```

The pixels, row by row from the top left.

**Type** [`Rgb`](../arlecchino.rendering.colors/Rgb.md)\[\]

### `Width` {#width}

```csharp
public int Width { get; init; }
```

How wide it is.

**Type** `int`

## Methods in detail

### `Deconstruct(out Rgb[], out int, out int)` {#deconstruct-out-rgb-out-int-out-int}

```csharp
public void Deconstruct(out Rgb[] Pixels, out int Width, out int Height);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Pixels` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md)\[\] |  |
| `Width` | `int` |  |
| `Height` | `int` |  |

