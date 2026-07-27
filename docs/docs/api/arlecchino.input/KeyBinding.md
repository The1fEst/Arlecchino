---
title: KeyBinding
sidebar_label: KeyBinding
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
| [`KeyBinding(ConsoleKey, ConsoleModifiers, ConsoleKey, ConsoleModifiers)`](#keybinding-consolekey-consolemodifiers-consolekey-consolemodifiers) | A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable. |

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
| [`Deconstruct(ConsoleKey&, ConsoleModifiers&, ConsoleKey&, ConsoleModifiers&)`](#deconstruct-consolekey-consolemodifiers-consolekey-consolemodifiers) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(KeyBinding)`](#equals-keybinding) |  |
| [`GetHashCode()`](#gethashcode) |  |
| [`Matches(ConsoleKeyInfo)`](#matches-consolekeyinfo) | Whether a key press is this binding, either of its combinations. Terminals that report no virtual key are still handled: letters, digits and the common control keys are then matched by the character typed. |
| [`ToString()`](#tostring) | How the binding is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. The palette and the hints box display this, so a rebound key relabels itself everywhere. |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(KeyBinding, KeyBinding)`](#operator-inequality-keybinding-keybinding) |  |
| [`operator Equality(KeyBinding, KeyBinding)`](#operator-equality-keybinding-keybinding) |  |

## Constructors in detail

### `KeyBinding(ConsoleKey, ConsoleModifiers, ConsoleKey, ConsoleModifiers)` {#keybinding-consolekey-consolemodifiers-consolekey-consolemodifiers}

```csharp
public KeyBinding(ConsoleKey Key, ConsoleModifiers Modifiers = None, ConsoleKey AlsoKey = None, ConsoleModifiers AlsoModifiers = None);
```

A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` | The key itself. |
| `Modifiers` | `ConsoleModifiers` | Modifiers that must be held, exactly. |
| `AlsoKey` | `ConsoleKey` | A second key that triggers the same thing, for actions the platforms disagree about — copying is `Ctrl+Insert` in one habit and `Ctrl+Shift+C` in another. |
| `AlsoModifiers` | `ConsoleModifiers` | Modifiers for that second key. |

## Properties in detail

### `AlsoKey` {#alsokey}

```csharp
public ConsoleKey AlsoKey { get; init; }
```

A second key that triggers the same thing, for actions the platforms disagree about — copying is `Ctrl+Insert` in one habit and `Ctrl+Shift+C` in another.

**Type** `ConsoleKey`

### `AlsoModifiers` {#alsomodifiers}

```csharp
public ConsoleModifiers AlsoModifiers { get; init; }
```

Modifiers for that second key.

**Type** `ConsoleModifiers`

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
public ConsoleModifiers Modifiers { get; init; }
```

Modifiers that must be held, exactly.

**Type** `ConsoleModifiers`

## Methods in detail

### `Deconstruct(ConsoleKey&, ConsoleModifiers&, ConsoleKey&, ConsoleModifiers&)` {#deconstruct-consolekey-consolemodifiers-consolekey-consolemodifiers}

```csharp
public void Deconstruct(out ConsoleKey Key, out ConsoleModifiers Modifiers, out ConsoleKey AlsoKey, out ConsoleModifiers AlsoModifiers);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` |  |
| `Modifiers` | `ConsoleModifiers` |  |
| `AlsoKey` | `ConsoleKey` |  |
| `AlsoModifiers` | `ConsoleModifiers` |  |

### `Equals(object)` {#equals-object}

```csharp
public virtual bool Equals(object obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(KeyBinding)` {#equals-keybinding}

```csharp
public bool Equals(KeyBinding other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`KeyBinding`](../arlecchino.input/KeyBinding.md) |  |

**Returns** `bool`

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

**Returns** `int`

### `Matches(ConsoleKeyInfo)` {#matches-consolekeyinfo}

```csharp
public bool Matches(ConsoleKeyInfo pressed);
```

Whether a key press is this binding, either of its combinations. Terminals that report no virtual key are still handled: letters, digits and the common control keys are then matched by the character typed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | `ConsoleKeyInfo` | The key that was pressed. |

**Returns** `bool` — `true` when the press should trigger this binding.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

How the binding is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. The palette and the hints box display this, so a rebound key relabels itself everywhere.

**Returns** `string` — The readable form, or an empty string when the binding is unset.

## Operators in detail

### `operator Inequality(KeyBinding, KeyBinding)` {#operator-inequality-keybinding-keybinding}

```csharp
public static bool op_Inequality(KeyBinding left, KeyBinding right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`KeyBinding`](../arlecchino.input/KeyBinding.md) |  |
| `right` | [`KeyBinding`](../arlecchino.input/KeyBinding.md) |  |

**Returns** `bool`

### `operator Equality(KeyBinding, KeyBinding)` {#operator-equality-keybinding-keybinding}

```csharp
public static bool op_Equality(KeyBinding left, KeyBinding right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`KeyBinding`](../arlecchino.input/KeyBinding.md) |  |
| `right` | [`KeyBinding`](../arlecchino.input/KeyBinding.md) |  |

**Returns** `bool`

