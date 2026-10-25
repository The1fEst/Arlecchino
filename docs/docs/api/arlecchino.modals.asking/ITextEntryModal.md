---
title: "ITextEntryModal"
sidebar_label: "ITextEntryModal"
---

# ITextEntryModal interface

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

A field that is typed into, shared by the text field and the number field so both edit and complain alike. What is typed and where the caret is come from [`ITextEntry`](../arlecchino.editing/ITextEntry.md).

```csharp
public interface ITextEntryModal : IAffixedModal, ITextEntry
```

**Implements** [`IAffixedModal`](../arlecchino.modals.asking/IAffixedModal.md), [`ITextEntry`](../arlecchino.editing/ITextEntry.md)  
**Implemented by** [`NumberModal`](../arlecchino.modals.asking/NumberModal.md), [`TextModal`](../arlecchino.modals.asking/TextModal.md)

## Properties

| Member | Summary |
|---|---|
| [`Masked`](#masked) | Whether to draw dots instead of the text. The value itself stays as typed. |
| [`Message`](#message) | Validation message shown under the field, cleared by typing. |

## Methods

| Member | Summary |
|---|---|
| [`AcceptsCharacter(char)`](#acceptscharacter-char) | Whether a character may be typed here at all. |

## Properties in detail

### `Masked` {#masked}

```csharp
public bool Masked { get; }
```

Whether to draw dots instead of the text. The value itself stays as typed.

**Type** `bool`

### `Message` {#message}

```csharp
public string? Message { get; set; }
```

Validation message shown under the field, cleared by typing.

**Type** `string`

## Methods in detail

### `AcceptsCharacter(char)` {#acceptscharacter-char}

```csharp
public bool AcceptsCharacter(char character);
```

Whether a character may be typed here at all.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `character` | `char` | The character resolved from the key press. |

**Returns** `bool` — `true` when it should be inserted.

