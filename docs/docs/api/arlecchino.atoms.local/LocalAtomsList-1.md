---
title: "LocalAtomsList<T>"
sidebar_label: "LocalAtomsList<T>"
---

# LocalAtomsList&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Local` &middot; **Assembly:** `Arlecchino.Core`

A list the undo stack never sees, for contents the user did not author: a log, search results, the rows of a scan. It notifies and asks for a frame as a [`TrackedAtomsList`](../arlecchino.atoms.tracked/TrackedAtomsList-1.md) does.

```csharp
public sealed class LocalAtomsList<T> : AtomsList<T>, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`LocalAtomsList(IReadOnlyList<T>, IEqualityComparer<T>)`](#localatomslist-ireadonlylist-t-iequalitycomparer-t) | Creates a list outside the undo history. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `LocalAtomsList(IReadOnlyList<T>, IEqualityComparer<T>)` {#localatomslist-ireadonlylist-t-iequalitycomparer-t}

```csharp
public LocalAtomsList(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates a list outside the undo history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What it starts with; empty when omitted. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How items are compared; the default comparer when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public virtual bool RecordsHistory { get; }
```

**Type** `bool`

