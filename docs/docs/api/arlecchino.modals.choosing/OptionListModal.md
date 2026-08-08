---
title: "OptionListModal"
sidebar_label: "OptionListModal"
---

# OptionListModal class

**Namespace:** `Arlecchino.Modals.Choosing` &middot; **Assembly:** `Arlecchino`

What the single- and multi-choice dialogs share: the options, the typed filter and the cursor.

```csharp
public abstract class OptionListModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)  
**Derived types** [`ChoiceModal`](../arlecchino.modals.choosing/ChoiceModal.md), [`MultiChoiceModal`](../arlecchino.modals.choosing/MultiChoiceModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`OptionListModal()`](#optionlistmodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Filter`](#filter) | Whatever has been typed to narrow the list. Editing it resets the cursor to the top. |
| [`FirstVisible`](#firstvisible) | Index of the first option drawn, since a long list only shows a window of it. |
| [`Index`](#index) | Cursor position within the options that match. |
| [`Options`](#options) | Everything that can be chosen from. |
| [`Rows`](#rows) | Where the rows were drawn last frame, used to turn a click into a row. |

## Methods

| Member | Summary |
|---|---|
| [`HandleMouse(ModalFrame, MouseEvent)`](#handlemouse-modalframe-mouseevent) | The wheel walks the list, and a click picks the row it landed on. It only takes that row when the row was already the one under the cursor, so a click never confirms something the eye had not settled on yet. |
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

### `Filter` {#filter}

```csharp
public string Filter { get; set; }
```

Whatever has been typed to narrow the list. Editing it resets the cursor to the top.

**Type** `string`

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

## Methods in detail

### `HandleMouse(ModalFrame, MouseEvent)` {#handlemouse-modalframe-mouseevent}

```csharp
public override void HandleMouse(ModalFrame frame, MouseEvent mouse);
```

The wheel walks the list, and a click picks the row it landed on. It only takes that row when the row was already the one under the cursor, so a click never confirms something the eye had not settled on yet.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | How to close. |
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

### `MatchingOptions()` {#matchingoptions}

```csharp
public List<string> MatchingOptions();
```

The options that pass the filter, in their original order.

**Returns** `List<T>`&lt;`string`&gt; — Matching options; all of them when nothing is typed.

### `Take(ModalFrame, string)` {#take-modalframe-string}

```csharp
public abstract void Take(ModalFrame frame, string picked);
```

Acts on the row that was picked, which is what tells one kind of list from the other.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | How to close, when picking closes. |
| `picked` | `string` | The option. |

