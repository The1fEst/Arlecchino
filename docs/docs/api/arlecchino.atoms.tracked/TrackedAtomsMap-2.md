---
title: "TrackedAtomsMap<TKey, TValue>"
sidebar_label: "TrackedAtomsMap<TKey, TValue>"
---

# TrackedAtomsMap&lt;TKey, TValue&gt; class

**Namespace:** `Arlecchino.Atoms.Tracked` &middot; **Assembly:** `Arlecchino.Core`

A map whose changes go on the undo stack: what the user set per profile, the notes kept against each entry, the overrides of a configuration. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step.

```csharp
public sealed class TrackedAtomsMap<TKey, TValue> :
    AtomsMap<TKey, TValue>,
    IReadableAtom<IReadOnlyDictionary<TKey, TValue>>
```

**Inherits from** [`AtomsMap`](../arlecchino.atoms.collections/AtomsMap-2.md)&lt;`TKey`, `TValue`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`TrackedAtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)`](#trackedatomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey) | Creates an undoable map. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `TrackedAtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)` {#trackedatomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey}

```csharp
public TrackedAtomsMap(IReadOnlyDictionary<TKey, TValue> initial, IEqualityComparer<TKey> comparer);
```

Creates an undoable map.

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

