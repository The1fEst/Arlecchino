---
title: "AtomsQueue<T>"
sidebar_label: "AtomsQueue<T>"
---

# AtomsQueue&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Collections` &middot; **Assembly:** `Arlecchino.Core`

A queue held as one piece of application state — work waiting to be done, files still to copy, commands typed ahead. Things join at the back and leave from the front, and every change goes through the same path a plain atom's write does: it is checked against the drawing thread, it notifies what reads the queue, it marks the frame stale, and it records an undo step when the queue is undoable. It is what a `ConcurrentQueue<T>` is not for: nothing here is thread-safe, because nothing needs to be. Background work hands its item over with `FrameThread.Post` and the queue is only ever touched by the thread that draws it, which is what lets a frame read it several times and see the same thing each time. [`AtomsQueue.Value`](../arlecchino.atoms.collections/AtomsQueue-1.md#value) is the contents in order, front first, so a view draws the queue by walking it. Whether changes can be undone is decided by the type created — [`TrackedAtomsQueue`](../arlecchino.atoms.tracked/TrackedAtomsQueue-1.md) or [`LocalAtomsQueue`](../arlecchino.atoms.local/LocalAtomsQueue-1.md).

```csharp
public abstract class AtomsQueue<T> : IReadableAtom<IReadOnlyList<T>>
```

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
| [`RecordsHistory`](#recordshistory) | Whether changes of this queue enter the undo history. |
| [`Value`](#value) | What is waiting now, front first: a live view rather than a copy, so a widget handed this once draws whatever is in the queue on every later frame. It is read-only all the way down, so every change goes through the members below. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Drops everything waiting. An empty queue changes nothing. |
| [`Dequeue()`](#dequeue) | Takes the one at the front, and throws when nothing is waiting. |
| [`Enqueue(T)`](#enqueue-t) | Puts something at the back. |
| [`Enqueue(IReadOnlyList<T>)`](#enqueue-ireadonlylist-t) | Puts several at the back at once, in the order given. One notification, one frame and one undo step for the lot. |
| [`GetEnumerator()`](#getenumerator) | Walks what is waiting, front first, so `foreach` over the queue itself reads the way it does over a list. Reach for [`AtomsQueue.Value`](../arlecchino.atoms.collections/AtomsQueue-1.md#value) where a sequence is what is wanted, LINQ included. |
| [`Peek()`](#peek) | Reads the one at the front without taking it, and throws when nothing is waiting. |
| [`Reset(IReadOnlyList<T>)`](#reset-ireadonlylist-t) | Replaces what is waiting in one go, for a queue that is rebuilt rather than worked through — a plan worked out again, a batch reordered. Contents equal to what is already there change nothing. |
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever what is waiting changes. |
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

### `RecordsHistory` {#recordshistory}

```csharp
public abstract bool RecordsHistory { get; }
```

Whether changes of this queue enter the undo history.

**Type** `bool`

### `Value` {#value}

```csharp
public IReadOnlyList<T> Value { get; }
```

What is waiting now, front first: a live view rather than a copy, so a widget handed this once draws whatever is in the queue on every later frame. It is read-only all the way down, so every change goes through the members below.

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

### `Peek()` {#peek}

```csharp
public T Peek();
```

Reads the one at the front without taking it, and throws when nothing is waiting.

**Returns** `T` — What is at the front.

### `Reset(IReadOnlyList<T>)` {#reset-ireadonlylist-t}

```csharp
public void Reset(IReadOnlyList<T> items);
```

Replaces what is waiting in one go, for a queue that is rebuilt rather than worked through — a plan worked out again, a batch reordered. Contents equal to what is already there change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What should be waiting instead, front first. |

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Calls back whenever what is waiting changes.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening.

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

