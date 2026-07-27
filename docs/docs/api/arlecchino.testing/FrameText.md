---
title: FrameText
sidebar_label: FrameText
---

# FrameText class

**Namespace:** `Arlecchino.Testing` &middot; **Assembly:** `Arlecchino.Testing`

Pulls apart what was written to a terminal. A frame is text with escape sequences woven through it, which is unreadable in an assertion message, so these separate the content from the styling and let a test assert on either.

```csharp
public static class FrameText
```

## Methods

| Member | Summary |
|---|---|
| [`AnsiSequence()`](#ansisequence) |  |
| [`BoxWidth(string)`](#boxwidth-string) | How wide a box is on one row, measured between its border characters. Useful for checking that a dialog grew to fit its content. |
| [`CursorJump()`](#cursorjump) |  |
| [`CursorJumpsIn(string)`](#cursorjumpsin-string) | The cursor moves in order. Since only what changed is redrawn, counting these is how a test shows that a frame touched a few cells rather than the whole screen. |
| [`Lines(string)`](#lines-string) | The frame as plain rows. |
| [`StyleSequence()`](#stylesequence) |  |
| [`StylesIn(string)`](#stylesin-string) | The colour sequences in order, for asserting that something was drawn as a warning. |
| [`WithoutStyles(string)`](#withoutstyles-string) | Strips every escape sequence, leaving what the user would actually read. |

## Methods in detail

### `AnsiSequence()` {#ansisequence}

```csharp
public static Regex AnsiSequence();
```

**Returns** `Regex`

Pattern:

```csharp
\\x1b\\[[0-9;?]*[a-zA-Z]
```

Explanation:

```csharp
○ Match the string "\u001b[".
○ Match a character in the set [0-9;?] atomically any number of times.
○ Match an ASCII letter.
```

### `BoxWidth(string)` {#boxwidth-string}

```csharp
public static int BoxWidth(string line);
```

How wide a box is on one row, measured between its border characters. Useful for checking that a dialog grew to fit its content.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `line` | `string` | A plain row, with styles already stripped. |

**Returns** `int` — The width in columns, or `-1` when the row holds no box.

### `CursorJump()` {#cursorjump}

```csharp
public static Regex CursorJump();
```

**Returns** `Regex`

Pattern:

```csharp
\\x1b\\[\\d+;\\d+H
```

Explanation:

```csharp
○ Match the string "\u001b[".
○ Match a Unicode digit atomically at least once.
○ Match ';'.
○ Match a Unicode digit atomically at least once.
○ Match 'H'.
```

### `CursorJumpsIn(string)` {#cursorjumpsin-string}

```csharp
public static List<string> CursorJumpsIn(string text);
```

The cursor moves in order. Since only what changed is redrawn, counting these is how a test shows that a frame touched a few cells rather than the whole screen.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The frame. |

**Returns** `List<T>`&lt;`string`&gt; — The sequences as they appeared.

### `Lines(string)` {#lines-string}

```csharp
public static string[] Lines(string text);
```

The frame as plain rows.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The frame. |

**Returns** `string`\[\] — One string per row.

### `StyleSequence()` {#stylesequence}

```csharp
public static Regex StyleSequence();
```

**Returns** `Regex`

Pattern:

```csharp
\\x1b\\[[0-9;]*m
```

Explanation:

```csharp
○ Match the string "\u001b[".
○ Match a character in the set [0-9;] atomically any number of times.
○ Match 'm'.
```

### `StylesIn(string)` {#stylesin-string}

```csharp
public static List<string> StylesIn(string text);
```

The colour sequences in order, for asserting that something was drawn as a warning.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The frame. |

**Returns** `List<T>`&lt;`string`&gt; — The sequences as they appeared.

### `WithoutStyles(string)` {#withoutstyles-string}

```csharp
public static string WithoutStyles(string text);
```

Strips every escape sequence, leaving what the user would actually read.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The frame. |

**Returns** `string` — The frame as plain text.

