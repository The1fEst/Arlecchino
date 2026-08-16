---
title: "TextAreaModal"
sidebar_label: "TextAreaModal"
---

# TextAreaModal class

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

Several lines of text, edited in place, where `Enter` starts a new line and the `Submit` binding confirms. The text is one line with newlines in it, so every edit is the one a field of one line has.

```csharp
public sealed class TextAreaModal : Modal, ITextEntry
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)  
**Implements** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

## Constructors

| Member | Summary |
|---|---|
| [`TextAreaModal()`](#textareamodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Anchor`](#anchor) | Where the selection was started from, on the caret while nothing is selected. |
| [`Caret`](#caret) | Where the caret sits, counted from the start of the whole text. |
| [`Column`](#column) | Where the caret sits inside its row, as an index into that line. |
| [`FirstVisible`](#firstvisible) | First visible row, kept in step with the caret while drawing. |
| [`Lines`](#lines) | The lines as they stand, top to bottom. |
| [`Message`](#message) | Why the last attempt to submit was refused, drawn under the text. |
| [`OnSubmit`](#onsubmit) | Called with the accepted text. |
| [`Row`](#row) | Row the caret is on. |
| [`Rows`](#rows) | Where the text area was drawn last frame, for turning a click into a caret position. |
| [`Text`](#text) | The whole text, lines joined with a newline. Assigning it puts the caret at the end. |
| [`Typing`](#typing) | The line being typed into, which here is the whole text with its newlines in it. |
| [`Validate`](#validate) | Checked when the text is submitted; return a message to keep the dialog open, or `null` to accept. |
| [`VisibleRows`](#visiblerows) | How many rows of text the dialog shows before it starts scrolling. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |
| [`HandlePaste(ModalFrame, string)`](#handlepaste-modalframe-string) | Takes pasted text whole, line breaks included, since this is the one dialog that holds more than one row of it. |
| [`InsertText(string)`](#inserttext-string) | Inserts text where the caret is, over whatever was selected. |
| [`MoveCaret(int, int)`](#movecaret-int-int) | Puts the caret at a row and a position inside it, clamped to what exists. |
| [`MoveRows(int)`](#moverows-int) | Moves the caret a number of rows, keeping as much of the column as the new row has. |
| [`MoveToLineEnd()`](#movetolineend) | Puts the caret at the end of its line. |
| [`MoveToLineStart()`](#movetolinestart) | Puts the caret at the start of its line. |
| [`SelectRows(int)`](#selectrows-int) | Takes the selection a number of rows, dragging it along behind the caret. |
| [`SelectToLineEnd()`](#selecttolineend) | Takes the selection on to the end of the line. |
| [`SelectToLineStart()`](#selecttolinestart) | Takes the selection back to the start of the line. |
| [`StartOf(int)`](#startof-int) | Where in the whole text a row begins. |

## Constructors in detail

### `TextAreaModal()` {#textareamodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public TextAreaModal();
```

## Properties in detail

### `Anchor` {#anchor}

```csharp
public int Anchor { get; set; }
```

Where the selection was started from, on the caret while nothing is selected.

**Type** `int`

### `Caret` {#caret}

```csharp
public int Caret { get; set; }
```

Where the caret sits, counted from the start of the whole text.

**Type** `int`

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
public string Text { get; set; }
```

The whole text, lines joined with a newline. Assigning it puts the caret at the end.

**Type** `string`

### `Typing` {#typing}

```csharp
public override ITextEntry Typing { get; }
```

The line being typed into, which here is the whole text with its newlines in it.

**Type** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

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

### `Draw(ModalFrame)` {#draw-modalframe}

```csharp
public override void Draw(ModalFrame frame);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |

### `Handle(ModalFrame, KeyPress)` {#handle-modalframe-keypress}

```csharp
public override void Handle(ModalFrame frame, KeyPress key);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) |  |

### `HandlePaste(ModalFrame, string)` {#handlepaste-modalframe-string}

```csharp
public override void HandlePaste(ModalFrame frame, string text);
```

Takes pasted text whole, line breaks included, since this is the one dialog that holds more than one row of it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) | The keys to obey, and how to close. |
| `text` | `string` | What was pasted. |

### `InsertText(string)` {#inserttext-string}

```csharp
public void InsertText(string text);
```

Inserts text where the caret is, over whatever was selected.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to insert; a newline in it starts a new line. |

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

### `SelectRows(int)` {#selectrows-int}

```csharp
public void SelectRows(int rows);
```

Takes the selection a number of rows, dragging it along behind the caret.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `rows` | `int` | How far to take it; negative goes up. |

### `SelectToLineEnd()` {#selecttolineend}

```csharp
public void SelectToLineEnd();
```

Takes the selection on to the end of the line.

### `SelectToLineStart()` {#selecttolinestart}

```csharp
public void SelectToLineStart();
```

Takes the selection back to the start of the line.

### `StartOf(int)` {#startof-int}

```csharp
public int StartOf(int row);
```

Where in the whole text a row begins.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | The row, clamped to the rows there are. |

**Returns** `int` — The index of the first character on it.

