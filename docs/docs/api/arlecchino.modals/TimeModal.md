---
title: TimeModal
sidebar_label: TimeModal
---

# TimeModal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

A time of day, edited as hours and minutes on a 24-hour clock. Everything wraps: stepping past midnight comes back around instead of stopping.

```csharp
public sealed class TimeModal : SegmentedModal
```

**Inherits from** [`SegmentedModal`](../arlecchino.modals/SegmentedModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`TimeModal()`](#timemodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`OnSubmit`](#onsubmit) | Called with the time that was confirmed. |
| [`SegmentCount`](#segmentcount) | Hours and minutes; seconds are not edited here. |
| [`Separator`](#separator) | Segments are separated the way a clock is written. |
| [`Value`](#value) | The time as it stands. Defaults to midnight. |

## Methods

| Member | Summary |
|---|---|
| [`Add(int)`](#add-int) | Steps by whole hours or minutes depending on the active segment, wrapping around the clock. |
| [`ApplyTypedValue(int, int)`](#applytypedvalue-int-int) | Stores a typed segment, wrapping input that is too large rather than refusing it. |
| [`SegmentLength(int)`](#segmentlength-int) | Two digits for both hours and minutes. |
| [`SegmentTexts()`](#segmenttexts) | The time as two-digit hours and minutes. |

## Constructors in detail

### `TimeModal()` {#timemodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public TimeModal();
```

## Properties in detail

### `OnSubmit` {#onsubmit}

```csharp
public Action<TimeOnly> OnSubmit { get; init; }
```

Called with the time that was confirmed.

**Type** `Action<T>`&lt;`TimeOnly`&gt;

### `SegmentCount` {#segmentcount}

```csharp
public override int SegmentCount { get; }
```

Hours and minutes; seconds are not edited here.

**Type** `int`

### `Separator` {#separator}

```csharp
public override string Separator { get; }
```

Segments are separated the way a clock is written.

**Type** `string`

### `Value` {#value}

```csharp
public TimeOnly Value { get; set; }
```

The time as it stands. Defaults to midnight.

**Type** `TimeOnly`

## Methods in detail

### `Add(int)` {#add-int}

```csharp
public override void Add(int delta);
```

Steps by whole hours or minutes depending on the active segment, wrapping around the clock.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `int` | How far to step; negative goes back. |

### `ApplyTypedValue(int, int)` {#applytypedvalue-int-int}

```csharp
public virtual void ApplyTypedValue(int segment, int value);
```

Stores a typed segment, wrapping input that is too large rather than refusing it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `segment` | `int` | Index of the segment that was typed into. |
| `value` | `int` | The digits, parsed. |

### `SegmentLength(int)` {#segmentlength-int}

```csharp
public virtual int SegmentLength(int segment);
```

Two digits for both hours and minutes.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `segment` | `int` | Index of the segment. |

**Returns** `int` — Its digit count.

### `SegmentTexts()` {#segmenttexts}

```csharp
public override string[] SegmentTexts();
```

The time as two-digit hours and minutes.

**Returns** `string`\[\] — One string per segment.

