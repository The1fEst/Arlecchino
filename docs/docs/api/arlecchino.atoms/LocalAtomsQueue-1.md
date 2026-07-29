---
title: LocalAtomsQueue&lt;T&gt;
sidebar_label: LocalAtomsQueue&lt;T&gt;
---

# LocalAtomsQueue&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A queue the undo stack never sees: files still to copy, requests waiting for an answer, work a background task will pick up. It notifies and asks for a frame exactly as a [`TrackedAtomsQueue`](../arlecchino.atoms/TrackedAtomsQueue-1.md) does.

```csharp
public sealed class LocalAtomsQueue<T> : AtomsQueue<T>, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsQueue`](../arlecchino.atoms/AtomsQueue-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`LocalAtomsQueue(IReadOnlyList<T>)`](#localatomsqueue-ireadonlylist-t) | Creates a queue outside the undo history. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `LocalAtomsQueue(IReadOnlyList<T>)` {#localatomsqueue-ireadonlylist-t}

```csharp
public LocalAtomsQueue(IReadOnlyList<T> initial);
```

Creates a queue outside the undo history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What is already waiting, front first; empty when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public virtual bool RecordsHistory { get; }
```

**Type** `bool`

