---
title: LocalAtomsMap&lt;TKey, TValue&gt;
sidebar_label: LocalAtomsMap&lt;TKey, TValue&gt;
---

# LocalAtomsMap&lt;TKey, TValue&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A map the undo stack never sees: what a scan found per folder, the sizes worked out so far, the state of each connection. It notifies and asks for a frame exactly as a [`TrackedAtomsMap`](../arlecchino.atoms/TrackedAtomsMap-2.md) does.

```csharp
public sealed class LocalAtomsMap<TKey, TValue> :
    AtomsMap<TKey, TValue>,
    IReadableAtom<IReadOnlyDictionary<TKey, TValue>>
```

**Inherits from** [`AtomsMap`](../arlecchino.atoms/AtomsMap-2.md)&lt;`TKey`, `TValue`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`LocalAtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)`](#localatomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey) | Creates a map outside the undo history. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `LocalAtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)` {#localatomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey}

```csharp
public LocalAtomsMap(IReadOnlyDictionary<TKey, TValue> initial, IEqualityComparer<TKey> comparer);
```

Creates a map outside the undo history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt; | What it starts with; empty when omitted. |
| `comparer` | `IEqualityComparer<T>`&lt;`TKey`&gt; | How keys are compared; the default comparer when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public virtual bool RecordsHistory { get; }
```

**Type** `bool`

