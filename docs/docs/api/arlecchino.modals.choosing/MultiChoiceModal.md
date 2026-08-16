---
title: "MultiChoiceModal"
sidebar_label: "MultiChoiceModal"
---

# MultiChoiceModal class

**Namespace:** `Arlecchino.Modals.Choosing` &middot; **Assembly:** `Arlecchino`

Any number of options out of a filterable list. Marks survive a change of filter.

```csharp
public sealed class MultiChoiceModal : OptionListModal, ITextEntry
```

**Inherits from** [`OptionListModal`](../arlecchino.modals.choosing/OptionListModal.md)  
**Implements** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

## Constructors

| Member | Summary |
|---|---|
| [`MultiChoiceModal()`](#multichoicemodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`OnSubmit`](#onsubmit) | Called with everything marked, in the order of the options. |
| [`Selected`](#selected) | Options marked so far. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |
| [`IsSelected(string)`](#isselected-string) | Whether an option is marked. |
| [`SelectedInOptionOrder()`](#selectedinoptionorder) | What is marked, in the order of the options rather than the order it was clicked in, so the result does not depend on how the user got there. |
| [`Take(ModalFrame, string)`](#take-modalframe-string) |  |
| [`Toggle(string)`](#toggle-string) | Marks an option, or unmarks it when it already was. |

## Constructors in detail

### `MultiChoiceModal()` {#multichoicemodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public MultiChoiceModal();
```

## Properties in detail

### `OnSubmit` {#onsubmit}

```csharp
public Action<IReadOnlyList<string>> OnSubmit { get; init; }
```

Called with everything marked, in the order of the options.

**Type** `Action<T>`&lt;`IReadOnlyList<T>`&lt;`string`&gt;&gt;

### `Selected` {#selected}

```csharp
public HashSet<string> Selected { get; init; }
```

Options marked so far.

**Type** `HashSet<T>`&lt;`string`&gt;

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

### `IsSelected(string)` {#isselected-string}

```csharp
public bool IsSelected(string option);
```

Whether an option is marked.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `option` | `string` | The option to check. |

**Returns** `bool` — `true` when it is marked.

### `SelectedInOptionOrder()` {#selectedinoptionorder}

```csharp
public List<string> SelectedInOptionOrder();
```

What is marked, in the order of the options rather than the order it was clicked in, so the result does not depend on how the user got there.

**Returns** `List<T>`&lt;`string`&gt; — The marked options.

### `Take(ModalFrame, string)` {#take-modalframe-string}

```csharp
public virtual void Take(ModalFrame frame, string picked);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `picked` | `string` |  |

### `Toggle(string)` {#toggle-string}

```csharp
public void Toggle(string option);
```

Marks an option, or unmarks it when it already was.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `option` | `string` | The option to flip. |

