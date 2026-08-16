---
title: "PastedText"
sidebar_label: "PastedText"
---

# PastedText class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

What a block of pasted text comes to where it lands. A line of one row takes the first line of it, since what was on the clipboard does not turn one row into several.

```csharp
public static class PastedText
```

## Methods

| Member | Summary |
|---|---|
| [`FirstLine(string)`](#firstline-string) | The first line of what was pasted, with the line breaks and everything after them gone. |
| [`FirstLine(string, Func<char, bool>)`](#firstline-string-func-char-bool) | The first line of what was pasted, with the characters the line refuses left out. What was on the clipboard does not widen what a field accepts. |

## Methods in detail

### `FirstLine(string)` {#firstline-string}

```csharp
public static string FirstLine(string text);
```

The first line of what was pasted, with the line breaks and everything after them gone.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was pasted. |

**Returns** `string` — The first line, or the whole text when it holds no line break.

### `FirstLine(string, Func<char, bool>)` {#firstline-string-func-char-bool}

```csharp
public static string FirstLine(string text, Func<char, bool> accepts);
```

The first line of what was pasted, with the characters the line refuses left out. What was on the clipboard does not widen what a field accepts.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was pasted. |
| `accepts` | `Func<T, TResult>`&lt;`char`, `bool`&gt; | Whether a character may be typed here at all. |

**Returns** `string` — What is left of the first line.

