---
title: "NumberModal"
sidebar_label: "NumberModal"
---

# NumberModal class

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

A number that can be both typed and stepped. Bounds are checked before your validator runs, and the message reports them with affixes, so the user sees the same form they are editing.

```csharp
public sealed class NumberModal :
    NumericModal,
    IAffixedModal,
    ITextEntryModal,
    ITextEntry,
    IBoundedModal
```

**Inherits from** [`NumericModal`](../arlecchino.modals.asking/NumericModal.md)  
**Implements** [`IAffixedModal`](../arlecchino.modals.asking/IAffixedModal.md), [`ITextEntryModal`](../arlecchino.modals.asking/ITextEntryModal.md), [`ITextEntry`](../arlecchino.editing/ITextEntry.md), [`IBoundedModal`](../arlecchino.modals.setting/IBoundedModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`NumberModal()`](#numbermodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Anchor`](#anchor) | Where the selection was started from, on the caret while nothing is selected. |
| [`Caret`](#caret) | Where the caret sits, pulled into the text when it would fall outside. |
| [`Masked`](#masked) | Numbers are never masked. |
| [`Maximum`](#maximum) | Highest value allowed. |
| [`Message`](#message) | Validation message shown under the field. |
| [`Minimum`](#minimum) | Lowest value allowed. A negative bound is also what allows a minus sign to be typed. |
| [`OnSubmit`](#onsubmit) | Called with the accepted number. |
| [`Text`](#text) | Whatever has been typed so far, which may not parse yet. Assigning it puts the caret at the end, which is what makes stepping leave the caret after the new number. |
| [`Typing`](#typing) | The line being typed into, which for a field of one line is the field itself. |
| [`Validate`](#validate) | Your own check, run after parsing and bounds. Return a message to keep the dialog open. |

## Methods

| Member | Summary |
|---|---|
| [`AcceptsCharacter(char)`](#acceptscharacter-char) | Whether a character belongs in a number here: digits always, a separator only when decimals are allowed, a minus only when the range goes below zero. |
| [`Add(decimal)`](#add-decimal) | Steps the value and rewrites the text with it. Text that does not parse is treated as zero, so stepping always leaves a valid number behind. |
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |
| [`TryGetValue(out decimal)`](#trygetvalue-out-decimal) | Reads what has been typed as a number. Both `.` and `,` are accepted. |

## Constructors in detail

### `NumberModal()` {#numbermodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public NumberModal();
```

## Properties in detail

### `Anchor` {#anchor}

```csharp
public int Anchor { get; set; }
```

Where the selection was started from, on the caret while nothing is selected.

**Type** `int`

### `Caret` {#caret}

```csharp
public int Caret { get; set; }
```

Where the caret sits, pulled into the text when it would fall outside.

**Type** `int`

### `Masked` {#masked}

```csharp
public bool Masked { get; }
```

Numbers are never masked.

**Type** `bool`

### `Maximum` {#maximum}

```csharp
public decimal Maximum { get; init; }
```

Highest value allowed.

**Type** `decimal`

### `Message` {#message}

```csharp
public string? Message { get; set; }
```

Validation message shown under the field.

**Type** `string`

### `Minimum` {#minimum}

```csharp
public decimal Minimum { get; init; }
```

Lowest value allowed. A negative bound is also what allows a minus sign to be typed.

**Type** `decimal`

### `OnSubmit` {#onsubmit}

```csharp
public Action<decimal> OnSubmit { get; init; }
```

Called with the accepted number.

**Type** `Action<T>`&lt;`decimal`&gt;

### `Text` {#text}

```csharp
public string Text { get; set; }
```

Whatever has been typed so far, which may not parse yet. Assigning it puts the caret at the end, which is what makes stepping leave the caret after the new number.

**Type** `string`

### `Typing` {#typing}

```csharp
public override ITextEntry Typing { get; }
```

The line being typed into, which for a field of one line is the field itself.

**Type** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

### `Validate` {#validate}

```csharp
public Func<decimal, string?>? Validate { get; init; }
```

Your own check, run after parsing and bounds. Return a message to keep the dialog open.

**Type** `Func<T, TResult>`&lt;`decimal`, `string`&gt;

## Methods in detail

### `AcceptsCharacter(char)` {#acceptscharacter-char}

```csharp
public bool AcceptsCharacter(char character);
```

Whether a character belongs in a number here: digits always, a separator only when decimals are allowed, a minus only when the range goes below zero.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `character` | `char` | The character resolved from the key press. |

**Returns** `bool` — `true` when it should be inserted.

### `Add(decimal)` {#add-decimal}

```csharp
public void Add(decimal delta);
```

Steps the value and rewrites the text with it. Text that does not parse is treated as zero, so stepping always leaves a valid number behind.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `decimal` | How far to move; negative goes down. |

### `Draw(ModalFrame)` {#draw-modalframe}

```csharp
public override void Draw(ModalFrame frame);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |

### `Handle(ModalFrame, KeyPress)` {#handle-modalframe-keypress}

```csharp
public override void Handle(ModalFrame frame, KeyPress key);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) |  |

### `TryGetValue(out decimal)` {#trygetvalue-out-decimal}

```csharp
public bool TryGetValue(out decimal value);
```

Reads what has been typed as a number. Both `.` and `,` are accepted.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `decimal` | The parsed value, when the text is a number. |

**Returns** `bool` — `true` when the text parses.

