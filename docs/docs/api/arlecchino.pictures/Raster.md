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
| [`Raster(RGB[], int, int)`](#raster-rgb-int-int) | What a picture turned out to hold, ready to be handed to a widget that draws pixels. |

## Properties

| Member | Summary |
|---|---|
| [`Height`](#height) | How tall it is. |
| [`Pixels`](#pixels) | The pixels, row by row from the top left. |
| [`Width`](#width) | How wide it is. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out RGB[], out int, out int)`](#deconstruct-out-rgb-out-int-out-int) |  |

## Constructors in detail

### `Raster(RGB[], int, int)` {#raster-rgb-int-int}

```csharp
public Raster(RGB[] Pixels, int Width, int Height);
```

What a picture turned out to hold, ready to be handed to a widget that draws pixels.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Pixels` | [`RGB`](../arlecchino.rendering.colors/RGB.md)\[\] | The pixels, row by row from the top left. |
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
public RGB[] Pixels { get; init; }
```

The pixels, row by row from the top left.

**Type** [`RGB`](../arlecchino.rendering.colors/RGB.md)\[\]

### `Width` {#width}

```csharp
public int Width { get; init; }
```

How wide it is.

**Type** `int`

## Methods in detail

### `Deconstruct(out RGB[], out int, out int)` {#deconstruct-out-rgb-out-int-out-int}

```csharp
public void Deconstruct(out RGB[] Pixels, out int Width, out int Height);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Pixels` | [`RGB`](../arlecchino.rendering.colors/RGB.md)\[\] |  |
| `Width` | `int` |  |
| `Height` | `int` |  |

