---
title: "ArlecchinoKeymap"
sidebar_label: "ArlecchinoKeymap"
---

# ArlecchinoKeymap class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Every key the framework itself reacts to, in one place. Replace the whole map through `UseKeymap`, or one binding at a time with `with`; each binding also relabels itself in the hints box and the palette.

```csharp
public sealed class ArlecchinoKeymap : IEquatable<ArlecchinoKeymap>
```

**Implements** `IEquatable<T>`&lt;[`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`ArlecchinoKeymap()`](#arlecchinokeymap) |  |

## Properties

| Member | Summary |
|---|---|
| [`Back`](#back) | Goes back in the history. `Alt+←` by default. |
| [`Cancel`](#cancel) | Dismisses a dialog or leaves a screen. `Esc` by default. |
| [`Confirm`](#confirm) | Accepts a dialog, opens a field, activates a row. `Enter` by default. |
| [`Copy`](#copy) | Copies what is being edited to the clipboard, under both habits: `Ctrl+Insert` and `Ctrl+Shift+C`. Plain `Ctrl+C` is left alone, since it stops the application. |
| [`DeleteForward`](#deleteforward) | Deletes the character after the caret. `Delete` by default. |
| [`Erase`](#erase) | Deletes: a character, a filter, a typed segment, a field value. `Backspace` by default. |
| [`EraseToStart`](#erasetostart) | Deletes everything before the caret. `Ctrl+U` by default, as in a shell. |
| [`EraseWord`](#eraseword) | Deletes the word before the caret. `Ctrl+Backspace` by default. |
| [`First`](#first) | Goes to the start of a list or the minimum of a range. `Home` by default. |
| [`Forward`](#forward) | Retraces a step back. `Alt+→` by default. |
| [`Help`](#help) | Opens the screen listing every key. `F1` by default. |
| [`JumpDown`](#jumpdown) | A long stride downward, or a page of rows. `PgDn` by default. |
| [`JumpUp`](#jumpup) | A long stride upward, or a page of rows. `PgUp` by default. |
| [`Last`](#last) | Goes to the end of a list or the maximum of a range. `End` by default. |
| [`Mark`](#mark) | Marks a row or flips a toggle. `Space` by default. |
| [`MoveDown`](#movedown) | Moves the cursor down, or steps a number down. `↓` by default. |
| [`MoveLeft`](#moveleft) | Moves left: a slider down, a tree node closed, out of a folder. `←` by default. |
| [`MoveRight`](#moveright) | Moves right: a slider up, a tree node open, into a folder. `→` by default. |
| [`MoveUp`](#moveup) | Moves the cursor up, or steps a number up. `↑` by default. |
| [`NextField`](#nextfield) | Moves to the next pane, segment or channel. `Tab` by default. |
| [`Notifications`](#notifications) | Opens the screen listing what the application has said lately. |
| [`PickCurrentFolder`](#pickcurrentfolder) | Picks the folder open in the file picker. `Ctrl+Enter` by default. |
| [`PreviousField`](#previousfield) | Moves to the previous one. `Shift+Tab` by default. |
| [`Submit`](#submit) | Accepts a dialog where `Enter` means something else — the multi-line text area, where it starts a new line. `Ctrl+Enter` by default. |
| [`ToggleLog`](#togglelog) | Shows or hides the log overlay. `Ctrl+L` by default. |
| [`WordLeft`](#wordleft) | Moves the caret to the previous word. `Ctrl+←` by default. |
| [`WordRight`](#wordright) | Moves the caret past the next word. `Ctrl+→` by default. |

## Methods

| Member | Summary |
|---|---|
| [`Replacing(KeyModifiers, KeyModifiers)`](#replacing-keymodifiers-keymodifiers) | The whole map with one modifier put in place of another, for a keyboard that cannot send the one the bindings were written on. Rewriting thirty bindings by hand leaves twenty-eight rewritten. |

## Constructors in detail

### `ArlecchinoKeymap()` {#arlecchinokeymap}

```csharp
public ArlecchinoKeymap();
```

## Properties in detail

### `Back` {#back}

```csharp
public KeyBinding Back { get; init; }
```

Goes back in the history. `Alt+←` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Cancel` {#cancel}

```csharp
public KeyBinding Cancel { get; init; }
```

Dismisses a dialog or leaves a screen. `Esc` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Confirm` {#confirm}

```csharp
public KeyBinding Confirm { get; init; }
```

Accepts a dialog, opens a field, activates a row. `Enter` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Copy` {#copy}

```csharp
public KeyBinding Copy { get; init; }
```

Copies what is being edited to the clipboard, under both habits: `Ctrl+Insert` and `Ctrl+Shift+C`. Plain `Ctrl+C` is left alone, since it stops the application.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `DeleteForward` {#deleteforward}

```csharp
public KeyBinding DeleteForward { get; init; }
```

Deletes the character after the caret. `Delete` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Erase` {#erase}

```csharp
public KeyBinding Erase { get; init; }
```

Deletes: a character, a filter, a typed segment, a field value. `Backspace` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `EraseToStart` {#erasetostart}

```csharp
public KeyBinding EraseToStart { get; init; }
```

Deletes everything before the caret. `Ctrl+U` by default, as in a shell.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `EraseWord` {#eraseword}

```csharp
public KeyBinding EraseWord { get; init; }
```

Deletes the word before the caret. `Ctrl+Backspace` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `First` {#first}

```csharp
public KeyBinding First { get; init; }
```

Goes to the start of a list or the minimum of a range. `Home` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Forward` {#forward}

```csharp
public KeyBinding Forward { get; init; }
```

Retraces a step back. `Alt+→` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Help` {#help}

```csharp
public KeyBinding Help { get; init; }
```

Opens the screen listing every key. `F1` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `JumpDown` {#jumpdown}

```csharp
public KeyBinding JumpDown { get; init; }
```

A long stride downward, or a page of rows. `PgDn` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `JumpUp` {#jumpup}

```csharp
public KeyBinding JumpUp { get; init; }
```

A long stride upward, or a page of rows. `PgUp` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Last` {#last}

```csharp
public KeyBinding Last { get; init; }
```

Goes to the end of a list or the maximum of a range. `End` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Mark` {#mark}

```csharp
public KeyBinding Mark { get; init; }
```

Marks a row or flips a toggle. `Space` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `MoveDown` {#movedown}

```csharp
public KeyBinding MoveDown { get; init; }
```

Moves the cursor down, or steps a number down. `↓` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `MoveLeft` {#moveleft}

```csharp
public KeyBinding MoveLeft { get; init; }
```

Moves left: a slider down, a tree node closed, out of a folder. `←` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `MoveRight` {#moveright}

```csharp
public KeyBinding MoveRight { get; init; }
```

Moves right: a slider up, a tree node open, into a folder. `→` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `MoveUp` {#moveup}

```csharp
public KeyBinding MoveUp { get; init; }
```

Moves the cursor up, or steps a number up. `↑` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `NextField` {#nextfield}

```csharp
public KeyBinding NextField { get; init; }
```

Moves to the next pane, segment or channel. `Tab` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Notifications` {#notifications}

```csharp
public KeyBinding Notifications { get; init; }
```

Opens the screen listing what the application has said lately.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `PickCurrentFolder` {#pickcurrentfolder}

```csharp
public KeyBinding PickCurrentFolder { get; init; }
```

Picks the folder open in the file picker. `Ctrl+Enter` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `PreviousField` {#previousfield}

```csharp
public KeyBinding PreviousField { get; init; }
```

Moves to the previous one. `Shift+Tab` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Submit` {#submit}

```csharp
public KeyBinding Submit { get; init; }
```

Accepts a dialog where `Enter` means something else — the multi-line text area, where it starts a new line. `Ctrl+Enter` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `ToggleLog` {#togglelog}

```csharp
public KeyBinding ToggleLog { get; init; }
```

Shows or hides the log overlay. `Ctrl+L` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `WordLeft` {#wordleft}

```csharp
public KeyBinding WordLeft { get; init; }
```

Moves the caret to the previous word. `Ctrl+←` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `WordRight` {#wordright}

```csharp
public KeyBinding WordRight { get; init; }
```

Moves the caret past the next word. `Ctrl+→` by default.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

## Methods in detail

### `Replacing(KeyModifiers, KeyModifiers)` {#replacing-keymodifiers-keymodifiers}

```csharp
public ArlecchinoKeymap Replacing(KeyModifiers from, KeyModifiers to);
```

The whole map with one modifier put in place of another, for a keyboard that cannot send the one the bindings were written on. Rewriting thirty bindings by hand leaves twenty-eight rewritten.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `from` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | The modifier to take out. |
| `to` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | The modifier to put in its place. |

**Returns** [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) — A new map, with every binding rewritten.

