---
title: "OptionListModal"
sidebar_label: "OptionListModal"
---

# OptionListModal class

**Namespace:** `Arlecchino.Modals.Choosing` &middot; **Assembly:** `Arlecchino`

What the single- and multi-choice dialogs share: the options, what has been typed to narrow them, and the cursor. What is typed is edited the way any other line is, so a symbol goes in one press.

```csharp
public abstract class OptionListModal : Modal, ITextEntry
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)  
**Implements** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)  
**Derived types** [`ChoiceModal`](../arlecchino.modals.choosing/ChoiceModal.md), [`MultiChoiceModal`](../arlecchino.modals.choosing/MultiChoiceModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`OptionListModal()`](#optionlistmodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Anchor`](#anchor) | Where a selection would start from, on the caret since none is drawn here. |
| [`Caret`](#caret) | Where the caret sits in what has been typed, which is at the end of it. |
| [`FirstVisible`](#firstvisible) | Index of the first option drawn, since a long list only shows a window of it. |
| [`Index`](#index) | Cursor position within the options that match. |
| [`Options`](#options) | Everything that can be chosen from. |
| [`Rows`](#rows) | Where the rows were drawn last frame, used to turn a click into a row. |
| [`Text`](#text) | Whatever has been typed to narrow the list. Editing it resets the cursor to the top. |
| [`Typing`](#typing) | The line being typed into, which here is what narrows the list. |

## Methods

| Member | Summary |
|---|---|
| [`HandleMouse(ModalFrame, MouseEvent)`](#handlemouse-modalframe-mouseevent) | The wheel walks the list, and a click picks the row it landed on. A row is taken only when it was already the one under the cursor. |
| [`HandlePaste(ModalFrame, string)`](#handlepaste-modalframe-string) | Takes pasted text into the filter and puts the cursor back on the first row, since what is showing after the paste is a different set of rows. |
| [`MatchingOptions()`](#matchingoptions) | The options that pass the filter, in their original order. |
| [`Take(ModalFrame, string)`](#take-modalframe-string) | Acts on the row that was picked, which is what tells one kind of list from the other. |

## Constructors in detail

### `OptionListModal()` {#optionlistmodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public OptionListModal();
```

## Properties in detail

### `Anchor` {#anchor}

```csharp
public int Anchor { get; set; }
```

Where a selection would start from, on the caret since none is drawn here.

**Type** `int`

### `Caret` {#caret}

```csharp
public int Caret { get; set; }
```

Where the caret sits in what has been typed, which is at the end of it.

**Type** `int`

### `FirstVisible` {#firstvisible}

```csharp
public int FirstVisible { get; set; }
```

Index of the first option drawn, since a long list only shows a window of it.

**Type** `int`

### `Index` {#index}

```csharp
public int Index { get; set; }
```

Cursor position within the options that match.

**Type** `int`

### `Options` {#options}

```csharp
public IReadOnlyList<string> Options { get; init; }
```

Everything that can be chosen from.

**Type** `IReadOnlyList<T>`&lt;`string`&gt;

### `Rows` {#rows}

```csharp
public SurfaceRegion Rows { get; set; }
```

Where the rows were drawn last frame, used to turn a click into a row.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Text` {#text}

```csharp
public string Text { get; set; }
```

Whatever has been typed to narrow the list. Editing it resets the cursor to the top.

**Type** `string`

### `Typing` {#typing}

```csharp
public override ITextEntry Typing { get; }
```

The line being typed into, which here is what narrows the list.

**Type** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

## Methods in detail

### `HandleMouse(ModalFrame, MouseEvent)` {#handlemouse-modalframe-mouseevent}

```csharp
public override void HandleMouse(ModalFrame frame, MouseEvent mouse);
```

The wheel walks the list, and a click picks the row it landed on. A row is taken only when it was already the one under the cursor.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | How to close. |
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

### `HandlePaste(ModalFrame, string)` {#handlepaste-modalframe-string}

```csharp
public override void HandlePaste(ModalFrame frame, string text);
```

Takes pasted text into the filter and puts the cursor back on the first row, since what is showing after the paste is a different set of rows.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | The keys to obey, and how to close. |
| `text` | `string` | What was pasted. |

### `MatchingOptions()` {#matchingoptions}

```csharp
public List<string> MatchingOptions();
```

The options that pass the filter, in their original order.

**Returns** `List<T>`&lt;`string`&gt; — Matching options; all of them when nothing is typed.

### `Take(ModalFrame, string)` {#take-modalframe-string}

```csharp
public abstract void Take(ModalFrame frame, string choice);
```

Acts on the row that was picked, which is what tells one kind of list from the other.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | How to close, when picking closes. |
| `choice` | `string` | The option. |

