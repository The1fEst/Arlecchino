---
title: "SpaceWords"
sidebar_label: "SpaceWords"
---

# SpaceWords class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Words told apart by the spaces between them, which is how a line of anything typed reads: the word being finished is what stands between the last space and the caret.

```csharp
public sealed class SpaceWords : ICutsWords
```

**Implements** [`ICutsWords`](../arlecchino.editing/ICutsWords.md)

## Constructors

| Member | Summary |
|---|---|
| [`SpaceWords()`](#spacewords) |  |

## Methods

| Member | Summary |
|---|---|
| [`Cut(string, int)`](#cut-string-int) |  |

## Constructors in detail

### `SpaceWords()` {#spacewords}

```csharp
public SpaceWords();
```

## Methods in detail

### `Cut(string, int)` {#cut-string-int}

```csharp
public CompletionAsk Cut(string text, int caret);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` |  |
| `caret` | `int` |  |

**Returns** [`CompletionAsk`](../arlecchino.editing/CompletionAsk.md)

