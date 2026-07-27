---
title: ChoiceModal
sidebar_label: ChoiceModal
---

# ChoiceModal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

One option out of a filterable list.

```csharp
public sealed class ChoiceModal : OptionListModal
```

**Inherits from** [`OptionListModal`](../arlecchino.modals/OptionListModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`ChoiceModal()`](#choicemodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`OnPicked`](#onpicked) | Called with the chosen option. |

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

