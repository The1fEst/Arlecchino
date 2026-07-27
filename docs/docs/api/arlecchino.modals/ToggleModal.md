---
title: ToggleModal
sidebar_label: ToggleModal
---

# ToggleModal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

A yes-or-no answer, flipped with the arrows or picked by clicking one of the two chips.

```csharp
public sealed class ToggleModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)

## Constructors

| Member | Summary |
|---|---|
| [`ToggleModal()`](#togglemodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`NoChip`](#nochip) | Where the negative chip was drawn last frame. |
| [`OnSubmit`](#onsubmit) | Called with the answer that was confirmed. |
| [`Value`](#value) | The answer as it stands. |
| [`YesChip`](#yeschip) | Where the affirmative chip was drawn last frame, used to turn a click into an answer. |

## Constructors in detail

### `ToggleModal()` {#togglemodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public ToggleModal();
```

## Properties in detail

### `NoChip` {#nochip}

```csharp
public SurfaceRegion NoChip { get; set; }
```

Where the negative chip was drawn last frame.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `OnSubmit` {#onsubmit}

```csharp
public Action<bool> OnSubmit { get; init; }
```

Called with the answer that was confirmed.

**Type** `Action<T>`&lt;`bool`&gt;

### `Value` {#value}

```csharp
public bool Value { get; set; }
```

The answer as it stands.

**Type** `bool`

### `YesChip` {#yeschip}

```csharp
public SurfaceRegion YesChip { get; set; }
```

Where the affirmative chip was drawn last frame, used to turn a click into an answer.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

