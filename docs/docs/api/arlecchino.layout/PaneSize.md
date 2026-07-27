---
title: PaneSize
sidebar_label: PaneSize
---

# PaneSize struct

**Namespace:** `Arlecchino.Layout` &middot; **Assembly:** `Arlecchino`

How much of a region a branch gives to its first half: a share of what there is, a fixed number of cells, or — for the toolbars and status bars that sit at the far edge — a fixed number of cells measured from the other end. The unit is the literal, not the number. A `double` is a share and an `int` is a count of cells, and both convert on their own, so the call site says which it meant by whether it has a decimal point:

```csharp
Branch(Rows, 3, header, body);      // three rows
Branch(Rows, 0.3, header, body);    // three tenths of the height
Branch(Columns, 3, side, main);     // three columns — a count follows the direction of the cut

```

The pair worth remembering is `1` and `1.0`: the first is one row, the second is all of them. A bare `0` is rejected by the compiler rather than guessed at — it fits both a [`PaneSplit`](../arlecchino.layout/PaneSplit.md) and a size — so write [`PaneSize.Fraction`](../arlecchino.layout/PaneSize.md#fraction-double) or [`PaneSize.Cells`](../arlecchino.layout/PaneSize.md#cells-int) when nothing is what you mean.

```csharp
public readonly struct PaneSize : IEquatable<PaneSize>
```

**Implements** `IEquatable<T>`&lt;[`PaneSize`](../arlecchino.layout/PaneSize.md)&gt;

## Methods

| Member | Summary |
|---|---|
| [`Cells(int)`](#cells-int) | A fixed number of cells, however big the region is. |
| [`CellsFromEnd(int)`](#cellsfromend-int) | Everything except a fixed number of cells, which is how a one-row status bar at the bottom is written: the first half takes the rest, the second half takes what was reserved. |
| [`Fraction(double)`](#fraction-double) | A share of the space, between nothing and all of it. |

## Operators

| Member | Summary |
|---|---|
| [`operator Implicit(double)`](#operator-implicit-double) |  |
| [`operator Implicit(int)`](#operator-implicit-int) |  |

## Methods in detail

### `Cells(int)` {#cells-int}

```csharp
public static PaneSize Cells(int count);
```

A fixed number of cells, however big the region is.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `count` | `int` | Columns or rows, whichever way the split runs. |

**Returns** [`PaneSize`](../arlecchino.layout/PaneSize.md) — The size.

### `CellsFromEnd(int)` {#cellsfromend-int}

```csharp
public static PaneSize CellsFromEnd(int count);
```

Everything except a fixed number of cells, which is how a one-row status bar at the bottom is written: the first half takes the rest, the second half takes what was reserved.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `count` | `int` | Columns or rows to leave for the second half. |

**Returns** [`PaneSize`](../arlecchino.layout/PaneSize.md) — The size.

### `Fraction(double)` {#fraction-double}

```csharp
public static PaneSize Fraction(double value);
```

A share of the space, between nothing and all of it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `double` | The share, clamped to `0..1`. |

**Returns** [`PaneSize`](../arlecchino.layout/PaneSize.md) — The size.

## Operators in detail

### `operator Implicit(double)` {#operator-implicit-double}

```csharp
public static PaneSize op_Implicit(double value);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `double` |  |

**Returns** [`PaneSize`](../arlecchino.layout/PaneSize.md)

### `operator Implicit(int)` {#operator-implicit-int}

```csharp
public static PaneSize op_Implicit(int count);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `count` | `int` |  |

**Returns** [`PaneSize`](../arlecchino.layout/PaneSize.md)

