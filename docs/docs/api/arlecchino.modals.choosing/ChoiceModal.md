---
title: "ChoiceModal"
sidebar_label: "ChoiceModal"
---

# ChoiceModal class

**Namespace:** `Arlecchino.Modals.Choosing` &middot; **Assembly:** `Arlecchino`

One option out of a filterable list.

```csharp
public sealed class ChoiceModal : OptionListModal, ITextEntry
```

**Inherits from** [`OptionListModal`](../arlecchino.modals.choosing/OptionListModal.md)  
**Implements** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

## Constructors

| Member | Summary |
|---|---|
| [`ChoiceModal()`](#choicemodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`OnPicked`](#onpicked) | Called with the chosen option. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |
| [`Take(ModalFrame, string)`](#take-modalframe-string) |  |

## Constructors in detail

### `ChoiceModal()` {#choicemodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public ChoiceModal();
```

## Properties in detail

### `OnPicked` {#onpicked}

```csharp
public Action<string> OnPicked { get; init; }
```

Called with the chosen option.

**Type** `Action<T>`&lt;`string`&gt;

## Methods in detail

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

### `Take(ModalFrame, string)` {#take-modalframe-string}

```csharp
public virtual void Take(ModalFrame frame, string choice);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `choice` | `string` |  |

