---
title: "KeyBinding"
sidebar_label: "KeyBinding"
---

# KeyBinding struct

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable.

```csharp
public readonly struct KeyBinding : IEquatable<KeyBinding>
```

**Implements** `IEquatable<T>`&lt;[`KeyBinding`](../arlecchino.input/KeyBinding.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`KeyBinding(ConsoleKey, KeyModifiers, ConsoleKey, KeyModifiers)`](#keybinding-consolekey-keymodifiers-consolekey-keymodifiers) | A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable. |

## Properties

| Member | Summary |
|---|---|
| [`AlsoKey`](#alsokey) | A second key that triggers the same thing, for actions the platforms disagree about — copying is `Ctrl+Insert` in one habit and `Ctrl+Shift+C` in another. |
| [`AlsoModifiers`](#alsomodifiers) | Modifiers for that second key. |
| [`IsNone`](#isnone) | Whether this binding is unset and therefore matches nothing. |
| [`Key`](#key) | The key itself. |
| [`Modifiers`](#modifiers) | Modifiers that must be held, exactly. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out ConsoleKey, out KeyModifiers, out ConsoleKey, out KeyModifiers)`](#deconstruct-out-consolekey-out-keymodifiers-out-consolekey-out-keymodifiers) |  |
| [`Matches(KeyPress)`](#matches-keypress) | Whether a key press is this binding, either of its combinations. Terminals that report no virtual key are still handled: letters, digits and the common control keys are then matched by the character typed. |
| [`Replacing(KeyModifiers, KeyModifiers)`](#replacing-keymodifiers-keymodifiers) | The same binding with one modifier put in place of another, wherever it appears. This is how an application moves off a modifier its users cannot press — a Mac terminal keeps Option for typing accented characters, so `Alt` never arrives and `Super` is what that keyboard has spare. |
| [`ToString()`](#tostring) | How the binding is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. The palette and the hints box display this, so a rebound key relabels itself everywhere. |

## Constructors in detail

### `KeyBinding(ConsoleKey, KeyModifiers, ConsoleKey, KeyModifiers)` {#keybinding-consolekey-keymodifiers-consolekey-keymodifiers}

```csharp
public KeyBinding(
    ConsoleKey Key,
    KeyModifiers Modifiers = None,
    ConsoleKey AlsoKey = None,
    KeyModifiers AlsoModifiers = None);
```

A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` | The key itself. |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | Modifiers that must be held, exactly. |
| `AlsoKey` | `ConsoleKey` | A second key that triggers the same thing, for actions the platforms disagree about — copying is `Ctrl+Insert` in one habit and `Ctrl+Shift+C` in another. |
| `AlsoModifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | Modifiers for that second key. |

## Properties in detail

### `AlsoKey` {#alsokey}

```csharp
public ConsoleKey AlsoKey { get; init; }
```

A second key that triggers the same thing, for actions the platforms disagree about — copying is `Ctrl+Insert` in one habit and `Ctrl+Shift+C` in another.

**Type** `ConsoleKey`

### `AlsoModifiers` {#alsomodifiers}

```csharp
public KeyModifiers AlsoModifiers { get; init; }
```

Modifiers for that second key.

**Type** [`KeyModifiers`](../arlecchino.input/KeyModifiers.md)

### `IsNone` {#isnone}

```csharp
public bool IsNone { get; }
```

Whether this binding is unset and therefore matches nothing.

**Type** `bool`

### `Key` {#key}

```csharp
public ConsoleKey Key { get; init; }
```

The key itself.

**Type** `ConsoleKey`

### `Modifiers` {#modifiers}

```csharp
public KeyModifiers Modifiers { get; init; }
```

Modifiers that must be held, exactly.

**Type** [`KeyModifiers`](../arlecchino.input/KeyModifiers.md)

## Methods in detail

### `Deconstruct(out ConsoleKey, out KeyModifiers, out ConsoleKey, out KeyModifiers)` {#deconstruct-out-consolekey-out-keymodifiers-out-consolekey-out-keymodifiers}

```csharp
public void Deconstruct(
    out ConsoleKey Key,
    out KeyModifiers Modifiers,
    out ConsoleKey AlsoKey,
    out KeyModifiers AlsoModifiers);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` |  |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) |  |
| `AlsoKey` | `ConsoleKey` |  |
| `AlsoModifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) |  |

### `Matches(KeyPress)` {#matches-keypress}

```csharp
public bool Matches(KeyPress pressed);
```

Whether a key press is this binding, either of its combinations. Terminals that report no virtual key are still handled: letters, digits and the common control keys are then matched by the character typed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `bool` — `true` when the press should trigger this binding.

### `Replacing(KeyModifiers, KeyModifiers)` {#replacing-keymodifiers-keymodifiers}

```csharp
public KeyBinding Replacing(KeyModifiers from, KeyModifiers to);
```

The same binding with one modifier put in place of another, wherever it appears. This is how an application moves off a modifier its users cannot press — a Mac terminal keeps Option for typing accented characters, so `Alt` never arrives and `Super` is what that keyboard has spare.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `from` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | The modifier to take out. |
| `to` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | The modifier to put in its place. |

**Returns** [`KeyBinding`](../arlecchino.input/KeyBinding.md) — The rewritten binding, or this one when the modifier is not in it.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

How the binding is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. The palette and the hints box display this, so a rebound key relabels itself everywhere.

**Returns** `string` — The readable form, or an empty string when the binding is unset.

