---
title: "Margin"
sidebar_label: "Margin"
---

# Margin struct

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

Blank space around something, measured in cells. Used both by the flow calls on [`Surface`](../arlecchino.rendering/Surface.md) and by [`SurfaceRegion.Inset`](../arlecchino.rendering/SurfaceRegion.md#inset-margin).

```csharp
public readonly struct Margin : IEquatable<Margin>
```

**Implements** `IEquatable<T>`&lt;[`Margin`](../arlecchino.rendering/Margin.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Margin(int)`](#margin-int) | The same amount of space on all four sides. |
| [`Margin(int, int, int, int)`](#margin-int-int-int-int) | Blank space around something, measured in cells. Used both by the flow calls on [`Surface`](../arlecchino.rendering/Surface.md) and by [`SurfaceRegion.Inset`](../arlecchino.rendering/SurfaceRegion.md#inset-margin). |

## Properties

| Member | Summary |
|---|---|
| [`Bottom`](#bottom) | Rows kept free below. |
| [`Left`](#left) | Cells kept free on the left. |
| [`Right`](#right) | Cells kept free on the right. |
| [`Top`](#top) | Rows kept free above. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out int, out int, out int, out int)`](#deconstruct-out-int-out-int-out-int-out-int) |  |

## Constructors in detail

### `Margin(int)` {#margin-int}

```csharp
public Margin(int all);
```

The same amount of space on all four sides.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `all` | `int` | Cells kept free on every side. |

### `Margin(int, int, int, int)` {#margin-int-int-int-int}

```csharp
public Margin(int Left, int Top, int Right, int Bottom);
```

Blank space around something, measured in cells. Used both by the flow calls on [`Surface`](../arlecchino.rendering/Surface.md) and by [`SurfaceRegion.Inset`](../arlecchino.rendering/SurfaceRegion.md#inset-margin).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Left` | `int` | Cells kept free on the left. |
| `Top` | `int` | Rows kept free above. |
| `Right` | `int` | Cells kept free on the right. |
| `Bottom` | `int` | Rows kept free below. |

## Properties in detail

### `Bottom` {#bottom}

```csharp
public int Bottom { get; init; }
```

Rows kept free below.

**Type** `int`

### `Left` {#left}

```csharp
public int Left { get; init; }
```

Cells kept free on the left.

**Type** `int`

### `Right` {#right}

```csharp
public int Right { get; init; }
```

Cells kept free on the right.

**Type** `int`

### `Top` {#top}

```csharp
public int Top { get; init; }
```

Rows kept free above.

**Type** `int`

## Methods in detail

### `Deconstruct(out int, out int, out int, out int)` {#deconstruct-out-int-out-int-out-int-out-int}

```csharp
public void Deconstruct(out int Left, out int Top, out int Right, out int Bottom);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Left` | `int` |  |
| `Top` | `int` |  |
| `Right` | `int` |  |
| `Bottom` | `int` |  |

