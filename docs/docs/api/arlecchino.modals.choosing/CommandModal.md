---
title: "CommandModal"
sidebar_label: "CommandModal"
---

# CommandModal class

**Namespace:** `Arlecchino.Modals.Choosing` &middot; **Assembly:** `Arlecchino`

The list of what can be pressed right now. It is a reminder rather than a menu: the keys keep working while it is open, so a command runs from its own key instead of from a selection. What a key or a row means is not this dialog's to know — the commands it lists belong to the view and to the application, and both are found through the container. Whoever opens it says what to do with a press, which is why the palette is opened by the framework rather than by an application writing `new CommandModal`.

```csharp
public sealed class CommandModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)

## Constructors

| Member | Summary |
|---|---|
| [`CommandModal()`](#commandmodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Commands`](#commands) | The key and label of every command available in this context. |
| [`OnKey`](#onkey) | What a key press means. Set by whoever opened the palette. |
| [`OnRow`](#onrow) | What a click on a row means. Set by whoever opened the palette. |
| [`Rows`](#rows) | Where the rows were drawn last frame, used to turn a click into a command. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |
| [`HandleMouse(ModalFrame, MouseEvent)`](#handlemouse-modalframe-mouseevent) |  |

## Constructors in detail

### `CommandModal()` {#commandmodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public CommandModal();
```

## Properties in detail

### `Commands` {#commands}

```csharp
public IReadOnlyList<ValueTuple<string, string>> Commands { get; init; }
```

The key and label of every command available in this context.

**Type** `IReadOnlyList<T>`&lt;`ValueTuple<T1, T2>`&lt;`string`, `string`&gt;&gt;

### `OnKey` {#onkey}

```csharp
public Action<KeyPress>? OnKey { get; init; }
```

What a key press means. Set by whoever opened the palette.

**Type** `Action<T>`&lt;[`KeyPress`](../arlecchino.input/KeyPress.md)&gt;

### `OnRow` {#onrow}

```csharp
public Action<int>? OnRow { get; init; }
```

What a click on a row means. Set by whoever opened the palette.

**Type** `Action<T>`&lt;`int`&gt;

### `Rows` {#rows}

```csharp
public SurfaceRegion Rows { get; set; }
```

Where the rows were drawn last frame, used to turn a click into a command.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

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

### `HandleMouse(ModalFrame, MouseEvent)` {#handlemouse-modalframe-mouseevent}

```csharp
public override void HandleMouse(ModalFrame frame, MouseEvent mouse);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) |  |

