---
title: Margin
sidebar_label: Margin
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
| [`Deconstruct(Int32&, Int32&, Int32&, Int32&)`](#deconstruct-int32-int32-int32-int32) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(Margin)`](#equals-margin) |  |
| [`GetHashCode()`](#gethashcode) |  |
| [`ToString()`](#tostring) |  |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(Margin, Margin)`](#operator-inequality-margin-margin) |  |
| [`operator Equality(Margin, Margin)`](#operator-equality-margin-margin) |  |

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

### `Deconstruct(Int32&, Int32&, Int32&, Int32&)` {#deconstruct-int32-int32-int32-int32}

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

### `Equals(object)` {#equals-object}

```csharp
public virtual bool Equals(object obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(Margin)` {#equals-margin}

```csharp
public bool Equals(Margin other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`Margin`](../arlecchino.rendering/Margin.md) |  |

**Returns** `bool`

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

**Returns** `int`

### `ToString()` {#tostring}

```csharp
public virtual string ToString();
```

**Returns** `string`

## Operators in detail

### `operator Inequality(Margin, Margin)` {#operator-inequality-margin-margin}

```csharp
public static bool op_Inequality(Margin left, Margin right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`Margin`](../arlecchino.rendering/Margin.md) |  |
| `right` | [`Margin`](../arlecchino.rendering/Margin.md) |  |

**Returns** `bool`

### `operator Equality(Margin, Margin)` {#operator-equality-margin-margin}

```csharp
public static bool op_Equality(Margin left, Margin right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`Margin`](../arlecchino.rendering/Margin.md) |  |
| `right` | [`Margin`](../arlecchino.rendering/Margin.md) |  |

**Returns** `bool`

