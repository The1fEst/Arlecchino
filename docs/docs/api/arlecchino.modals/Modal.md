---
title: "Modal"
sidebar_label: "Modal"
---

# Modal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

A dialog waiting for an answer, assigned to `ArlecchinoState.Modal`. It draws itself and reads its own keys, so a kind an application writes is another subclass and nothing more.

```csharp
public abstract class Modal
```

**Derived types** [`NumericModal`](../arlecchino.modals.asking/NumericModal.md), [`TextAreaModal`](../arlecchino.modals.asking/TextAreaModal.md), [`TextModal`](../arlecchino.modals.asking/TextModal.md), [`CommandModal`](../arlecchino.modals.choosing/CommandModal.md), [`OptionListModal`](../arlecchino.modals.choosing/OptionListModal.md), [`ColorModal`](../arlecchino.modals.setting/ColorModal.md), [`SegmentedModal`](../arlecchino.modals.setting/SegmentedModal.md), [`ToggleModal`](../arlecchino.modals.setting/ToggleModal.md), [`MessageModal`](../arlecchino.modals.telling/MessageModal.md), [`NotificationModal`](../arlecchino.modals.telling/NotificationModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`Modal()`](#modal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Box`](#box) | Where the box was drawn last frame. Set as the dialog draws itself, and used to tell a click on it from a click outside. |
| [`Title`](#title) | Title written into the top edge of the box. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(ModalFrame)`](#draw-modalframe) | Draws it. |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) | Reads one key, which reaches no one else while this dialog is on top. |
| [`HandleMouse(ModalFrame, MouseEvent)`](#handlemouse-modalframe-mouseevent) | Reads one mouse event. Dialogs that cannot be clicked leave it alone. |

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

Where the box was drawn last frame. Set as the dialog draws itself, and used to tell a click on it from a click outside.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Title` {#title}

```csharp
public string Title { get; init; }
```

Title written into the top edge of the box.

**Type** `string`

## Methods in detail

### `Draw(ModalFrame)` {#draw-modalframe}

```csharp
public abstract void Draw(ModalFrame frame);
```

Draws it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | Where to draw, and the words to draw in. |

### `Handle(ModalFrame, KeyPress)` {#handle-modalframe-keypress}

```csharp
public abstract void Handle(ModalFrame frame, KeyPress key);
```

Reads one key, which reaches no one else while this dialog is on top.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | The keys to obey, and how to close. |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that arrived. |

### `HandleMouse(ModalFrame, MouseEvent)` {#handlemouse-modalframe-mouseevent}

```csharp
public virtual void HandleMouse(ModalFrame frame, MouseEvent mouse);
```

Reads one mouse event. Dialogs that cannot be clicked leave it alone.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | The keys to obey, and how to close. |
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

