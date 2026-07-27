---
title: TextEditing
sidebar_label: TextEditing
---

# TextEditing class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

Editing a line of text: where the caret goes and what each edit does to it. Kept apart from the fields themselves so the text field, the number field and anything added later behave identically, and so the behaviour can be tested without a terminal. Editing never touches the validation message — that is the router's job, which re-checks the field and clears the message only once the input is actually valid.

```csharp
public static class TextEditing
```

## Methods

| Member | Summary |
|---|---|
| [`Backspace(ITextEntryModal)`](#backspace-itextentrymodal) | Removes the symbol before the caret, doing nothing at the start of the line. A symbol, not a `char`: an emoji or a letter with a combining mark goes in one press rather than being left as half a surrogate pair. |
| [`Delete(ITextEntryModal)`](#delete-itextentrymodal) | Removes the symbol after the caret, leaving the caret where it is. |
| [`EraseToStart(ITextEntryModal)`](#erasetostart-itextentrymodal) | Removes everything before the caret, which is how a field is retyped from scratch. |
| [`EraseWord(ITextEntryModal)`](#eraseword-itextentrymodal) | Removes everything from the start of the word before the caret up to the caret. |
| [`Insert(ITextEntryModal, char)`](#insert-itextentrymodal-char) | Puts a character in at the caret and steps past it. |
| [`MoveCaret(ITextEntryModal, int)`](#movecaret-itextentrymodal-int) | Moves the caret by whole symbols, stopping at either end. |
| [`MoveToEnd(ITextEntryModal)`](#movetoend-itextentrymodal) | Moves the caret past the last character. |
| [`MoveToStart(ITextEntryModal)`](#movetostart-itextentrymodal) | Moves the caret to the start of the line. |
| [`MoveWord(ITextEntryModal, int)`](#moveword-itextentrymodal-int) | Moves the caret a word at a time: to the start of the word behind it, or past the end of the word ahead of it. |

## Methods in detail

### `Backspace(ITextEntryModal)` {#backspace-itextentrymodal}

```csharp
public static void Backspace(ITextEntryModal modal);
```

Removes the symbol before the caret, doing nothing at the start of the line. A symbol, not a `char`: an emoji or a letter with a combining mark goes in one press rather than being left as half a surrogate pair.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |

### `Delete(ITextEntryModal)` {#delete-itextentrymodal}

```csharp
public static void Delete(ITextEntryModal modal);
```

Removes the symbol after the caret, leaving the caret where it is.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |

### `EraseToStart(ITextEntryModal)` {#erasetostart-itextentrymodal}

```csharp
public static void EraseToStart(ITextEntryModal modal);
```

Removes everything before the caret, which is how a field is retyped from scratch.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |

### `EraseWord(ITextEntryModal)` {#eraseword-itextentrymodal}

```csharp
public static void EraseWord(ITextEntryModal modal);
```

Removes everything from the start of the word before the caret up to the caret.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |

### `Insert(ITextEntryModal, char)` {#insert-itextentrymodal-char}

```csharp
public static void Insert(ITextEntryModal modal, char character);
```

Puts a character in at the caret and steps past it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |
| `character` | `char` | The character to insert. |

### `MoveCaret(ITextEntryModal, int)` {#movecaret-itextentrymodal-int}

```csharp
public static void MoveCaret(ITextEntryModal modal, int delta);
```

Moves the caret by whole symbols, stopping at either end.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |
| `delta` | `int` | How many symbols to move by; negative goes left. |

### `MoveToEnd(ITextEntryModal)` {#movetoend-itextentrymodal}

```csharp
public static void MoveToEnd(ITextEntryModal modal);
```

Moves the caret past the last character.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |

### `MoveToStart(ITextEntryModal)` {#movetostart-itextentrymodal}

```csharp
public static void MoveToStart(ITextEntryModal modal);
```

Moves the caret to the start of the line.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |

### `MoveWord(ITextEntryModal, int)` {#moveword-itextentrymodal-int}

```csharp
public static void MoveWord(ITextEntryModal modal, int direction);
```

Moves the caret a word at a time: to the start of the word behind it, or past the end of the word ahead of it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`ITextEntryModal`](../arlecchino.modals/ITextEntryModal.md) | The field being edited. |
| `direction` | `int` | Negative to go left, positive to go right. |

