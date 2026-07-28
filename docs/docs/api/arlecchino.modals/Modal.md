---
title: Modal
sidebar_label: Modal
---

# Modal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

A dialog waiting for an answer. Assign one to `ArlecchinoState.Modal` — while it is open it takes every key, draws over the view and suppresses the hints box.

```csharp
public abstract class Modal
```

**Derived types** [`ColorModal`](../arlecchino.modals/ColorModal.md), [`CommandModal`](../arlecchino.modals/CommandModal.md), [`MessageModal`](../arlecchino.modals/MessageModal.md), [`NotificationModal`](../arlecchino.modals/NotificationModal.md), [`NumericModal`](../arlecchino.modals/NumericModal.md), [`OptionListModal`](../arlecchino.modals/OptionListModal.md), [`SegmentedModal`](../arlecchino.modals/SegmentedModal.md), [`TextAreaModal`](../arlecchino.modals/TextAreaModal.md), [`TextModal`](../arlecchino.modals/TextModal.md), [`ToggleModal`](../arlecchino.modals/ToggleModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`Modal()`](#modal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Box`](#box) | Where the box was drawn last frame. Filled in by the renderer and used to tell a click on the dialog from a click outside it. |
| [`Title`](#title) | Title written into the top edge of the box. |

## Constructors in detail

### `Modal()` {#modal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public Modal();
```

## Properties in detail

### `Box` {#box}

```csharp
public SurfaceRegion Box { get; set; }
```

Where the box was drawn last frame. Filled in by the renderer and used to tell a click on the dialog from a click outside it.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Title` {#title}

```csharp
public string Title { get; init; }
```

Title written into the top edge of the box.

**Type** `string`

