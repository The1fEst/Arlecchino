---
title: "CommandRegistry"
sidebar_label: "CommandRegistry"
---

# CommandRegistry class

**Namespace:** `Arlecchino.Commands` &middot; **Assembly:** `Arlecchino`

The application commands registered with `AddCommand`. Take it in a view to list or run them yourself — the sample draws its menu straight from this.

```csharp
public class CommandRegistry
```

## Constructors

| Member | Summary |
|---|---|
| [`CommandRegistry(IEnumerable<IArlecchinoCommand>)`](#commandregistry-ienumerable-iarlecchinocommand) | Collects the registered commands. |

## Properties

| Member | Summary |
|---|---|
| [`Commands`](#commands) | The registered commands, in registration order. |

## Methods

| Member | Summary |
|---|---|
| [`Send(KeyPress)`](#send-keypress) | Runs the command a key belongs to, if any. |
| [`TryFind(KeyPress, out IArlecchinoCommand)`](#tryfind-keypress-out-iarlecchinocommand) | Finds the command a key press belongs to. |

## Constructors in detail

### `CommandRegistry(IEnumerable<IArlecchinoCommand>)` {#commandregistry-ienumerable-iarlecchinocommand}

```csharp
public CommandRegistry(IEnumerable<IArlecchinoCommand> commands);
```

Collects the registered commands.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `commands` | `IEnumerable<T>`&lt;[`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md)&gt; | Commands from the container, in registration order. |

## Properties in detail

### `Commands` {#commands}

```csharp
public IReadOnlyList<IArlecchinoCommand> Commands { get; }
```

The registered commands, in registration order.

**Type** `IReadOnlyList<T>`&lt;[`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md)&gt;

## Methods in detail

### `Send(KeyPress)` {#send-keypress}

```csharp
public ViewRoute Send(KeyPress pressed);
```

Runs the command a key belongs to, if any.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — The route the command returned, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none).

### `TryFind(KeyPress, out IArlecchinoCommand)` {#tryfind-keypress-out-iarlecchinocommand}

```csharp
public bool TryFind(KeyPress pressed, out IArlecchinoCommand? command);
```

Finds the command a key press belongs to.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |
| `command` | [`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md) | The command, when one claims the key. |

**Returns** `bool` — `true` when a command claimed the key.

