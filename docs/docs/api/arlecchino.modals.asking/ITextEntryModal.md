---
title: "ITextEntryModal"
sidebar_label: "ITextEntryModal"
---

# ITextEntryModal interface

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

A field that is typed into, shared by the text field and the number field so both edit and complain alike.

```csharp
public interface ITextEntryModal : IAffixedModal
```

**Implements** [`IAffixedModal`](../arlecchino.modals.asking/IAffixedModal.md)  
**Implemented by** [`NumberModal`](../arlecchino.modals.asking/NumberModal.md), [`TextModal`](../arlecchino.modals.asking/TextModal.md)

## Properties

| Member | Summary |
|---|---|
| [`Caret`](#caret) | Where the caret sits, counted in characters from the start of the text. Values outside the text are pulled back in, so a caret can never point past the end. |
| [`Masked`](#masked) | Whether to draw dots instead of the text. The value itself stays as typed. |
| [`Message`](#message) | Validation message shown under the field, cleared by typing. |
| [`Text`](#text) | Whatever has been typed so far. Assigning it puts the caret at the end, since replacing the text wholesale means the old caret no longer refers to anything. |

## Methods

| Member | Summary |
|---|---|
| [`AcceptsCharacter(char)`](#acceptscharacter-char) | Whether a character may be typed here at all. |

## Properties in detail

### `Caret` {#caret}

```csharp
public int Caret { get; set; }
```

Where the caret sits, counted in characters from the start of the text. Values outside the text are pulled back in, so a caret can never point past the end.

**Type** `int`

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

### `Text` {#text}

```csharp
public string Text { get; set; }
```

Whatever has been typed so far. Assigning it puts the caret at the end, since replacing the text wholesale means the old caret no longer refers to anything.

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

