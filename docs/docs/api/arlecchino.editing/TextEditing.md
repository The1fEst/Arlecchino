---
title: "TextEditing"
sidebar_label: "TextEditing"
---

# TextEditing class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Editing a line of text: where the caret goes and what each edit does to it, apart from whatever holds the line. A symbol is a grapheme cluster rather than a `char`, so an emoji is rubbed out whole.

```csharp
public static class TextEditing
```

## Methods

| Member | Summary |
|---|---|
| [`Backspace(ITextEntry)`](#backspace-itextentry) | Removes the symbol before the caret, or whatever is selected. A symbol, not a `char`: an emoji or a letter with a combining mark goes in one press rather than being left as half a surrogate pair. |
| [`Delete(ITextEntry)`](#delete-itextentry) | Removes the symbol after the caret, or whatever is selected. |
| [`EraseSelection(ITextEntry)`](#eraseselection-itextentry) | Removes whatever is selected, which is what typing over a selection comes to. |
| [`EraseToStart(ITextEntry)`](#erasetostart-itextentry) | Removes everything before the caret, or whatever is selected. |
| [`EraseWord(ITextEntry)`](#eraseword-itextentry) | Removes the word before the caret, or whatever is selected. |
| [`Insert(ITextEntry, char)`](#insert-itextentry-char) | Puts a character in at the caret and steps past it, over whatever was selected. |
| [`InsertText(ITextEntry, string)`](#inserttext-itextentry-string) | Puts a run of text in at the caret and leaves it after the run, over whatever was selected. Pasting comes to this, so a block arrives as one edit rather than as one edit per character. |
| [`MoveCaret(ITextEntry, int)`](#movecaret-itextentry-int) | Moves the caret by whole symbols, stopping at either end and dropping the selection. |
| [`MoveToEnd(ITextEntry)`](#movetoend-itextentry) | Moves the caret past the last character. |
| [`MoveToStart(ITextEntry)`](#movetostart-itextentry) | Moves the caret to the start of the line. |
| [`MoveWord(ITextEntry, int)`](#moveword-itextentry-int) | Moves the caret a word at a time: to the start of the word behind it, or past the end of the word ahead of it. |
| [`SelectAll(ITextEntry)`](#selectall-itextentry) | Selects the whole line, leaving the caret at the end of it. |
| [`SelectCaret(ITextEntry, int)`](#selectcaret-itextentry-int) | Takes the caret the same way, dragging the selection along behind it. |
| [`SelectToEnd(ITextEntry)`](#selecttoend-itextentry) | Selects from the caret to the end of the line. |
| [`SelectToStart(ITextEntry)`](#selecttostart-itextentry) | Selects from the caret back to the start of the line. |
| [`SelectWord(ITextEntry, int)`](#selectword-itextentry-int) | Takes the caret a word the same way, dragging the selection along behind it. |
| [`Selected(ITextEntry)`](#selected-itextentry) | Whatever is selected, for putting on the clipboard. |
| [`Selection(ITextEntry)`](#selection-itextentry) | What is selected, as the place it starts and the place it ends. |

## Methods in detail

### `Backspace(ITextEntry)` {#backspace-itextentry}

```csharp
public static void Backspace(ITextEntry entry);
```

Removes the symbol before the caret, or whatever is selected. A symbol, not a `char`: an emoji or a letter with a combining mark goes in one press rather than being left as half a surrogate pair.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `Delete(ITextEntry)` {#delete-itextentry}

```csharp
public static void Delete(ITextEntry entry);
```

Removes the symbol after the caret, or whatever is selected.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `EraseSelection(ITextEntry)` {#eraseselection-itextentry}

```csharp
public static bool EraseSelection(ITextEntry entry);
```

Removes whatever is selected, which is what typing over a selection comes to.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

**Returns** `bool` — `true` when something was selected and has gone.

### `EraseToStart(ITextEntry)` {#erasetostart-itextentry}

```csharp
public static void EraseToStart(ITextEntry entry);
```

Removes everything before the caret, or whatever is selected.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `EraseWord(ITextEntry)` {#eraseword-itextentry}

```csharp
public static void EraseWord(ITextEntry entry);
```

Removes the word before the caret, or whatever is selected.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `Insert(ITextEntry, char)` {#insert-itextentry-char}

```csharp
public static void Insert(ITextEntry entry, char character);
```

Puts a character in at the caret and steps past it, over whatever was selected.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `character` | `char` | The character to insert. |

### `InsertText(ITextEntry, string)` {#inserttext-itextentry-string}

```csharp
public static void InsertText(ITextEntry entry, string text);
```

Puts a run of text in at the caret and leaves it after the run, over whatever was selected. Pasting comes to this, so a block arrives as one edit rather than as one edit per character.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `text` | `string` | The text to insert. |

### `MoveCaret(ITextEntry, int)` {#movecaret-itextentry-int}

```csharp
public static void MoveCaret(ITextEntry entry, int delta);
```

Moves the caret by whole symbols, stopping at either end and dropping the selection.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `delta` | `int` | How many symbols to move by; negative goes left. |

### `MoveToEnd(ITextEntry)` {#movetoend-itextentry}

```csharp
public static void MoveToEnd(ITextEntry entry);
```

Moves the caret past the last character.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `MoveToStart(ITextEntry)` {#movetostart-itextentry}

```csharp
public static void MoveToStart(ITextEntry entry);
```

Moves the caret to the start of the line.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `MoveWord(ITextEntry, int)` {#moveword-itextentry-int}

```csharp
public static void MoveWord(ITextEntry entry, int direction);
```

Moves the caret a word at a time: to the start of the word behind it, or past the end of the word ahead of it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `direction` | `int` | Negative to go left, positive to go right. |

### `SelectAll(ITextEntry)` {#selectall-itextentry}

```csharp
public static void SelectAll(ITextEntry entry);
```

Selects the whole line, leaving the caret at the end of it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `SelectCaret(ITextEntry, int)` {#selectcaret-itextentry-int}

```csharp
public static void SelectCaret(ITextEntry entry, int delta);
```

Takes the caret the same way, dragging the selection along behind it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `delta` | `int` | How many symbols to move by; negative goes left. |

### `SelectToEnd(ITextEntry)` {#selecttoend-itextentry}

```csharp
public static void SelectToEnd(ITextEntry entry);
```

Selects from the caret to the end of the line.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `SelectToStart(ITextEntry)` {#selecttostart-itextentry}

```csharp
public static void SelectToStart(ITextEntry entry);
```

Selects from the caret back to the start of the line.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

### `SelectWord(ITextEntry, int)` {#selectword-itextentry-int}

```csharp
public static void SelectWord(ITextEntry entry, int direction);
```

Takes the caret a word the same way, dragging the selection along behind it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `direction` | `int` | Negative to go left, positive to go right. |

### `Selected(ITextEntry)` {#selected-itextentry}

```csharp
public static string Selected(ITextEntry entry);
```

Whatever is selected, for putting on the clipboard.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

**Returns** `string` — The selected text, or an empty string while nothing is selected.

### `Selection(ITextEntry)` {#selection-itextentry}

```csharp
public static ValueTuple<int, int> Selection(ITextEntry entry);
```

What is selected, as the place it starts and the place it ends.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |

**Returns** `ValueTuple<T1, T2>`&lt;`int`, `int`&gt; — The two places, the smaller first; they are equal while nothing is selected.

