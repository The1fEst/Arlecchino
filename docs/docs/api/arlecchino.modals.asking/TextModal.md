---
title: "TextModal"
sidebar_label: "TextModal"
---

# TextModal class

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

A line of text: free text, a secret, an email or a link. The built-in format check runs before [`TextModal.Validate`](../arlecchino.modals.asking/TextModal.md#validate), so your validator only sees input that already looks right.

```csharp
public sealed class TextModal : Modal, ITextEntryModal, IAffixedModal, ITextEntry
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)  
**Implements** [`ITextEntryModal`](../arlecchino.modals.asking/ITextEntryModal.md), [`IAffixedModal`](../arlecchino.modals.asking/IAffixedModal.md), [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

## Constructors

| Member | Summary |
|---|---|
| [`TextModal()`](#textmodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Anchor`](#anchor) | Where the selection was started from, on the caret while nothing is selected. |
| [`Caret`](#caret) | Where the caret sits, pulled into the text when it would fall outside. |
| [`Format`](#format) | Built-in check applied on confirm. |
| [`Masked`](#masked) | Whether to draw dots instead of the text. |
| [`Message`](#message) | Validation message shown under the field. |
| [`OnSubmit`](#onsubmit) | Called with the accepted text. |
| [`Prefix`](#prefix) | Drawn before the field. |
| [`Suffix`](#suffix) | Drawn after the field. |
| [`Text`](#text) | Whatever has been typed so far. Assigning it puts the caret at the end. |
| [`Typing`](#typing) | The line being typed into, which for a field of one line is the field itself. |
| [`Validate`](#validate) | Your own check, run after the format one. Return a message to keep the dialog open. |

## Methods

| Member | Summary |
|---|---|
| [`AcceptsCharacter(char)`](#acceptscharacter-char) | Accepts anything that can be typed. |
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |

## Constructors in detail

### `TextModal()` {#textmodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public TextModal();
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

### `Format` {#format}

```csharp
public TextFormat Format { get; init; }
```

Built-in check applied on confirm.

**Type** [`TextFormat`](../arlecchino.modals.asking/TextFormat.md)

### `Masked` {#masked}

```csharp
public bool Masked { get; init; }
```

Whether to draw dots instead of the text.

**Type** `bool`

### `Message` {#message}

```csharp
public string? Message { get; set; }
```

Validation message shown under the field.

**Type** `string`

### `OnSubmit` {#onsubmit}

```csharp
public Action<string> OnSubmit { get; init; }
```

Called with the accepted text.

**Type** `Action<T>`&lt;`string`&gt;

### `Prefix` {#prefix}

```csharp
public string Prefix { get; init; }
```

Drawn before the field.

**Type** `string`

### `Suffix` {#suffix}

```csharp
public string Suffix { get; init; }
```

Drawn after the field.

**Type** `string`

### `Text` {#text}

```csharp
public string Text { get; set; }
```

Whatever has been typed so far. Assigning it puts the caret at the end.

**Type** `string`

### `Typing` {#typing}

```csharp
public override ITextEntry Typing { get; }
```

The line being typed into, which for a field of one line is the field itself.

**Type** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

### `Validate` {#validate}

```csharp
public Func<string, string?>? Validate { get; init; }
```

Your own check, run after the format one. Return a message to keep the dialog open.

**Type** `Func<T, TResult>`&lt;`string`, `string`&gt;

## Methods in detail

### `AcceptsCharacter(char)` {#acceptscharacter-char}

```csharp
public bool AcceptsCharacter(char character);
```

Accepts anything that can be typed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `character` | `char` | The character resolved from the key press. |

**Returns** `bool` — Always `true`.

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

