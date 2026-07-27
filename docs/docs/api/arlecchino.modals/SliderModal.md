---
title: SliderModal
sidebar_label: SliderModal
---

# SliderModal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

A value inside a range, adjusted by arrows or by dragging the track. There is nothing to type, so the value is always valid and the dialog never reports an error.

```csharp
public sealed class SliderModal : NumericModal, IAffixedModal, IBoundedModal
```

**Inherits from** [`NumericModal`](../arlecchino.modals/NumericModal.md)  
**Implements** [`IAffixedModal`](../arlecchino.modals/IAffixedModal.md), [`IBoundedModal`](../arlecchino.modals/IBoundedModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`SliderModal()`](#slidermodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Fraction`](#fraction) | How far along the track the handle sits, as `0` to `1`. An empty range reads as `0`. |
| [`Maximum`](#maximum) | Value at the right end of the track. |
| [`Minimum`](#minimum) | Value at the left end of the track. |
| [`OnSubmit`](#onsubmit) | Called with the value the handle was left at. |
| [`Track`](#track) | Where the track was drawn last frame, used to turn a click into a value. |
| [`Value`](#value) | Where the handle currently sits. |

## Methods

| Member | Summary |
|---|---|
| [`Add(decimal)`](#add-decimal) | Moves the handle, stopping at the ends of the range. |
| [`MoveToMaximum()`](#movetomaximum) | Jumps to the right end. |
| [`MoveToMinimum()`](#movetominimum) | Jumps to the left end. |
| [`SetFromFraction(decimal)`](#setfromfraction-decimal) | Places the handle at a position along the track. |

## Constructors in detail

### `SliderModal()` {#slidermodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public SliderModal();
```

## Properties in detail

### `Fraction` {#fraction}

```csharp
public decimal Fraction { get; }
```

How far along the track the handle sits, as `0` to `1`. An empty range reads as `0`.

**Type** `decimal`

### `Maximum` {#maximum}

```csharp
public decimal Maximum { get; init; }
```

Value at the right end of the track.

**Type** `decimal`

### `Minimum` {#minimum}

```csharp
public decimal Minimum { get; init; }
```

Value at the left end of the track.

**Type** `decimal`

### `OnSubmit` {#onsubmit}

```csharp
public Action<decimal> OnSubmit { get; init; }
```

Called with the value the handle was left at.

**Type** `Action<T>`&lt;`decimal`&gt;

### `Track` {#track}

```csharp
public SurfaceRegion Track { get; set; }
```

Where the track was drawn last frame, used to turn a click into a value.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Value` {#value}

```csharp
public decimal Value { get; set; }
```

Where the handle currently sits.

**Type** `decimal`

## Methods in detail

### `Add(decimal)` {#add-decimal}

```csharp
public void Add(decimal delta);
```

Moves the handle, stopping at the ends of the range.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `decimal` | How far to move; negative goes left. |

### `MoveToMaximum()` {#movetomaximum}

```csharp
public void MoveToMaximum();
```

Jumps to the right end.

### `MoveToMinimum()` {#movetominimum}

```csharp
public void MoveToMinimum();
```

Jumps to the left end.

### `SetFromFraction(decimal)` {#setfromfraction-decimal}

```csharp
public void SetFromFraction(decimal fraction);
```

Places the handle at a position along the track.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `fraction` | `decimal` | Position from `0` at the left end to `1` at the right; anything outside is pulled in. |

