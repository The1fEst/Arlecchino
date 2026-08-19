---
title: "PictureLimits"
sidebar_label: "PictureLimits"
---

# PictureLimits struct

**Namespace:** `Arlecchino.Pictures` &middot; **Assembly:** `Arlecchino.Pictures`

What a caller will hold, and what it has a use for. A format that can read itself at a smaller size does so rather than decoding pixels that will never be drawn.

```csharp
public readonly struct PictureLimits : IEquatable<PictureLimits>
```

**Implements** `IEquatable<T>`&lt;[`PictureLimits`](../arlecchino.pictures/PictureLimits.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`PictureLimits(int, int)`](#picturelimits-int-int) | What a caller will hold, and what it has a use for. A format that can read itself at a smaller size does so rather than decoding pixels that will never be drawn. |

## Properties

| Member | Summary |
|---|---|
| [`Default`](#default) | What a caller gets by asking for nothing in particular: the whole picture, within reason. |
| [`EnoughPixels`](#enoughpixels) | How many pixels the caller has a use for, or nought for as many as the picture holds. A decoder answers with at least this many where it can; the size it lands on is its own. |
| [`MostPixels`](#mostpixels) | How many pixels may be held at once, refusing anything larger. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out int, out int)`](#deconstruct-out-int-out-int) |  |
| [`For(int)`](#for-int) | Limits for a caller that will draw the picture no larger than a given number of pixels. |

## Constructors in detail

### `PictureLimits(int, int)` {#picturelimits-int-int}

```csharp
public PictureLimits(int MostPixels, int EnoughPixels);
```

What a caller will hold, and what it has a use for. A format that can read itself at a smaller size does so rather than decoding pixels that will never be drawn.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `MostPixels` | `int` | How many pixels may be held at once, refusing anything larger. |
| `EnoughPixels` | `int` | How many pixels the caller has a use for, or nought for as many as the picture holds. A decoder answers with at least this many where it can; the size it lands on is its own. |

## Properties in detail

### `Default` {#default}

```csharp
public static PictureLimits Default { get; }
```

What a caller gets by asking for nothing in particular: the whole picture, within reason.

**Type** [`PictureLimits`](../arlecchino.pictures/PictureLimits.md)

### `EnoughPixels` {#enoughpixels}

```csharp
public int EnoughPixels { get; init; }
```

How many pixels the caller has a use for, or nought for as many as the picture holds. A decoder answers with at least this many where it can; the size it lands on is its own.

**Type** `int`

### `MostPixels` {#mostpixels}

```csharp
public int MostPixels { get; init; }
```

How many pixels may be held at once, refusing anything larger.

**Type** `int`

## Methods in detail

### `Deconstruct(out int, out int)` {#deconstruct-out-int-out-int}

```csharp
public void Deconstruct(out int MostPixels, out int EnoughPixels);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `MostPixels` | `int` |  |
| `EnoughPixels` | `int` |  |

### `For(int)` {#for-int}

```csharp
public static PictureLimits For(int pixels);
```

Limits for a caller that will draw the picture no larger than a given number of pixels.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pixels` | `int` | How many pixels the caller has a use for. |

**Returns** [`PictureLimits`](../arlecchino.pictures/PictureLimits.md) — The limits.

