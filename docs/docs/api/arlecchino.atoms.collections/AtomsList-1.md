---
title: "AtomsList<T>"
sidebar_label: "AtomsList<T>"
---

# AtomsList&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Collections` &middot; **Assembly:** `Arlecchino.Core`

A list held as one piece of application state, changed in place. Every change notifies what reads the list, marks the frame stale and records an undo step.

```csharp
public abstract class AtomsList<T> : IReadableAtom<IReadOnlyList<T>>
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`AtomsList(IReadOnlyList<T>, IEqualityComparer<T>)`](#atomslist-ireadonlylist-t-iequalitycomparer-t) | Creates the list. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many items there are. |
| [`Item`](#item) | The item at a position. Writing an equal item changes nothing and notifies no one. |
| [`RecordsHistory`](#recordshistory) | Whether changes of this list enter the undo history. |
| [`Value`](#value) | What the list holds now, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below. |

## Methods

| Member | Summary |
|---|---|
| [`Add(T)`](#add-t) | Puts an item at the end. |
| [`Add(IReadOnlyList<T>)`](#add-ireadonlylist-t) | Puts several items at the end at once. One notification, one frame and one undo step for the lot, which is what a loop of [`AtomsList.Add`](../arlecchino.atoms.collections/AtomsList-1.md#add-t) cannot give — that would undo a page of rows one row at a time. |
| [`Clear()`](#clear) | Takes everything out. An empty list changes nothing. |
| [`GetEnumerator()`](#getenumerator) | Walks what the list holds, so `foreach` over the list itself reads the way it does over a list. Reach for [`AtomsList.Value`](../arlecchino.atoms.collections/AtomsList-1.md#value) where a sequence is wanted, LINQ included. |
| [`IndexOf(T)`](#indexof-t) | Where an item is, or `-1` when the list does not hold it. |
| [`Insert(int, T)`](#insert-int-t) | Puts an item at a position, moving the rest along. |
| [`Remove(T)`](#remove-t) | Takes out the first item equal to this one, and does nothing when there is none. |
| [`RemoveAt(int)`](#removeat-int) | Takes out the item at a position. |
| [`RemoveRange(int, int)`](#removerange-int-int) | Takes out several items in a row at once, with one notification, one frame and one undo step for the lot. Trimming a list one item at a time would come back the same way. |
| [`Reset(IReadOnlyList<T>)`](#reset-ireadonlylist-t) | Replaces the contents in one go, for the case the list is not edited but reloaded — a query answered, a folder read again, a filter applied. Contents equal to what is already there change nothing. |
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the contents change. |
| [`Touch()`](#touch) | Says that an item already in the list changed inside itself, so everything watching the list hears about it. Replace the item instead, unless its identity has to survive the change. |

## Constructors in detail

### `AtomsList(IReadOnlyList<T>, IEqualityComparer<T>)` {#atomslist-ireadonlylist-t-iequalitycomparer-t}

```csharp
public AtomsList(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates the list.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What it starts with; empty when omitted. It is copied, not held. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How [`AtomsList.Remove`](../arlecchino.atoms.collections/AtomsList-1.md#remove-t) finds an item, and how writing to the indexer decides it changed nothing; the default comparer for `T` is used when omitted. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many items there are.

**Type** `int`

### `Item` {#item}

```csharp
public T this[int index] { get; set; }
```

The item at a position. Writing an equal item changes nothing and notifies no one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Which one. |

**Type** `T`

### `RecordsHistory` {#recordshistory}

```csharp
public abstract bool RecordsHistory { get; }
```

Whether changes of this list enter the undo history.

**Type** `bool`

### `Value` {#value}

```csharp
public IReadOnlyList<T> Value { get; }
```

What the list holds now, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below.

**Type** `IReadOnlyList<T>`&lt;`T`&gt;

## Methods in detail

### `Add(T)` {#add-t}

```csharp
public void Add(T item);
```

Puts an item at the end.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to add. |

### `Add(IReadOnlyList<T>)` {#add-ireadonlylist-t}

```csharp
public void Add(IReadOnlyList<T> items);
```

Puts several items at the end at once. One notification, one frame and one undo step for the lot, which is what a loop of [`AtomsList.Add`](../arlecchino.atoms.collections/AtomsList-1.md#add-t) cannot give — that would undo a page of rows one row at a time.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What to add. Adding none changes nothing. |

### `Clear()` {#clear}

```csharp
public void Clear();
```

Takes everything out. An empty list changes nothing.

### `GetEnumerator()` {#getenumerator}

```csharp
public List<T> GetEnumerator();
```

Walks what the list holds, so `foreach` over the list itself reads the way it does over a list. Reach for [`AtomsList.Value`](../arlecchino.atoms.collections/AtomsList-1.md#value) where a sequence is wanted, LINQ included.

**Returns** `Enumerator<T>`&lt;`T`&gt; — The enumerator, which throws when the list changes while it is being walked.

### `IndexOf(T)` {#indexof-t}

```csharp
public int IndexOf(T item);
```

Where an item is, or `-1` when the list does not hold it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to look for. |

**Returns** `int` — The position of the first item equal to it.

### `Insert(int, T)` {#insert-int-t}

```csharp
public void Insert(int index, T item);
```

Puts an item at a position, moving the rest along.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where it goes. |
| `item` | `T` | What to insert. |

### `Remove(T)` {#remove-t}

```csharp
public void Remove(T item);
```

Takes out the first item equal to this one, and does nothing when there is none.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to take out. |

### `RemoveAt(int)` {#removeat-int}

```csharp
public void RemoveAt(int index);
```

Takes out the item at a position.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Which one. |

### `RemoveRange(int, int)` {#removerange-int-int}

```csharp
public void RemoveRange(int index, int count);
```

Takes out several items in a row at once, with one notification, one frame and one undo step for the lot. Trimming a list one item at a time would come back the same way.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where to start. |
| `count` | `int` | How many to take out. Taking none changes nothing. |

### `Reset(IReadOnlyList<T>)` {#reset-ireadonlylist-t}

```csharp
public void Reset(IReadOnlyList<T> items);
```

Replaces the contents in one go, for the case the list is not edited but reloaded — a query answered, a folder read again, a filter applied. Contents equal to what is already there change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What the list should hold instead. |

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

### `Touch()` {#touch}

```csharp
public void Touch();
```

Says that an item already in the list changed inside itself, so everything watching the list hears about it. Replace the item instead, unless its identity has to survive the change.

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

