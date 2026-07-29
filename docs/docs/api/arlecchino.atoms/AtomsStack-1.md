---
title: AtomsStack&lt;T&gt;
sidebar_label: AtomsStack&lt;T&gt;
---

# AtomsStack&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A stack held as one piece of application state — where the user has been, the modals over one another, a plan being unwound. Things go on top and come off the top, and every change goes through the same path a plain atom's write does: it is checked against the drawing thread, it notifies what reads the stack, it marks the frame stale, and it records an undo step when the stack is undoable. It is what a `ConcurrentStack<T>` is not for: nothing here is thread-safe, because nothing needs to be. Background work hands its item over with `FrameThread.Post` and the stack is only ever touched by the thread that draws it. [`AtomsStack.Value`](../arlecchino.atoms/AtomsStack-1.md#value) reads from the top down, the way `Stack<T>` itself enumerates, so `Value[0]` is what [`AtomsStack.Peek`](../arlecchino.atoms/AtomsStack-1.md#peek) answers. Whether changes can be undone is decided by the type created — [`TrackedAtomsStack`](../arlecchino.atoms/TrackedAtomsStack-1.md) or [`LocalAtomsStack`](../arlecchino.atoms/LocalAtomsStack-1.md).

```csharp
public abstract class AtomsStack<T> : IReadableAtom<IReadOnlyList<T>>
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`AtomsStack(IReadOnlyList<T>)`](#atomsstack-ireadonlylist-t) | Creates the stack. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many are on it. |
| [`IsEmpty`](#isempty) | Whether nothing is on it. |
| [`RecordsHistory`](#recordshistory) | Whether changes of this stack enter the undo history. |
| [`Value`](#value) | What is on the stack now, top first: a live view rather than a copy, so a widget handed this once draws whatever is on it on every later frame. It is read-only all the way down, so every change goes through the members below. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Takes everything off. An empty stack changes nothing. |
| [`GetEnumerator()`](#getenumerator) | Walks the stack from the top down, so `foreach` over it reads the way it does over a `Stack<T>`. Reach for [`AtomsStack.Value`](../arlecchino.atoms/AtomsStack-1.md#value) where a sequence is what is wanted, LINQ included. |
| [`Peek()`](#peek) | Reads the one on top without taking it, and throws when the stack is empty. |
| [`Pop()`](#pop) | Takes the one on top, and throws when the stack is empty. |
| [`Push(T)`](#push-t) | Puts something on top. |
| [`Push(IReadOnlyList<T>)`](#push-ireadonlylist-t) | Puts several on at once, the last of them ending up on top — what a loop of [`AtomsStack.Push`](../arlecchino.atoms/AtomsStack-1.md#push-t) would leave, in one notification and one undo step. |
| [`Reset(IReadOnlyList<T>)`](#reset-ireadonlylist-t) | Replaces what is on the stack in one go, for a stack that is rebuilt rather than unwound. Contents equal to what is already there change nothing. |
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever what is on the stack changes. |
| [`TryPeek(out T)`](#trypeek-out-t) | Reads the one on top without taking it or throwing. |
| [`TryPop(out T)`](#trypop-out-t) | Takes the one on top without throwing when the stack is empty. |

## Constructors in detail

### `AtomsStack(IReadOnlyList<T>)` {#atomsstack-ireadonlylist-t}

```csharp
public AtomsStack(IReadOnlyList<T> initial);
```

Creates the stack.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What is already on it, top first; empty when omitted. It is copied, not held. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many are on it.

**Type** `int`

### `IsEmpty` {#isempty}

```csharp
public bool IsEmpty { get; }
```

Whether nothing is on it.

**Type** `bool`

### `RecordsHistory` {#recordshistory}

```csharp
public abstract bool RecordsHistory { get; }
```

Whether changes of this stack enter the undo history.

**Type** `bool`

### `Value` {#value}

```csharp
public IReadOnlyList<T> Value { get; }
```

What is on the stack now, top first: a live view rather than a copy, so a widget handed this once draws whatever is on it on every later frame. It is read-only all the way down, so every change goes through the members below.

**Type** `IReadOnlyList<T>`&lt;`T`&gt;

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Takes everything off. An empty stack changes nothing.

### `GetEnumerator()` {#getenumerator}

```csharp
public List<T> GetEnumerator();
```

Walks the stack from the top down, so `foreach` over it reads the way it does over a `Stack<T>`. Reach for [`AtomsStack.Value`](../arlecchino.atoms/AtomsStack-1.md#value) where a sequence is what is wanted, LINQ included.

**Returns** `Enumerator<T>`&lt;`T`&gt; — The enumerator, which throws when the stack changes while it is being walked.

### `Peek()` {#peek}

```csharp
public T Peek();
```

Reads the one on top without taking it, and throws when the stack is empty.

**Returns** `T` — What is on top.

### `Pop()` {#pop}

```csharp
public T Pop();
```

Takes the one on top, and throws when the stack is empty.

**Returns** `T` — What was on top.

### `Push(T)` {#push-t}

```csharp
public void Push(T item);
```

Puts something on top.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What to put on. |

### `Push(IReadOnlyList<T>)` {#push-ireadonlylist-t}

```csharp
public void Push(IReadOnlyList<T> items);
```

Puts several on at once, the last of them ending up on top — what a loop of [`AtomsStack.Push`](../arlecchino.atoms/AtomsStack-1.md#push-t) would leave, in one notification and one undo step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What to put on. Putting none on changes nothing. |

### `Reset(IReadOnlyList<T>)` {#reset-ireadonlylist-t}

```csharp
public void Reset(IReadOnlyList<T> items);
```

Replaces what is on the stack in one go, for a stack that is rebuilt rather than unwound. Contents equal to what is already there change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What should be on it instead, top first. |

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Calls back whenever what is on the stack changes.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening.

### `TryPeek(out T)` {#trypeek-out-t}

```csharp
public bool TryPeek(out T item);
```

Reads the one on top without taking it or throwing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What is on top, when there is one. |

**Returns** `bool` — `true` when there was one.

### `TryPop(out T)` {#trypop-out-t}

```csharp
public bool TryPop(out T item);
```

Takes the one on top without throwing when the stack is empty.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | `T` | What was on top, when there was one. |

**Returns** `bool` — `true` when something was taken.

