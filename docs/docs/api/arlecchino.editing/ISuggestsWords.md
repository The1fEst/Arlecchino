---
title: "ISuggestsWords"
sidebar_label: "ISuggestsWords"
---

# ISuggestsWords interface

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Where the words a half-typed one could turn into come from. It is asked rather than read, since the answer can be a folder on the far side of a network.

```csharp
public interface ISuggestsWords
```

**Implemented by** [`WordList`](../arlecchino.editing/WordList.md)

## Methods

| Member | Summary |
|---|---|
| [`SuggestAsync(CompletionAsk, CancellationToken)`](#suggestasync-completionask-cancellationtoken) | What the word being typed could still turn into. |

## Methods in detail

### `SuggestAsync(CompletionAsk, CancellationToken)` {#suggestasync-completionask-cancellationtoken}

```csharp
public ValueTask<IReadOnlyList<string>> SuggestAsync(CompletionAsk ask, CancellationToken token);
```

What the word being typed could still turn into.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `ask` | [`CompletionAsk`](../arlecchino.editing/CompletionAsk.md) | The word and the line it stands in. |
| `token` | `CancellationToken` | Gives up the wait, as it is given up when the line is typed into again. |

**Returns** `ValueTask<TResult>`&lt;`IReadOnlyList<T>`&lt;`string`&gt;&gt; — The words, the likeliest first, or nothing when the word can go nowhere.

