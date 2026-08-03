---
title: SegmentedModal
sidebar_label: SegmentedModal
---

# SegmentedModal class

**Namespace:** `Arlecchino.Modals.Setting` &middot; **Assembly:** `Arlecchino`

A value edited as a row of fixed-width number segments, the way dates and times are. Digits are collected per segment and only applied once the segment fills up, so a half-typed segment never produces a nonsensical intermediate value.

```csharp
public abstract class SegmentedModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)  
**Derived types** [`DateModal`](../arlecchino.modals.setting/DateModal.md), [`TimeModal`](../arlecchino.modals.setting/TimeModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`SegmentedModal()`](#segmentedmodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Segment`](#segment) | Which segment the arrows and digits currently act on. |
| [`SegmentCount`](#segmentcount) | How many segments the value is made of. |
| [`Separator`](#separator) | What is drawn between the segments. |

## Methods

| Member | Summary |
|---|---|
| [`Add(int)`](#add-int) | Steps the active segment, carrying into the neighbours as that value type requires. |
| [`ApplyTypedValue(int, int)`](#applytypedvalue-int-int) | Stores what was typed into a segment, keeping the whole value legal. |
| [`ClearTypedDigits()`](#cleartypeddigits) | Throws away a partly typed segment, restoring what was stored. |
| [`CommitTypedDigits()`](#committypeddigits) | Applies a partly typed segment, padding it with leading zeroes. Called before anything that reads the value, so confirming the dialog keeps what was typed. |
| [`EditedSegmentTexts()`](#editedsegmenttexts) | What to draw: the stored value, but with half-typed digits shown in place of the segment being edited so typing is visible before it takes effect. |
| [`Handle(ModalFrame, ConsoleKeyInfo)`](#handle-modalframe-consolekeyinfo) |  |
| [`MoveSegment(int)`](#movesegment-int) | Moves between segments, stopping at the ends. Anything half-typed is applied first. |
| [`SegmentLength(int)`](#segmentlength-int) | How many digits a segment holds, which is also when typing moves on to the next one. |
| [`SegmentTexts()`](#segmenttexts) | The stored value as one padded string per segment. |
| [`Submit()`](#submit) | Hands the value over to whoever asked for it, once the segments have been committed. |
| [`TypeDigit(char)`](#typedigit-char) | Adds a digit to the active segment. A full segment is applied and left behind, and typing past it starts the next one over rather than being dropped. |

## Constructors in detail

### `SegmentedModal()` {#segmentedmodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public SegmentedModal();
```

## Properties in detail

### `Segment` {#segment}

```csharp
public int Segment { get; }
```

Which segment the arrows and digits currently act on.

**Type** `int`

### `SegmentCount` {#segmentcount}

```csharp
public abstract int SegmentCount { get; }
```

How many segments the value is made of.

**Type** `int`

### `Separator` {#separator}

```csharp
public abstract string Separator { get; }
```

What is drawn between the segments.

**Type** `string`

## Methods in detail

### `Add(int)` {#add-int}

```csharp
public abstract void Add(int delta);
```

Steps the active segment, carrying into the neighbours as that value type requires.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `int` | How far to step; negative goes down. |

### `ApplyTypedValue(int, int)` {#applytypedvalue-int-int}

```csharp
public abstract void ApplyTypedValue(int segment, int value);
```

Stores what was typed into a segment, keeping the whole value legal.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `segment` | `int` | Index of the segment that was typed into. |
| `value` | `int` | The digits, parsed. |

### `ClearTypedDigits()` {#cleartypeddigits}

```csharp
public void ClearTypedDigits();
```

Throws away a partly typed segment, restoring what was stored.

### `CommitTypedDigits()` {#committypeddigits}

```csharp
public void CommitTypedDigits();
```

Applies a partly typed segment, padding it with leading zeroes. Called before anything that reads the value, so confirming the dialog keeps what was typed.

### `EditedSegmentTexts()` {#editedsegmenttexts}

```csharp
public string[] EditedSegmentTexts();
```

What to draw: the stored value, but with half-typed digits shown in place of the segment being edited so typing is visible before it takes effect.

**Returns** `string`\[\] — One string per segment.

### `Handle(ModalFrame, ConsoleKeyInfo)` {#handle-modalframe-consolekeyinfo}

```csharp
public override void Handle(ModalFrame frame, ConsoleKeyInfo key);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `key` | `ConsoleKeyInfo` |  |

### `MoveSegment(int)` {#movesegment-int}

```csharp
public void MoveSegment(int delta);
```

Moves between segments, stopping at the ends. Anything half-typed is applied first.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `int` | How far to move; negative goes left. |

### `SegmentLength(int)` {#segmentlength-int}

```csharp
public abstract int SegmentLength(int segment);
```

How many digits a segment holds, which is also when typing moves on to the next one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `segment` | `int` | Index of the segment. |

**Returns** `int` — Its digit count.

### `SegmentTexts()` {#segmenttexts}

```csharp
public abstract string[] SegmentTexts();
```

The stored value as one padded string per segment.

**Returns** `string`\[\] — A fresh array the caller may modify.

### `Submit()` {#submit}

```csharp
public abstract void Submit();
```

Hands the value over to whoever asked for it, once the segments have been committed.

### `TypeDigit(char)` {#typedigit-char}

```csharp
public void TypeDigit(char digit);
```

Adds a digit to the active segment. A full segment is applied and left behind, and typing past it starts the next one over rather than being dropped.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `digit` | `char` | The digit that was typed. |

