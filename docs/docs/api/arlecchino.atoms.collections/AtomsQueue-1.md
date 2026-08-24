---
title: "AtomsQueue<T>"
sidebar_label: "AtomsQueue<T>"
---

# AtomsQueue&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Collections` &middot; **Assembly:** `Arlecchino.Core`

A queue held as one piece of application state, joined at the back and left from the front. It is touched only by the drawing thread, so background work hands its item over with `FrameThread.Post`.

```csharp
public class AtomsQueue<T> : AtomsCollectionBase, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsCollectionBase`](../arlecchino.atoms.collections/AtomsCollectionBase.md)  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`AtomsQueue(IReadOnlyList<T>)`](#atomsqueue-ireadonlylist-t) | Creates the queue. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many are waiting. |
| [`IsEmpty`](#isempty) | Whether nothing is waiting. |
| [`Value`](#value) | What is waiting now, front first, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Drops everything waiting. An empty queue changes nothing. |
| [`Dequeue()`](#dequeue) | Takes the one at the front, and throws when nothing is waiting. |
| [`Enqueue(T)`](#enqueue-t) | Puts something at the back. |
| [`Enqueue(IReadOnlyList<T>)`](#enqueue-ireadonlylist-t) | Puts several at the back at once, in the order given. One notification, one frame and one undo step for the lot. |
| [`GetEnumerator()`](#getenumerator) | Walks what is waiting, front first, so `foreach` over the queue itself reads the way it does over a list. Reach for [`AtomsQueue.Value`](../arlecchino.atoms.collections/AtomsQueue-1.md#value) where a sequence is what is wanted, LINQ included. |
| [`InsertSilently(int, T[])`](#insertsilently-int-t) | Puts items in without reaching [`AtomsQueue.OnInserted`](../arlecchino.atoms.collections/AtomsQueue-1.md#oninserted-int-t). |
| [`OnInserted(int, T[])`](#oninserted-int-t) | Called once items joined, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved). |
| [`OnRemoved(int, T[])`](#onremoved-int-t) | Called once items left, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved). |
| [`OnReset(T[], T[])`](#onreset-t-t) | Called once the contents were swapped wholesale, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved). |
| [`Peek()`](#peek) | Reads the one at the front without taking it, and throws when nothing is waiting. |
| [`RemoveSilently(int, int)`](#removesilently-int-int) | Takes items out without reaching [`AtomsQueue.OnRemoved`](../arlecchino.atoms.collections/AtomsQueue-1.md#onremoved-int-t). |
| [`Reset(IReadOnlyList<T>)`](#reset-ireadonlylist-t) | Replaces what is waiting in one go, for a queue that is rebuilt rather than worked through — a plan worked out again, a batch reordered. Contents equal to what is already there change nothing. |
| [`ResetSilently(T[])`](#resetsilently-t) | Swaps the contents without reaching [`AtomsQueue.OnReset`](../arlecchino.atoms.collections/AtomsQueue-1.md#onreset-t-t). |
| [`TryDequeue(out T)`](#trydequeue-out-t) | Takes the one at the front without throwing when nothing is waiting. |
| [`TryPeek(out T)`](#trypeek-out-t) | Reads the one at the front without taking it or throwing. |

## Constructors in detail

### `AtomsQueue(IReadOnlyList<T>)` {#atomsqueue-ireadonlylist-t}

```csharp
public AtomsQueue(IReadOnlyList<T> initial);
```

Creates the queue.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What is already waiting, front first; empty when omitted. It is copied, not held. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many are waiting.

**Type** `int`

### `IsEmpty` {#isempty}

```csharp
public bool IsEmpty { get; }
```

Whether nothing is waiting.

**Type** `bool`

### `Value` {#value}

```csharp
public IReadOnlyList<T> Value { get; }
```

What is waiting now, front first, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below.

**Type** `IReadOnlyList<T>`&lt;`T`&gt;

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Drops everything waiting. An empty queue changes nothing.

### `Dequeue()` {#dequeue}

```csharp
public T Dequeue();
```

Takes the one at the front, and throws when nothing is waiting.

**Returns** `T` — What was at the front.

### `Enqueue(T)` {#enqueue-t}

```csharp
public void Enqueue(T item);
```

Puts something at the back.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to add. |

### `Enqueue(IReadOnlyList<T>)` {#enqueue-ireadonlylist-t}

```csharp
public void Enqueue(IReadOnlyList<T> items);
```

Puts several at the back at once, in the order given. One notification, one frame and one undo step for the lot.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What to add. Adding none changes nothing. |

### `GetEnumerator()` {#getenumerator}

```csharp
public List<T> GetEnumerator();
```

Walks what is waiting, front first, so `foreach` over the queue itself reads the way it does over a list. Reach for [`AtomsQueue.Value`](../arlecchino.atoms.collections/AtomsQueue-1.md#value) where a sequence is what is wanted, LINQ included.

**Returns** `Enumerator<T>`&lt;`T`&gt; — The enumerator, which throws when the queue changes while it is being walked.

### `InsertSilently(int, T[])` {#insertsilently-int-t}

```csharp
public void InsertSilently(int index, T[] items);
```

Puts items in without reaching [`AtomsQueue.OnInserted`](../arlecchino.atoms.collections/AtomsQueue-1.md#oninserted-int-t).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where they go, counting from the front. |
| `items` | `T`\[\] | What to put in, front first. |

### `OnInserted(int, T[])` {#oninserted-int-t}

```csharp
public virtual void OnInserted(int index, T[] items);
```

Called once items joined, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where they joined, counting from the front. |
| `items` | `T`\[\] | What joined, front first. |

### `OnRemoved(int, T[])` {#onremoved-int-t}

```csharp
public virtual void OnRemoved(int index, T[] items);
```

Called once items left, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where they were, counting from the front. |
| `items` | `T`\[\] | What left, front first. |

### `OnReset(T[], T[])` {#onreset-t-t}

```csharp
public virtual void OnReset(T[] previous, T[] current);
```

Called once the contents were swapped wholesale, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `T`\[\] | What was waiting, front first. |
| `current` | `T`\[\] | What is waiting now, front first. |

### `Peek()` {#peek}

```csharp
public T Peek();
```

Reads the one at the front without taking it, and throws when nothing is waiting.

**Returns** `T` — What is at the front.

### `RemoveSilently(int, int)` {#removesilently-int-int}

```csharp
public void RemoveSilently(int index, int count);
```

Takes items out without reaching [`AtomsQueue.OnRemoved`](../arlecchino.atoms.collections/AtomsQueue-1.md#onremoved-int-t).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where to start, counting from the front. |
| `count` | `int` | How many. |

### `Reset(IReadOnlyList<T>)` {#reset-ireadonlylist-t}

```csharp
public void Reset(IReadOnlyList<T> items);
```

Replaces what is waiting in one go, for a queue that is rebuilt rather than worked through — a plan worked out again, a batch reordered. Contents equal to what is already there change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What should be waiting instead, front first. |

### `ResetSilently(T[])` {#resetsilently-t}

```csharp
public void ResetSilently(T[] items);
```

Swaps the contents without reaching [`AtomsQueue.OnReset`](../arlecchino.atoms.collections/AtomsQueue-1.md#onreset-t-t).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `T`\[\] | What the queue should hold, front first. |

### `TryDequeue(out T)` {#trydequeue-out-t}

```csharp
public bool TryDequeue(out T item);
```

Takes the one at the front without throwing when nothing is waiting.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What was at the front, when there was one. |

**Returns** `bool` — `true` when something was taken.

### `TryPeek(out T)` {#trypeek-out-t}

```csharp
public bool TryPeek(out T item);
```

Reads the one at the front without taking it or throwing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What is at the front, when there is one. |

**Returns** `bool` — `true` when there was one.

