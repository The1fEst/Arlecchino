---
title: "WholeLine"
sidebar_label: "WholeLine"
---

# WholeLine class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Everything up to the caret as one word, for a field that holds one thing: a path, a host, a name. Nothing in it divides it, spaces included, since a name is allowed to have them.

```csharp
public sealed class WholeLine : ICutsWords
```

**Implements** [`ICutsWords`](../arlecchino.editing/ICutsWords.md)

## Constructors

| Member | Summary |
|---|---|
| [`WholeLine()`](#wholeline) |  |

## Methods

| Member | Summary |
|---|---|
| [`Cut(string, int)`](#cut-string-int) |  |

## Constructors in detail

### `WholeLine()` {#wholeline}

```csharp
public WholeLine();
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

