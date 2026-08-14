---
title: "TextAreaModal"
sidebar_label: "TextAreaModal"
---

# TextAreaModal class

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

Several lines of text, edited in place, where `Enter` starts a new line and the `Submit` binding confirms. Every move and edit goes by symbols, so emoji and combining marks survive a backspace.

```csharp
public sealed class TextAreaModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)

## Constructors

| Member | Summary |
|---|---|
| [`TextAreaModal()`](#textareamodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Column`](#column) | Where the caret sits inside its row, as an index into that line. |
| [`FirstVisible`](#firstvisible) | First visible row, kept in step with the caret while drawing. |
| [`Lines`](#lines) | The lines as they stand, top to bottom. |
| [`Message`](#message) | Why the last attempt to submit was refused, drawn under the text. |
| [`OnSubmit`](#onsubmit) | Called with the accepted text. |
| [`Row`](#row) | Row the caret is on. |
| [`Rows`](#rows) | Where the text area was drawn last frame, for turning a click into a caret position. |
| [`Text`](#text) | The whole text, lines joined with a newline. Assigning it puts the caret at the end. |
| [`Validate`](#validate) | Checked when the text is submitted; return a message to keep the dialog open, or `null` to accept. |
| [`VisibleRows`](#visiblerows) | How many rows of text the dialog shows before it starts scrolling. |

## Methods

| Member | Summary |
|---|---|
| [`Break()`](#break) | Splits the current line at the caret, which is what `Enter` does here. |
| [`DeleteForward()`](#deleteforward) | Deletes the symbol after the caret, pulling up the next line when the caret is at the end of a line. |
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Erase()`](#erase) | Deletes the symbol before the caret, joining this line onto the one above when the caret is at the start of a line. |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |
| [`Insert(char)`](#insert-char) | Inserts a character where the caret is. |
| [`InsertText(string)`](#inserttext-string) | Inserts text where the caret is, starting a new line for every newline in it. |
| [`MoveCaret(int, int)`](#movecaret-int-int) | Puts the caret at a row and a position inside it, clamped to what exists. |
| [`MoveLeft()`](#moveleft) | Moves the caret one symbol left, wrapping to the end of the line above. |
| [`MoveRight()`](#moveright) | Moves the caret one symbol right, wrapping to the start of the line below. |
| [`MoveRows(int)`](#moverows-int) | Moves the caret a number of rows, keeping as much of the column as the new row has. |
| [`MoveToLineEnd()`](#movetolineend) | Puts the caret at the end of its line. |
| [`MoveToLineStart()`](#movetolinestart) | Puts the caret at the start of its line. |
| [`SetText(string)`](#settext-string) | Replaces the whole text and puts the caret at its end. |

## Constructors in detail

### `TextAreaModal()` {#textareamodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public TextAreaModal();
```

## Properties in detail

### `Column` {#column}

```csharp
public int Column { get; }
```

Where the caret sits inside its row, as an index into that line.

**Type** `int`

### `FirstVisible` {#firstvisible}

```csharp
public int FirstVisible { get; set; }
```

First visible row, kept in step with the caret while drawing.

**Type** `int`

### `Lines` {#lines}

```csharp
public IReadOnlyList<string> Lines { get; }
```

The lines as they stand, top to bottom.

**Type** `IReadOnlyList<T>`&lt;`string`&gt;

### `Message` {#message}

```csharp
public string Message { get; set; }
```

Why the last attempt to submit was refused, drawn under the text.

**Type** `string`

### `OnSubmit` {#onsubmit}

```csharp
public Action<string> OnSubmit { get; init; }
```

Called with the accepted text.

**Type** `Action<T>`&lt;`string`&gt;

### `Row` {#row}

```csharp
public int Row { get; }
```

Row the caret is on.

**Type** `int`

### `Rows` {#rows}

```csharp
public SurfaceRegion Rows { get; set; }
```

Where the text area was drawn last frame, for turning a click into a caret position.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Text` {#text}

```csharp
public string Text { get; init; }
```

The whole text, lines joined with a newline. Assigning it puts the caret at the end.

**Type** `string`

### `Validate` {#validate}

```csharp
public Func<string, string?>? Validate { get; init; }
```

Checked when the text is submitted; return a message to keep the dialog open, or `null` to accept.

**Type** `Func<T, TResult>`&lt;`string`, `string`&gt;

### `VisibleRows` {#visiblerows}

```csharp
public int VisibleRows { get; init; }
```

How many rows of text the dialog shows before it starts scrolling.

**Type** `int`

## Methods in detail

### `Break()` {#break}

```csharp
public void Break();
```

Splits the current line at the caret, which is what `Enter` does here.

### `DeleteForward()` {#deleteforward}

```csharp
public void DeleteForward();
```

Deletes the symbol after the caret, pulling up the next line when the caret is at the end of a line.

### `Draw(ModalFrame)` {#draw-modalframe}

```csharp
public override void Draw(ModalFrame frame);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |

### `Erase()` {#erase}

```csharp
public void Erase();
```

Deletes the symbol before the caret, joining this line onto the one above when the caret is at the start of a line.

### `Handle(ModalFrame, KeyPress)` {#handle-modalframe-keypress}

```csharp
public override void Handle(ModalFrame frame, KeyPress key);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) |  |

### `Insert(char)` {#insert-char}

```csharp
public void Insert(char character);
```

Inserts a character where the caret is.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `character` | `char` | What to insert. |

### `InsertText(string)` {#inserttext-string}

```csharp
public void InsertText(string text);
```

Inserts text where the caret is, starting a new line for every newline in it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to insert. |

### `MoveCaret(int, int)` {#movecaret-int-int}

```csharp
public void MoveCaret(int row, int column);
```

Puts the caret at a row and a position inside it, clamped to what exists.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row to move to. |
| `column` | `int` | Index inside that row. |

### `MoveLeft()` {#moveleft}

```csharp
public void MoveLeft();
```

Moves the caret one symbol left, wrapping to the end of the line above.

### `MoveRight()` {#moveright}

```csharp
public void MoveRight();
```

Moves the caret one symbol right, wrapping to the start of the line below.

### `MoveRows(int)` {#moverows-int}

```csharp
public void MoveRows(int rows);
```

Moves the caret a number of rows, keeping as much of the column as the new row has.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `rows` | `int` | How far to move; negative goes up. |

### `MoveToLineEnd()` {#movetolineend}

```csharp
public void MoveToLineEnd();
```

Puts the caret at the end of its line.

### `MoveToLineStart()` {#movetolinestart}

```csharp
public void MoveToLineStart();
```

Puts the caret at the start of its line.

### `SetText(string)` {#settext-string}

```csharp
public void SetText(string text);
```

Replaces the whole text and puts the caret at its end.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to hold. |

