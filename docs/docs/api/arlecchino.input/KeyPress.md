---
title: "KeyPress"
sidebar_label: "KeyPress"
---

# KeyPress struct

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

One key press, as the framework hands it to a view. It is `ConsoleKeyInfo` with room for Command, which that type has nowhere to put.

```csharp
public readonly struct KeyPress : IEquatable<KeyPress>
```

**Implements** `IEquatable<T>`&lt;[`KeyPress`](../arlecchino.input/KeyPress.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`KeyPress(char)`](#keypress-char) | Creates a press the terminal reported as a character with no key behind it. |
| [`KeyPress(ConsoleKey, KeyModifiers, char)`](#keypress-consolekey-keymodifiers-char) | One key press, as the framework hands it to a view. It is `ConsoleKeyInfo` with room for Command, which that type has nowhere to put. |

## Properties

| Member | Summary |
|---|---|
| [`Character`](#character) | The character it typed, or `'\0'` for keys that type nothing. |
| [`IsNothing`](#isnothing) | Whether this is the press a terminal hands back when there was nothing to read — no key, no character, nothing held. |
| [`Key`](#key) | The key itself, or `default` when the terminal named no key and only sent a character. |
| [`Modifiers`](#modifiers) | What was held with it. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out ConsoleKey, out KeyModifiers, out char)`](#deconstruct-out-consolekey-out-keymodifiers-out-char) |  |
| [`From(ConsoleKeyInfo)`](#from-consolekeyinfo) | Takes over what a console reported. Nothing is lost: the console cannot report Command in the first place, which is the whole reason this type exists. |

## Constructors in detail

### `KeyPress(char)` {#keypress-char}

```csharp
public KeyPress(char character);
```

Creates a press the terminal reported as a character with no key behind it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `character` | `char` | The character that arrived. |

### `KeyPress(ConsoleKey, KeyModifiers, char)` {#keypress-consolekey-keymodifiers-char}

```csharp
public KeyPress(ConsoleKey Key, KeyModifiers Modifiers = None, char Character = '\0');
```

One key press, as the framework hands it to a view. It is `ConsoleKeyInfo` with room for Command, which that type has nowhere to put.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` | The key itself, or `default` when the terminal named no key and only sent a character. |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | What was held with it. |
| `Character` | `char` | The character it typed, or `'\0'` for keys that type nothing. |

## Properties in detail

### `Character` {#character}

```csharp
public char Character { get; init; }
```

The character it typed, or `'\0'` for keys that type nothing.

**Type** `char`

### `IsNothing` {#isnothing}

```csharp
public bool IsNothing { get; }
```

Whether this is the press a terminal hands back when there was nothing to read — no key, no character, nothing held.

**Type** `bool`

### `Key` {#key}

```csharp
public ConsoleKey Key { get; init; }
```

The key itself, or `default` when the terminal named no key and only sent a character.

**Type** `ConsoleKey`

### `Modifiers` {#modifiers}

```csharp
public KeyModifiers Modifiers { get; init; }
```

What was held with it.

**Type** [`KeyModifiers`](../arlecchino.input/KeyModifiers.md)

## Methods in detail

### `Deconstruct(out ConsoleKey, out KeyModifiers, out char)` {#deconstruct-out-consolekey-out-keymodifiers-out-char}

```csharp
public void Deconstruct(out ConsoleKey Key, out KeyModifiers Modifiers, out char Character);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` |  |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) |  |
| `Character` | `char` |  |

### `From(ConsoleKeyInfo)` {#from-consolekeyinfo}

```csharp
public static KeyPress From(ConsoleKeyInfo key);
```

Takes over what a console reported. Nothing is lost: the console cannot report Command in the first place, which is the whole reason this type exists.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | What the console handed over. |

**Returns** [`KeyPress`](../arlecchino.input/KeyPress.md) — The same press.

