---
title: ViewCommand
sidebar_label: ViewCommand
---

# ViewCommand class

**Namespace:** `Arlecchino.Commands` &middot; **Assembly:** `Arlecchino`

A key a screen reacts to, declared as data rather than hidden in a switch. That is what lets the palette list it, the hints box label it, and the conflict check see it.

```csharp
public sealed class ViewCommand
```

## Constructors

| Member | Summary |
|---|---|
| [`ViewCommand()`](#viewcommand) |  |

## Properties

| Member | Summary |
|---|---|
| [`Binding`](#binding) | Key that runs the command. Checked before the screen's own key handling. |
| [`IsEnabled`](#isenabled) | Whether the command can run now. A disabled command still swallows its key rather than letting it fall through — the key is spoken for either way. |
| [`Label`](#label) | Name shown in the palette and the hints box; a delegate, so it can be translated. |
| [`Run`](#run) | What the command does. |

## Methods

| Member | Summary |
|---|---|
| [`For(KeyBinding, Func<string>, Action)`](#for-keybinding-func-string-action) | A command that does something and stays on the screen. |
| [`For(ConsoleKey, Func<string>, Action)`](#for-consolekey-func-string-action) | A command on a plain key that does something and stays on the screen. |
| [`Navigating(ConsoleKey, Func<string>, Func<ViewRoute>)`](#navigating-consolekey-func-string-func-viewroute) | A command on a plain key that navigates somewhere. |

## Constructors in detail

### `ViewCommand()` {#viewcommand}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public ViewCommand();
```

## Properties in detail

### `Binding` {#binding}

```csharp
public KeyBinding Binding { get; init; }
```

Key that runs the command. Checked before the screen's own key handling.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `IsEnabled` {#isenabled}

```csharp
public Func<bool> IsEnabled { get; init; }
```

Whether the command can run now. A disabled command still swallows its key rather than letting it fall through — the key is spoken for either way.

**Type** `Func<TResult>`&lt;`bool`&gt;

### `Label` {#label}

```csharp
public Func<string> Label { get; init; }
```

Name shown in the palette and the hints box; a delegate, so it can be translated.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Run` {#run}

```csharp
public Func<ViewRoute> Run { get; init; }
```

What the command does.

**Type** `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

## Methods in detail

### `For(KeyBinding, Func<string>, Action)` {#for-keybinding-func-string-action}

```csharp
public static ViewCommand For(KeyBinding binding, Func<string> label, Action run);
```

A command that does something and stays on the screen.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `binding` | [`KeyBinding`](../arlecchino.input/KeyBinding.md) | Key that runs it. |
| `label` | `Func<TResult>`&lt;`string`&gt; | Name shown to the user. |
| `run` | `Action` | What to do. |

**Returns** [`ViewCommand`](../arlecchino.commands/ViewCommand.md) — The command.

### `For(ConsoleKey, Func<string>, Action)` {#for-consolekey-func-string-action}

```csharp
public static ViewCommand For(ConsoleKey key, Func<string> label, Action run);
```

A command on a plain key that does something and stays on the screen.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKey` | Key that runs it. |
| `label` | `Func<TResult>`&lt;`string`&gt; | Name shown to the user. |
| `run` | `Action` | What to do. |

**Returns** [`ViewCommand`](../arlecchino.commands/ViewCommand.md) — The command.

### `Navigating(ConsoleKey, Func<string>, Func<ViewRoute>)` {#navigating-consolekey-func-string-func-viewroute}

```csharp
public static ViewCommand Navigating(ConsoleKey key, Func<string> label, Func<ViewRoute> run);
```

A command on a plain key that navigates somewhere.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKey` | Key that runs it. |
| `label` | `Func<TResult>`&lt;`string`&gt; | Name shown to the user. |
| `run` | `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt; | What to do; the route it returns is applied. |

**Returns** [`ViewCommand`](../arlecchino.commands/ViewCommand.md) — The command.

