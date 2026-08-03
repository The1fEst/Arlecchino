---
title: IBoundedModal
sidebar_label: IBoundedModal
---

# IBoundedModal interface

**Namespace:** `Arlecchino.Modals.Setting` &middot; **Assembly:** `Arlecchino`

A value that moves in steps between two ends. Shared by the number field and the slider, so the stepping keys work the same in both.

```csharp
public interface IBoundedModal
```

**Implemented by** [`NumberModal`](../arlecchino.modals.asking/NumberModal.md), [`SliderModal`](../arlecchino.modals.setting/SliderModal.md)

## Properties

| Member | Summary |
|---|---|
| [`LargeStep`](#largestep) | How far the page keys move the value. |
| [`Maximum`](#maximum) | Highest value allowed. |
| [`Minimum`](#minimum) | Lowest value allowed. |
| [`Step`](#step) | How far the arrow keys move the value. |

## Methods

| Member | Summary |
|---|---|
| [`Add(decimal)`](#add-decimal) | Moves the value and clamps it to the bounds. |

## Properties in detail

### `LargeStep` {#largestep}

```csharp
public decimal LargeStep { get; }
```

How far the page keys move the value.

**Type** `decimal`

### `Maximum` {#maximum}

```csharp
public decimal Maximum { get; }
```

Highest value allowed.

**Type** `decimal`

### `Minimum` {#minimum}

```csharp
public decimal Minimum { get; }
```

Lowest value allowed.

**Type** `decimal`

### `Step` {#step}

```csharp
public decimal Step { get; }
```

How far the arrow keys move the value.

**Type** `decimal`

## Methods in detail

### `Add(decimal)` {#add-decimal}

```csharp
public void Add(decimal delta);
```

Moves the value and clamps it to the bounds.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `decimal` | How far to move; negative goes down. |

