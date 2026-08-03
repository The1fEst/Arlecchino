---
title: DateModal
sidebar_label: DateModal
---

# DateModal class

**Namespace:** `Arlecchino.Modals.Setting` &middot; **Assembly:** `Arlecchino`

A calendar date, edited as year, month and day. The value is kept inside the bounds and inside the calendar at every step, so a day that the new month does not have is pulled back to its last day rather than rejected.

```csharp
public sealed class DateModal : SegmentedModal
```

**Inherits from** [`SegmentedModal`](../arlecchino.modals.setting/SegmentedModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`DateModal()`](#datemodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Maximum`](#maximum) | Latest date allowed. |
| [`Minimum`](#minimum) | Earliest date allowed. |
| [`OnSubmit`](#onsubmit) | Called with the date that was confirmed. |
| [`SegmentCount`](#segmentcount) | Year, month and day. |
| [`Separator`](#separator) | Segments are drawn in ISO order, so they are separated by a dash. |
| [`Value`](#value) | The date as it stands. Defaults to today. |

## Methods

| Member | Summary |
|---|---|
| [`Add(int)`](#add-int) | Steps by whole years, months or days depending on the active segment, so stepping the month at the end of a long month lands on a date the shorter month actually has. |
| [`ApplyTypedValue(int, int)`](#applytypedvalue-int-int) | Stores a typed segment, pulling impossible input into range: month into 1-12 and day into the days the resulting month has. |
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`SegmentLength(int)`](#segmentlength-int) | Four digits for the year, two for the rest. |
| [`SegmentTexts()`](#segmenttexts) | The date as a four-digit year and two-digit month and day. |
| [`Submit()`](#submit) |  |

## Constructors in detail

### `DateModal()` {#datemodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public DateModal();
```

## Properties in detail

### `Maximum` {#maximum}

```csharp
public DateOnly Maximum { get; init; }
```

Latest date allowed.

**Type** `DateOnly`

### `Minimum` {#minimum}

```csharp
public DateOnly Minimum { get; init; }
```

Earliest date allowed.

**Type** `DateOnly`

### `OnSubmit` {#onsubmit}

```csharp
public Action<DateOnly> OnSubmit { get; init; }
```

Called with the date that was confirmed.

**Type** `Action<T>`&lt;`DateOnly`&gt;

### `SegmentCount` {#segmentcount}

```csharp
public override int SegmentCount { get; }
```

Year, month and day.

**Type** `int`

### `Separator` {#separator}

```csharp
public override string Separator { get; }
```

Segments are drawn in ISO order, so they are separated by a dash.

**Type** `string`

### `Value` {#value}

```csharp
public DateOnly Value { get; set; }
```

The date as it stands. Defaults to today.

**Type** `DateOnly`

## Methods in detail

### `Add(int)` {#add-int}

```csharp
public override void Add(int delta);
```

Steps by whole years, months or days depending on the active segment, so stepping the month at the end of a long month lands on a date the shorter month actually has.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `int` | How far to step; negative goes back. |

### `ApplyTypedValue(int, int)` {#applytypedvalue-int-int}

```csharp
public virtual void ApplyTypedValue(int segment, int value);
```

Stores a typed segment, pulling impossible input into range: month into 1-12 and day into the days the resulting month has.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `segment` | `int` | Index of the segment that was typed into. |
| `value` | `int` | The digits, parsed. |

### `Draw(ModalFrame)` {#draw-modalframe}

```csharp
public override void Draw(ModalFrame frame);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |

### `SegmentLength(int)` {#segmentlength-int}

```csharp
public virtual int SegmentLength(int segment);
```

Four digits for the year, two for the rest.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `segment` | `int` | Index of the segment. |

**Returns** `int` — Its digit count.

### `SegmentTexts()` {#segmenttexts}

```csharp
public override string[] SegmentTexts();
```

The date as a four-digit year and two-digit month and day.

**Returns** `string`\[\] — One string per segment.

### `Submit()` {#submit}

```csharp
public virtual void Submit();
```

