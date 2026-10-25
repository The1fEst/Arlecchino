---
title: "ICutsWords"
sidebar_label: "ICutsWords"
---

# ICutsWords interface

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Which part of a line is the word being finished. A line of shell reaches back to the last space, where a field holding one path is all one word.

```csharp
public interface ICutsWords
```

**Implemented by** [`SpaceWords`](../arlecchino.editing/SpaceWords.md), [`WholeLine`](../arlecchino.editing/WholeLine.md)

## Methods

| Member | Summary |
|---|---|
| [`Cut(string, int)`](#cut-string-int) | Cuts the word being typed out of the line. |

## Methods in detail

### `Cut(string, int)` {#cut-string-int}

```csharp
public CompletionAsk Cut(string text, int caret);
```

Cuts the word being typed out of the line.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The line as it stands. |
| `caret` | `int` | Where the caret is, which is where the word ends. |

**Returns** [`CompletionAsk`](../arlecchino.editing/CompletionAsk.md) — The word and the line it was cut from.

