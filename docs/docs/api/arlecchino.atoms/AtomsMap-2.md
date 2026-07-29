---
title: AtomsMap&lt;TKey, TValue&gt;
sidebar_label: AtomsMap&lt;TKey, TValue&gt;
---

# AtomsMap&lt;TKey, TValue&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A map held as one piece of application state — what is known about each server, the settings read so far, a count per kind. Every change goes through the same path a plain atom's write does: it is checked against the drawing thread, it notifies what reads the map, it marks the frame stale, and it records an undo step when the map is undoable. It is to `Atom<Dictionary<TKey, TValue>>` what [`AtomsList`](../arlecchino.atoms/AtomsList-1.md) is to an atom around a list, and for the same reason: writing into the dictionary inside an atom never reaches `Atom.Value`, so nothing is notified and no frame is asked for, and writing the same instance back is taken for a change of nothing because a dictionary is compared by reference. It holds a dictionary but is not one: there is no `IDictionary` to write through, because the members below are the only way in and that is what keeps every change on the frame's path. Whether changes can be undone is decided by the type created — [`TrackedAtomsMap`](../arlecchino.atoms/TrackedAtomsMap-2.md) or [`LocalAtomsMap`](../arlecchino.atoms/LocalAtomsMap-2.md) — exactly as it is for atoms and lists.

```csharp
public abstract class AtomsMap<TKey, TValue> : IReadableAtom<IReadOnlyDictionary<TKey, TValue>>
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`AtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)`](#atomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey) | Creates the map. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many entries there are. |
| [`Item`](#item) | The value kept against a key. Reading a key the map does not hold throws, as a dictionary does; writing puts the entry there whether or not it was there before, and writing an equal value changes nothing and notifies nobody. |
| [`RecordsHistory`](#recordshistory) | Whether changes of this map enter the undo history. |
| [`Value`](#value) | What the map holds now: a live view rather than a copy, so something handed this once reads whatever is in it on every later frame. It is read-only all the way down, so every change goes through the members below and is seen by the frame and by the history. |

## Methods

| Member | Summary |
|---|---|
| [`Add(TKey, TValue)`](#add-tkey-tvalue) | Puts an entry in, and throws when the key is already there — the dictionary's own rule, for the cases where a second entry under one key means something has gone wrong. Use the indexer to put one in whether or not it is there already. |
| [`Clear()`](#clear) | Takes everything out. An empty map changes nothing. |
| [`ContainsKey(TKey)`](#containskey-tkey) | Whether the map holds an entry under a key. |
| [`GetEnumerator()`](#getenumerator) | Walks the entries, so `foreach` over the map itself reads the way it does over a dictionary. It is not an `IEnumerable` — the enumerator is all a `foreach` asks for, and stopping there is what keeps the members above the only way to change anything. Reach for [`AtomsMap.Value`](../arlecchino.atoms/AtomsMap-2.md#value) where a sequence is what is wanted, LINQ included. |
| [`Remove(TKey)`](#remove-tkey) | Takes an entry out, and does nothing when the key is not there. |
| [`Reset(IReadOnlyDictionary<TKey, TValue>)`](#reset-ireadonlydictionary-tkey-tvalue) | Replaces the contents in one go, for the map that is reloaded rather than edited — settings read again, a listing answered afresh. Contents equal to what is already there change nothing. |
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the contents change. |
| [`TryAdd(TKey, TValue)`](#tryadd-tkey-tvalue) | Puts an entry in unless the key is taken, and says which happened — [`AtomsMap.Add`](../arlecchino.atoms/AtomsMap-2.md#add-tkey-tvalue) without the exception, for the case where losing the race with an earlier entry is an answer rather than a fault. |
| [`TryGetValue(TKey, out TValue)`](#trygetvalue-tkey-out-tvalue) | Reads an entry without throwing when it is not there. |
| [`TryRemove(TKey, out TValue)`](#tryremove-tkey-out-tvalue) | Takes an entry out and hands back what was kept under it, which is the reading and the removal in one step rather than a lookup followed by a hope. |

## Constructors in detail

### `AtomsMap(IReadOnlyDictionary<TKey, TValue>, IEqualityComparer<TKey>)` {#atomsmap-ireadonlydictionary-tkey-tvalue-iequalitycomparer-tkey}

```csharp
public AtomsMap(IReadOnlyDictionary<TKey, TValue> initial, IEqualityComparer<TKey> comparer);
```

Creates the map.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt; | What it starts with; empty when omitted. It is copied, not held. |
| `comparer` | `IEqualityComparer<T>`&lt;`TKey`&gt; | How keys are compared; the default comparer for `TKey` is used when omitted. Values are compared with their own default comparer, which is what decides that a write changed nothing. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many entries there are.

**Type** `int`

### `Item` {#item}

```csharp
public TValue this[TKey key] { get; set; }
```

The value kept against a key. Reading a key the map does not hold throws, as a dictionary does; writing puts the entry there whether or not it was there before, and writing an equal value changes nothing and notifies nobody.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` | Which entry. |

