---
title: "WordList"
sidebar_label: "WordList"
---

# WordList class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Words an application already holds: the names of its commands, the hosts it knows. They are read through a delegate, so the list is whatever it is when the word is being finished.

```csharp
public sealed class WordList : ISuggestsWords
```

**Implements** [`ISuggestsWords`](../arlecchino.editing/ISuggestsWords.md)

## Constructors

| Member | Summary |
|---|---|
| [`WordList(Func<IReadOnlyList<string>>)`](#wordlist-func-ireadonlylist-string) | Offers what a delegate lists. |

## Methods

| Member | Summary |
|---|---|
| [`SuggestAsync(CompletionAsk, CancellationToken)`](#suggestasync-completionask-cancellationtoken) | The words that begin with what has been typed, in the order they were listed. Case is forgiven, since a name is looked for rather than checked. |

## Constructors in detail

### `WordList(Func<IReadOnlyList<string>>)` {#wordlist-func-ireadonlylist-string}

```csharp
public WordList(Func<IReadOnlyList<string>> words);
```

Offers what a delegate lists.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `words` | `Func<TResult>`&lt;`IReadOnlyList<T>`&lt;`string`&gt;&gt; | Everything that could be typed, whether it fits what has been or not. |

## Methods in detail

### `SuggestAsync(CompletionAsk, CancellationToken)` {#suggestasync-completionask-cancellationtoken}

```csharp
public ValueTask<IReadOnlyList<string>> SuggestAsync(CompletionAsk ask, CancellationToken token);
```

The words that begin with what has been typed, in the order they were listed. Case is forgiven, since a name is looked for rather than checked.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `ask` | [`CompletionAsk`](../arlecchino.editing/CompletionAsk.md) | The word and the line it stands in. |
| `token` | `CancellationToken` | Not waited on: the list is already here. |

**Returns** `ValueTask<TResult>`&lt;`IReadOnlyList<T>`&lt;`string`&gt;&gt; — What fits.

