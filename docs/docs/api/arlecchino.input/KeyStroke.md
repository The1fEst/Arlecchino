---
title: "KeyStroke"
sidebar_label: "KeyStroke"
---

# KeyStroke struct

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

One key and the modifiers held with it, which is the smallest thing a binding can be made of. A [`KeyBinding`](../arlecchino.input/KeyBinding.md) is one of these plus its alternatives and its finishing key.

```csharp
public readonly struct KeyStroke : IEquatable<KeyStroke>
```

**Implements** `IEquatable<T>`&lt;[`KeyStroke`](../arlecchino.input/KeyStroke.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`KeyStroke(char)`](#keystroke-char) | A stroke on a character rather than on a key, which is the only way to name punctuation. It answers on every layout that can type that character, whatever the keyboard does to produce it. |
| [`KeyStroke(ConsoleKey, KeyModifiers)`](#keystroke-consolekey-keymodifiers) | One key and the modifiers held with it, which is the smallest thing a binding can be made of. A [`KeyBinding`](../arlecchino.input/KeyBinding.md) is one of these plus its alternatives and its finishing key. |

## Properties

| Member | Summary |
|---|---|
| [`IsNone`](#isnone) | Whether the stroke is unset and therefore stands for no key at all. |
| [`Key`](#key) | The key itself. |
| [`Modifiers`](#modifiers) | Modifiers that must be held, exactly. |
| [`Typed`](#typed) | The character this stroke answers to, or `'\0'` when it is a stroke on a key. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out ConsoleKey, out KeyModifiers)`](#deconstruct-out-consolekey-out-keymodifiers) |  |
| [`Matches(KeyPress)`](#matches-keypress) | Whether a key press is this stroke, by the key where one was reported and by the character otherwise. A key that answers to two names matches under either, and a stroke on a character forgives Shift. |
| [`Replacing(KeyModifiers, KeyModifiers)`](#replacing-keymodifiers-keymodifiers) | The same stroke with one modifier put in place of another. |
| [`ToString()`](#tostring) | How the stroke is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. The palette and the hints box display this, so a rebound key relabels itself everywhere. |

## Constructors in detail

### `KeyStroke(char)` {#keystroke-char}

```csharp
public KeyStroke(char typed);
```

A stroke on a character rather than on a key, which is the only way to name punctuation. It answers on every layout that can type that character, whatever the keyboard does to produce it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `typed` | `char` | The character to answer to. |

### `KeyStroke(ConsoleKey, KeyModifiers)` {#keystroke-consolekey-keymodifiers}

```csharp
public KeyStroke(ConsoleKey Key, KeyModifiers Modifiers = None);
```

One key and the modifiers held with it, which is the smallest thing a binding can be made of. A [`KeyBinding`](../arlecchino.input/KeyBinding.md) is one of these plus its alternatives and its finishing key.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` | The key itself. |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | Modifiers that must be held, exactly. |

## Properties in detail

### `IsNone` {#isnone}

```csharp
public bool IsNone { get; }
```

Whether the stroke is unset and therefore stands for no key at all.

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

### `Typed` {#typed}

```csharp
public char Typed { get; }
```

The character this stroke answers to, or `'\0'` when it is a stroke on a key.

**Type** `char`

## Methods in detail

### `Deconstruct(out ConsoleKey, out KeyModifiers)` {#deconstruct-out-consolekey-out-keymodifiers}

```csharp
public void Deconstruct(out ConsoleKey Key, out KeyModifiers Modifiers);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` |  |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) |  |

### `Matches(KeyPress)` {#matches-keypress}

```csharp
public bool Matches(KeyPress pressed);
```

Whether a key press is this stroke, by the key where one was reported and by the character otherwise. A key that answers to two names matches under either, and a stroke on a character forgives Shift.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `bool` — `true` when the press is this combination.

### `Replacing(KeyModifiers, KeyModifiers)` {#replacing-keymodifiers-keymodifiers}

```csharp
public KeyStroke Replacing(KeyModifiers from, KeyModifiers to);
```

The same stroke with one modifier put in place of another.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `from` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | The modifier to take out. |
| `to` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | The modifier to put in its place. |

**Returns** [`KeyStroke`](../arlecchino.input/KeyStroke.md) — The rewritten stroke, or this one when the modifier is not held.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

How the stroke is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. The palette and the hints box display this, so a rebound key relabels itself everywhere.

**Returns** `string` — The readable form, or an empty string when the stroke is unset.

