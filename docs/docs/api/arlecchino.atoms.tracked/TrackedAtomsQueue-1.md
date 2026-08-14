---
title: "TrackedAtomsQueue<T>"
sidebar_label: "TrackedAtomsQueue<T>"
---

# TrackedAtomsQueue&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Tracked` &middot; **Assembly:** `Arlecchino.Core`

A queue whose changes go on the undo stack, picked up by [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) with nothing to register. Each call is one step, including one that puts several in at once.

```csharp
public sealed class TrackedAtomsQueue<T> : AtomsQueue<T>, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsQueue`](../arlecchino.atoms.collections/AtomsQueue-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`TrackedAtomsQueue(IReadOnlyList<T>)`](#trackedatomsqueue-ireadonlylist-t) | Creates an undoable queue. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `TrackedAtomsQueue(IReadOnlyList<T>)` {#trackedatomsqueue-ireadonlylist-t}

```csharp
public TrackedAtomsQueue(IReadOnlyList<T> initial);
```

Creates an undoable queue.

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

