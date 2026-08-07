---
title: "MessageModal"
sidebar_label: "MessageModal"
---

# MessageModal class

**Namespace:** `Arlecchino.Modals.Telling` &middot; **Assembly:** `Arlecchino`

Something the user only has to read: a result, a warning, an explanation of what just failed. It takes no input beyond the key that closes it, which is what separates it from every other dialog here.

```csharp
public sealed class MessageModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)

## Constructors

| Member | Summary |
|---|---|
| [`MessageModal()`](#messagemodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`OnClosed`](#onclosed) | Called once it is dismissed, for a screen that wants to carry on afterwards. |
| [`Text`](#text) | The message, drawn under the title. Long text wraps to the width of the box. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |

## Constructors in detail

### `MessageModal()` {#messagemodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public MessageModal();
```

## Properties in detail

### `OnClosed` {#onclosed}

```csharp
public Action? OnClosed { get; init; }
```

Called once it is dismissed, for a screen that wants to carry on afterwards.

**Type** `Action`

### `Text` {#text}

```csharp
public string Text { get; init; }
```

The message, drawn under the title. Long text wraps to the width of the box.

**Type** `string`

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

