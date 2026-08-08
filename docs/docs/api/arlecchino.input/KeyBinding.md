---
title: "KeyBinding"
sidebar_label: "KeyBinding"
---

# KeyBinding struct

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable. A binding starts as the one combination it is named after and is added to from there: [`KeyBinding.AddAlternative`](../arlecchino.input/KeyBinding.md#addalternative-consolekey-keymodifiers) for the combinations a platform disagrees about, and [`KeyBinding.ThenKey`](../arlecchino.input/KeyBinding.md#thenkey-consolekey-keymodifiers) for a second keystroke, which turns the binding into a chord.

```csharp
public readonly struct KeyBinding : IEquatable<KeyBinding>
```

**Implements** `IEquatable<T>`&lt;[`KeyBinding`](../arlecchino.input/KeyBinding.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`KeyBinding(ConsoleKey, KeyModifiers)`](#keybinding-consolekey-keymodifiers) | A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable. A binding starts as the one combination it is named after and is added to from there: [`KeyBinding.AddAlternative`](../arlecchino.input/KeyBinding.md#addalternative-consolekey-keymodifiers) for the combinations a platform disagrees about, and [`KeyBinding.ThenKey`](../arlecchino.input/KeyBinding.md#thenkey-consolekey-keymodifiers) for a second keystroke, which turns the binding into a chord. |

## Properties

| Member | Summary |
|---|---|
| [`Alternatives`](#alternatives) | The other combinations that trigger the same thing, in the order they were added. They are matched but never written, since a binding is shown under one name. An alternative is one keystroke even where the binding is a chord, which is how a chord reaches a keyboard the other way round: `Ctrl+G U` for the machine that has to spell it out, and `Ctrl+PgUp` for the one with the key. |
| [`First`](#first) | The combination the binding is named after, and the one it is written from. |
| [`IsChord`](#ischord) | Whether this takes two keystrokes rather than one. A chord is how an application reaches past the modifiers a terminal will give it. A Mac terminal keeps Option for typing and its Command belongs to the window, so what is left are the letters held with Control — and there are not thirty of those. A leader spends one of them and hands back the whole alphabet behind it. |
| [`IsNone`](#isnone) | Whether this binding is unset and therefore matches nothing. |
| [`Key`](#key) | The key itself. |
| [`Modifiers`](#modifiers) | Modifiers that must be held, exactly. |
| [`Second`](#second) | The keystroke that finishes a chord, or `null` when the binding is one press. |

## Methods

| Member | Summary |
|---|---|
| [`AddAlternative(ConsoleKey, KeyModifiers)`](#addalternative-consolekey-keymodifiers) | The same binding, with one more combination that triggers it. Platforms disagree about some of them — copying is `Ctrl+Insert` in one habit and `Ctrl+Shift+C` in another — and a binding that carries both is right on either machine. Call it as often as there are habits. |
| [`Closes(KeyPress)`](#closes-keypress) | Whether a key press is the second half of this chord. |
| [`Deconstruct(out ConsoleKey, out KeyModifiers)`](#deconstruct-out-consolekey-out-keymodifiers) |  |
| [`Equals(KeyBinding)`](#equals-keybinding) | Whether two bindings stand for the same keys. The alternatives count and so does their order, since that is the order they are matched in. |
| [`GetHashCode()`](#gethashcode) | A hash over the same keystrokes equality compares. |
| [`Matches(KeyPress)`](#matches-keypress) | Whether one key press is this whole binding. The combination it is named after counts only when the binding is one keystroke — a chord is opened rather than matched — but an alternative counts either way, since an alternative is always a single press. |
| [`Opens(KeyPress)`](#opens-keypress) | Whether a key press is the first half of this chord. A binding of one keystroke opens nothing: it either matches or it does not. |
| [`Replacing(KeyModifiers, KeyModifiers)`](#replacing-keymodifiers-keymodifiers) | The same binding with one modifier put in place of another, wherever it appears. This is how an application moves off a modifier its users cannot press — a Mac terminal keeps Option for typing accented characters, so `Alt` never arrives and `Super` is what that keyboard has spare. |
| [`ThenKey(ConsoleKey, KeyModifiers)`](#thenkey-consolekey-keymodifiers) | The same binding, finished by a second keystroke pressed after the first one is let go. The leader is expected to say what the group is about — the operations behind one, the places behind another — since what a person remembers is the grouping and not the letters. A binding gets one finishing key: calling this twice replaces it rather than growing a third keystroke, because a chord longer than two is a sequence nobody recalls. |
| [`ToString()`](#tostring) | How the binding is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. A chord is written as its two keystrokes with a space between them, `Ctrl+X T`. |

## Constructors in detail

### `KeyBinding(ConsoleKey, KeyModifiers)` {#keybinding-consolekey-keymodifiers}

```csharp
public KeyBinding(ConsoleKey Key, KeyModifiers Modifiers = None);
```

A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable. A binding starts as the one combination it is named after and is added to from there: [`KeyBinding.AddAlternative`](../arlecchino.input/KeyBinding.md#addalternative-consolekey-keymodifiers) for the combinations a platform disagrees about, and [`KeyBinding.ThenKey`](../arlecchino.input/KeyBinding.md#thenkey-consolekey-keymodifiers) for a second keystroke, which turns the binding into a chord.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` | The key itself. |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | Modifiers that must be held, exactly. |

## Properties in detail

### `Alternatives` {#alternatives}

```csharp
public IReadOnlyList<KeyStroke> Alternatives { get; }
```

The other combinations that trigger the same thing, in the order they were added. They are matched but never written, since a binding is shown under one name. An alternative is one keystroke even where the binding is a chord, which is how a chord reaches a keyboard the other way round: `Ctrl+G U` for the machine that has to spell it out, and `Ctrl+PgUp` for the one with the key.

**Type** `IReadOnlyList<T>`&lt;[`KeyStroke`](../arlecchino.input/KeyStroke.md)&gt;

### `First` {#first}

```csharp
public KeyStroke First { get; }
```

The combination the binding is named after, and the one it is written from.

**Type** [`KeyStroke`](../arlecchino.input/KeyStroke.md)

### `IsChord` {#ischord}

```csharp
public bool IsChord { get; }
```

Whether this takes two keystrokes rather than one. A chord is how an application reaches past the modifiers a terminal will give it. A Mac terminal keeps Option for typing and its Command belongs to the window, so what is left are the letters held with Control — and there are not thirty of those. A leader spends one of them and hands back the whole alphabet behind it.

**Type** `bool`

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

### `Second` {#second}

```csharp
public Nullable<KeyStroke> Second { get; }
```

The keystroke that finishes a chord, or `null` when the binding is one press.

**Type** `Nullable<T>`&lt;[`KeyStroke`](../arlecchino.input/KeyStroke.md)&gt;

## Methods in detail

### `AddAlternative(ConsoleKey, KeyModifiers)` {#addalternative-consolekey-keymodifiers}

```csharp
public KeyBinding AddAlternative(ConsoleKey key, KeyModifiers modifiers = None);
```

The same binding, with one more combination that triggers it. Platforms disagree about some of them — copying is `Ctrl+Insert` in one habit and `Ctrl+Shift+C` in another — and a binding that carries both is right on either machine. Call it as often as there are habits.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKey` | The key of the added combination. |
| `modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | Modifiers that must be held with it, exactly. |

**Returns** [`KeyBinding`](../arlecchino.input/KeyBinding.md) — The binding, with the combination added after the ones already there.

### `Closes(KeyPress)` {#closes-keypress}

```csharp
public bool Closes(KeyPress pressed);
```

Whether a key press is the second half of this chord.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `bool` — `true` when the chord is complete.

### `Deconstruct(out ConsoleKey, out KeyModifiers)` {#deconstruct-out-consolekey-out-keymodifiers}

```csharp
public void Deconstruct(out ConsoleKey Key, out KeyModifiers Modifiers);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Key` | `ConsoleKey` |  |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) |  |

### `Equals(KeyBinding)` {#equals-keybinding}

```csharp
public bool Equals(KeyBinding other);
```

Whether two bindings stand for the same keys. The alternatives count and so does their order, since that is the order they are matched in.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`KeyBinding`](../arlecchino.input/KeyBinding.md) | The binding to compare with. |

**Returns** `bool` — `true` when both are made of the same keystrokes.

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

A hash over the same keystrokes equality compares.

**Returns** `int` — The hash code.

### `Matches(KeyPress)` {#matches-keypress}

```csharp
public bool Matches(KeyPress pressed);
```

Whether one key press is this whole binding. The combination it is named after counts only when the binding is one keystroke — a chord is opened rather than matched — but an alternative counts either way, since an alternative is always a single press.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `bool` — `true` when the press should trigger this binding on its own.

### `Opens(KeyPress)` {#opens-keypress}

```csharp
public bool Opens(KeyPress pressed);
```

Whether a key press is the first half of this chord. A binding of one keystroke opens nothing: it either matches or it does not.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `pressed` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `bool` — `true` when the chord has been started and the next key will finish it.

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

### `ThenKey(ConsoleKey, KeyModifiers)` {#thenkey-consolekey-keymodifiers}

```csharp
public KeyBinding ThenKey(ConsoleKey key, KeyModifiers modifiers = None);
```

The same binding, finished by a second keystroke pressed after the first one is let go. The leader is expected to say what the group is about — the operations behind one, the places behind another — since what a person remembers is the grouping and not the letters. A binding gets one finishing key: calling this twice replaces it rather than growing a third keystroke, because a chord longer than two is a sequence nobody recalls.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKey` | The key that finishes the chord. |
| `modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | Modifiers held with it, which is usually none. |

**Returns** [`KeyBinding`](../arlecchino.input/KeyBinding.md) — The chord.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

How the binding is shown to the user — `Ctrl+S`, `Alt+←`, `Esc`. A chord is written as its two keystrokes with a space between them, `Ctrl+X T`.

**Returns** `string` — The readable form, or an empty string when the binding is unset.

## Example

```csharp
new KeyBinding(ConsoleKey.Insert, KeyModifiers.Control)
.AddAlternative(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);

new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T);

```

