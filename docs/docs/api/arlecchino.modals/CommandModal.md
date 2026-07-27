---
title: CommandModal
sidebar_label: CommandModal
---

# CommandModal class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

The list of what can be pressed right now. It is a reminder rather than a menu: the keys keep working while it is open, so a command runs from its own key instead of from a selection.

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
| [`Rows`](#rows) | Where the rows were drawn last frame, used to turn a click into a command. |

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

### `Rows` {#rows}

```csharp
public SurfaceRegion Rows { get; set; }
```

Where the rows were drawn last frame, used to turn a click into a command.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