**Type** `TValue`

### `RecordsHistory` {#recordshistory}

```csharp
public abstract bool RecordsHistory { get; }
```

Whether changes of this map enter the undo history.

**Type** `bool`

### `Value` {#value}

```csharp
public IReadOnlyDictionary<TKey, TValue> Value { get; }
```

What the map holds now: a live view rather than a copy, so something handed this once reads whatever is in it on every later frame. It is read-only all the way down, so every change goes through the members below and is seen by the frame and by the history.

**Type** `IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt;

## Methods in detail

### `Add(TKey, TValue)` {#add-tkey-tvalue}

```csharp
public void Add(TKey key, TValue value);
```

Puts an entry in, and throws when the key is already there — the dictionary's own rule, for the cases where a second entry under one key means something has gone wrong. Use the indexer to put one in whether or not it is there already.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` | What to keep it under. |
| `value` | `TValue` | What to keep. |

### `Clear()` {#clear}

```csharp
public void Clear();
```

Takes everything out. An empty map changes nothing.

### `ContainsKey(TKey)` {#containskey-tkey}

```csharp
public bool ContainsKey(TKey key);
```

Whether the map holds an entry under a key.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` | What to look for. |

**Returns** `bool` — `true` when it is there.

### `GetEnumerator()` {#getenumerator}

```csharp
public Dictionary<TKey, TValue> GetEnumerator();
```

Walks the entries, so `foreach` over the map itself reads the way it does over a dictionary. It is not an `IEnumerable` — the enumerator is all a `foreach` asks for, and stopping there is what keeps the members above the only way to change anything. Reach for [`AtomsMap.Value`](../arlecchino.atoms/AtomsMap-2.md#value) where a sequence is what is wanted, LINQ included.

**Returns** `Enumerator<TKey, TValue>`&lt;`TKey`, `TValue`&gt; — The enumerator, which throws when the map changes while it is being walked.

### `Remove(TKey)` {#remove-tkey}

```csharp
public void Remove(TKey key);
```

Takes an entry out, and does nothing when the key is not there.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` | Which entry. |

### `Reset(IReadOnlyDictionary<TKey, TValue>)` {#reset-ireadonlydictionary-tkey-tvalue}

```csharp
public void Reset(IReadOnlyDictionary<TKey, TValue> items);
```

Replaces the contents in one go, for the map that is reloaded rather than edited — settings read again, a listing answered afresh. Contents equal to what is already there change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyDictionary<TKey, TValue>`&lt;`TKey`, `TValue`&gt; | What the map should hold instead. |

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Calls back whenever the contents change.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening.

### `TryAdd(TKey, TValue)` {#tryadd-tkey-tvalue}

```csharp
public bool TryAdd(TKey key, TValue value);
```

Puts an entry in unless the key is taken, and says which happened — [`AtomsMap.Add`](../arlecchino.atoms/AtomsMap-2.md#add-tkey-tvalue) without the exception, for the case where losing the race with an earlier entry is an answer rather than a fault.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` | What to keep it under. |
| `value` | `TValue` | What to keep. |

**Returns** `bool` — `true` when the entry went in, `false` when the key was already there.

### `TryGetValue(TKey, out TValue)` {#trygetvalue-tkey-out-tvalue}

```csharp
public bool TryGetValue(TKey key, out TValue value);
```

Reads an entry without throwing when it is not there.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` | What to look for. |
| `value` | `TValue` | What was kept under it, when it was there. |

**Returns** `bool` — `true` when it was there.

### `TryRemove(TKey, out TValue)` {#tryremove-tkey-out-tvalue}

```csharp
public bool TryRemove(TKey key, out TValue value);
```

Takes an entry out and hands back what was kept under it, which is the reading and the removal in one step rather than a lookup followed by a hope.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `TKey` | Which entry. |
| `value` | `TValue` | What was kept under it, when it was there. |

**Returns** `bool` — `true` when something was taken out.

