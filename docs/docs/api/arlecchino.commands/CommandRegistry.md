---
title: CommandRegistry
sidebar_label: CommandRegistry
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
| [`Send(ConsoleKeyInfo)`](#send-consolekeyinfo) | Runs the command a key belongs to, if any. |
| [`TryFind(ConsoleKeyInfo, out IArlecchinoCommand)`](#tryfind-consolekeyinfo-out-iarlecchinocommand) | Finds the command a key press belongs to. |

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

### `Send(ConsoleKeyInfo)` {#send-consolekeyinfo}

```csharp
public ViewRoute Send(ConsoleKeyInfo pressed);
```

Runs the command a key belongs to, if any.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | `ConsoleKeyInfo` | The key that was pressed. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — The route the command returned, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none).

### `TryFind(ConsoleKeyInfo, out IArlecchinoCommand)` {#tryfind-consolekeyinfo-out-iarlecchinocommand}

```csharp
public bool TryFind(ConsoleKeyInfo pressed, out IArlecchinoCommand? command);
```

Finds the command a key press belongs to.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | `ConsoleKeyInfo` | The key that was pressed. |
| `command` | [`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md) | The command, when one claims the key. |

**Returns** `bool` — `true` when a command claimed the key.

