---
title: "HistoryAtomsMap<TKey, TValue>"
sidebar_label: "HistoryAtomsMap<TKey, TValue>"
---

# HistoryAtomsMap&lt;TKey, TValue&gt; class

**Namespace:** `Arlecchino.Atoms.History.Collections` &middot; **Assembly:** `Arlecchino.Atoms.History`

A map whose changes go on the undo stack. A plain [`AtomsMap`](../arlecchino.atoms.collections/AtomsMap-2.md) is the one whose changes do not.

```csharp
public sealed class HistoryAtomsMap<TKey, TValue> :
    AtomsMap<TKey, TValue>,
    IReadableAtom,
    IReadableAtom<IReadOnlyDictionary<TKey, TValue>>
```

**Inherits from** [`AtomsMap`](../arlecchino.atoms.collections/AtomsMap-2.md)&lt;`TKey`, `TValue`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md), [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`HistoryAtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)`](#historyatomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey) | Creates a map that keeps history. |

## Properties

| Member | Summary |
|---|---|
| [`IsObserved`](#isobserved) |  |

## Methods

| Member | Summary |
|---|---|
| [`OnAdded(TKey, TValue)`](#onadded-tkey-tvalue) |  |
| [`OnRemoved(TKey, TValue)`](#onremoved-tkey-tvalue) |  |
| [`OnReplaced(TKey, TValue, TValue)`](#onreplaced-tkey-tvalue-tvalue) |  |
| [`OnReset(KeyValuePair<TKey, TValue>[], KeyValuePair<TKey, TValue>[])`](#onreset-keyvaluepair-tkey-tvalue-keyvaluepair-tkey-tvalue) |  |

## Constructors in detail

### `HistoryAtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)` {#historyatomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey}

```csharp
public HistoryAtomsMap(IReadOnlyDictionary<TKey, TValue> initial, IEqualityComparer<TKey> comparer);
```

Creates a map that keeps history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt; | What it starts with; empty when omitted. It is copied, not held. |
| `comparer` | `IEqualityComparer<T>`&lt;`TKey`&gt; | How keys are compared. |

## Properties in detail

### `IsObserved` {#isobserved}

```csharp
public virtual bool IsObserved { get; }
```

**Type** `bool`

## Methods in detail

### `OnAdded(TKey, TValue)` {#onadded-tkey-tvalue}

```csharp
public virtual void OnAdded(TKey key, TValue value);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` |  |
| `value` | `TValue` |  |

### `OnRemoved(TKey, TValue)` {#onremoved-tkey-tvalue}

```csharp
public virtual void OnRemoved(TKey key, TValue value);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` |  |
| `value` | `TValue` |  |

### `OnReplaced(TKey, TValue, TValue)` {#onreplaced-tkey-tvalue-tvalue}

```csharp
public virtual void OnReplaced(TKey key, TValue previous, TValue current);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` |  |
| `previous` | `TValue` |  |
| `current` | `TValue` |  |

### `OnReset(KeyValuePair<TKey, TValue>[], KeyValuePair<TKey, TValue>[])` {#onreset-keyvaluepair-tkey-tvalue-keyvaluepair-tkey-tvalue}

```csharp
public virtual void OnReset(
    KeyValuePair<TKey, TValue>[] previous,
    KeyValuePair<TKey, TValue>[] current);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `KeyValuePair<TKey, TValue>`&lt;`TKey`, `TValue`&gt;\[\] |  |
| `current` | `KeyValuePair<TKey, TValue>`&lt;`TKey`, `TValue`&gt;\[\] |  |

