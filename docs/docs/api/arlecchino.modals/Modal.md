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
| [`Typing`](#typing) | The line this dialog is being typed into now, or nothing while it is not being typed into at all. Naming it is all a dialog has to do to be pasted into. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(ModalFrame)`](#draw-modalframe) | Draws it. |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) | Reads one key, which reaches no one else while this dialog is on top. |
| [`HandleMouse(ModalFrame, MouseEvent)`](#handlemouse-modalframe-mouseevent) | Reads one mouse event. Dialogs that cannot be clicked leave it alone. |
| [`HandlePaste(ModalFrame, string)`](#handlepaste-modalframe-string) | Takes a block of pasted text, which lands in [`Modal.Typing`](../arlecchino.modals/Modal.md#typing) as one edit. A dialog of several rows overrides this; one of a single row has nothing to add. |

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

### `Typing` {#typing}

```csharp
public virtual ITextEntry? Typing { get; }
```

The line this dialog is being typed into now, or nothing while it is not being typed into at all. Naming it is all a dialog has to do to be pasted into.

**Type** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

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

### `HandlePaste(ModalFrame, string)` {#handlepaste-modalframe-string}

```csharp
public virtual void HandlePaste(ModalFrame frame, string text);
```

Takes a block of pasted text, which lands in [`Modal.Typing`](../arlecchino.modals/Modal.md#typing) as one edit. A dialog of several rows overrides this; one of a single row has nothing to add.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | The keys to obey, and how to close. |
| `text` | `string` | What was pasted, with the terminal's markers already stripped. |

