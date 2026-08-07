---
title: "LocalAtom<T>"
sidebar_label: "LocalAtom<T>"
---

# LocalAtom&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Local` &middot; **Assembly:** `Arlecchino.Core`

An atom the undo stack never sees: a filter, a cursor, a load in progress, a selection — state the user did not author and would not expect to travel back through. It notifies and repaints exactly as a [`TrackedAtom`](../arlecchino.atoms.tracked/TrackedAtom-1.md) does.

```csharp
public sealed class LocalAtom<T> : Atom<T>, IReadableAtom<T>
```

**Inherits from** [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`T`&gt;

## Constructors

| Member | Summary |
|---|---|
| [`LocalAtom(T, IEqualityComparer<T>)`](#localatom-t-iequalitycomparer-t) | Creates an atom holding a starting value, outside the undo history. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `LocalAtom(T, IEqualityComparer<T>)` {#localatom-t-iequalitycomparer-t}

```csharp
public LocalAtom(T initial, IEqualityComparer<T> comparer);
```

Creates an atom holding a starting value, outside the undo history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `T` | The value to start with. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How to decide that a write changed nothing; the default comparer for `T` is used when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public virtual bool RecordsHistory { get; }
```

**Type** `bool`

