---
title: NotificationModal
sidebar_label: NotificationModal
---

# NotificationModal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

One notification, read in full. The output row and the notifications screen have one line each to give a message, which is not enough for the errors a copy collected or the output of a command — opening the entry shows the whole of it, and offers whatever the entry said could be done about it. The notifications screen opens this itself, so an application only fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions) when it raises the entry.

```csharp
public sealed class NotificationModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)

## Constructors

| Member | Summary |
|---|---|
| [`NotificationModal()`](#notificationmodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Actions`](#actions) | What can be done about it. Empty for a message that is only to be read. |
| [`Chips`](#chips) | Where each action was drawn last frame, filled in by the renderer so a click can be resolved to the action under it. |
| [`Entry`](#entry) | The entry being read. |
| [`Index`](#index) | Which action is selected, moved with the left and right keys. |
| [`Text`](#text) | The whole text, wrapped by the renderer to the width of the box. |

## Methods

| Member | Summary |
|---|---|
| [`Move(int)`](#move-int) | Moves the selection along the actions, stopping at both ends. |
| [`Run()`](#run) | Runs the selected action, if there is one. |

## Constructors in detail

### `NotificationModal()` {#notificationmodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public NotificationModal();
```

## Properties in detail

### `Actions` {#actions}

```csharp
public IReadOnlyList<NotificationAction> Actions { get; }
```

What can be done about it. Empty for a message that is only to be read.

**Type** `IReadOnlyList<T>`&lt;[`NotificationAction`](../arlecchino.diagnostics/NotificationAction.md)&gt;

### `Chips` {#chips}

```csharp
public IReadOnlyList<SurfaceRegion> Chips { get; set; }
```

Where each action was drawn last frame, filled in by the renderer so a click can be resolved to the action under it.

**Type** `IReadOnlyList<T>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt;

### `Entry` {#entry}

```csharp
public Notification Entry { get; init; }
```

The entry being read.

**Type** [`Notification`](../arlecchino.diagnostics/Notification.md)

### `Index` {#index}

```csharp
public int Index { get; set; }
```

Which action is selected, moved with the left and right keys.

**Type** `int`

### `Text` {#text}

```csharp
public string Text { get; }
```

The whole text, wrapped by the renderer to the width of the box.

**Type** `string`

## Methods in detail

### `Move(int)` {#move-int}

```csharp
public void Move(int delta);
```

Moves the selection along the actions, stopping at both ends.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `int` | How far to move; negative goes left. |

### `Run()` {#run}

```csharp
public void Run();
```

Runs the selected action, if there is one.

