---
title: "AtomsSet<T>"
sidebar_label: "AtomsSet<T>"
---

# AtomsSet&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Collections` &middot; **Assembly:** `Arlecchino.Core`

A set held as one piece of application state, behaving as a `HashSet<T>` does: adding what is already there changes nothing. A walk of it is in no order, so sort it where the reader sees the order.

```csharp
public abstract class AtomsSet<T> : IReadableAtom<IReadOnlySet<T>>
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlySet<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`AtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)`](#atomsset-ireadonlylist-t-iequalitycomparer-t) | Creates the set. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many it holds. |
| [`IsEmpty`](#isempty) | Whether it holds nothing. |
| [`RecordsHistory`](#recordshistory) | Whether changes of this set enter the undo history. |
| [`Value`](#value) | What the set holds now, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below. |

## Methods

| Member | Summary |
|---|---|
| [`Add(T)`](#add-t) | Puts something in. Putting in what is already there changes nothing. |
| [`Add(IReadOnlyList<T>)`](#add-ireadonlylist-t) | Puts several in at once. One notification, one frame and one undo step for however many of them were not there already. |
| [`Clear()`](#clear) | Takes everything out. An empty set changes nothing. |
| [`Contains(T)`](#contains-t) | Whether the set holds something. |
| [`GetEnumerator()`](#getenumerator) | Walks what the set holds, in no order it promises, so `foreach` over the set itself reads the way it does over a `HashSet<T>`. Reach for [`AtomsSet.Value`](../arlecchino.atoms.collections/AtomsSet-1.md#value) where a sequence is what is wanted, LINQ included. |
| [`Remove(T)`](#remove-t) | Takes something out, and does nothing when it is not there. |
| [`Reset(IReadOnlyList<T>)`](#reset-ireadonlylist-t) | Replaces the contents in one go, for the set that is worked out again rather than edited — what a new listing marks, what a filter leaves. Contents equal to what is already there change nothing. |
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the contents change. |
| [`TryAdd(T)`](#tryadd-t) | Puts something in and says whether it was new. |
| [`TryRemove(T)`](#tryremove-t) | Takes something out and says whether it was there. |

## Constructors in detail

### `AtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)` {#atomsset-ireadonlylist-t-iequalitycomparer-t}

```csharp
public AtomsSet(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates the set.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What it starts with; empty when omitted. It is copied, not held. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How items are compared; the default comparer for `T` is used when omitted. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many it holds.

**Type** `int`

### `IsEmpty` {#isempty}

```csharp
public bool IsEmpty { get; }
```

Whether it holds nothing.

**Type** `bool`

### `RecordsHistory` {#recordshistory}

```csharp
public abstract bool RecordsHistory { get; }
```

Whether changes of this set enter the undo history.

**Type** `bool`

### `Value` {#value}

```csharp
public IReadOnlySet<T> Value { get; }
```

What the set holds now, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below.

**Type** `IReadOnlySet<T>`&lt;`T`&gt;

## Methods in detail

### `Add(T)` {#add-t}

```csharp
public void Add(T item);
```

Puts something in. Putting in what is already there changes nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to put in. |

### `Add(IReadOnlyList<T>)` {#add-ireadonlylist-t}

```csharp
public void Add(IReadOnlyList<T> items);
```

Puts several in at once. One notification, one frame and one undo step for however many of them were not there already.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What to put in. Adding none, or only what is there, changes nothing. |

### `Clear()` {#clear}

```csharp
public void Clear();
```

Takes everything out. An empty set changes nothing.

### `Contains(T)` {#contains-t}

```csharp
public bool Contains(T item);
```

Whether the set holds something.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to look for. |

**Returns** `bool` — `true` when it is there.

### `GetEnumerator()` {#getenumerator}

```csharp
public HashSet<T> GetEnumerator();
```

Walks what the set holds, in no order it promises, so `foreach` over the set itself reads the way it does over a `HashSet<T>`. Reach for [`AtomsSet.Value`](../arlecchino.atoms.collections/AtomsSet-1.md#value) where a sequence is what is wanted, LINQ included.

**Returns** `Enumerator<T>`&lt;`T`&gt; — The enumerator, which throws when the set changes while it is being walked.

### `Remove(T)` {#remove-t}

```csharp
public void Remove(T item);
```

Takes something out, and does nothing when it is not there.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to take out. |

### `Reset(IReadOnlyList<T>)` {#reset-ireadonlylist-t}

```csharp
public void Reset(IReadOnlyList<T> items);
```

Replaces the contents in one go, for the set that is worked out again rather than edited — what a new listing marks, what a filter leaves. Contents equal to what is already there change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What the set should hold instead. |

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

### `TryAdd(T)` {#tryadd-t}

```csharp
public bool TryAdd(T item);
```

Puts something in and says whether it was new.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to put in. |

**Returns** `bool` — `true` when it went in, `false` when it was already there.

### `TryRemove(T)` {#tryremove-t}

```csharp
public bool TryRemove(T item);
```

Takes something out and says whether it was there.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to take out. |

**Returns** `bool` — `true` when something was taken out.

