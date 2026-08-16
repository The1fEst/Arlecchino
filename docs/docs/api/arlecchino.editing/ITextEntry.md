---
title: "ITextEntry"
sidebar_label: "ITextEntry"
---

# ITextEntry interface

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

A line of text being typed into, which is all the editing needs to know about whatever holds it. A dialog field is one; so is a line an application draws for itself.

```csharp
public interface ITextEntry
```

**Implemented by** [`TextEntry`](../arlecchino.editing/TextEntry.md), [`ITextEntryModal`](../arlecchino.modals.asking/ITextEntryModal.md), [`NumberModal`](../arlecchino.modals.asking/NumberModal.md), [`TextAreaModal`](../arlecchino.modals.asking/TextAreaModal.md), [`TextModal`](../arlecchino.modals.asking/TextModal.md), [`ChoiceModal`](../arlecchino.modals.choosing/ChoiceModal.md), [`MultiChoiceModal`](../arlecchino.modals.choosing/MultiChoiceModal.md), [`OptionListModal`](../arlecchino.modals.choosing/OptionListModal.md)

## Properties

| Member | Summary |
|---|---|
| [`Anchor`](#anchor) | Where the selection was started from, counted the same way. It stands on the caret while nothing is selected, and the text between the two is what is selected while they differ. |
| [`Caret`](#caret) | Where the caret sits, counted in characters from the start of the text. Values outside the text are pulled back in, so a caret can never point past the end. |
| [`Text`](#text) | Whatever has been typed so far. Assigning it puts the caret and the anchor at the end, since replacing the text wholesale means neither refers to anything anymore. |

## Properties in detail

### `Anchor` {#anchor}

```csharp
public int Anchor { get; set; }
```

Where the selection was started from, counted the same way. It stands on the caret while nothing is selected, and the text between the two is what is selected while they differ.

**Type** `int`

### `Caret` {#caret}

```csharp
public int Caret { get; set; }
```

Where the caret sits, counted in characters from the start of the text. Values outside the text are pulled back in, so a caret can never point past the end.

**Type** `int`

### `Text` {#text}

```csharp
public string Text { get; set; }
```

Whatever has been typed so far. Assigning it puts the caret and the anchor at the end, since replacing the text wholesale means neither refers to anything anymore.

**Type** `string`

