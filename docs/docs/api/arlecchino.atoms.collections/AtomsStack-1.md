---
title: "AtomsStack<T>"
sidebar_label: "AtomsStack<T>"
---

# AtomsStack&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Collections` &middot; **Assembly:** `Arlecchino.Core`

A stack held as one piece of application state, put on and taken off the top. [`AtomsStack.Value`](../arlecchino.atoms.collections/AtomsStack-1.md#value) reads from the top down, so `Value[0]` is what [`AtomsStack.Peek`](../arlecchino.atoms.collections/AtomsStack-1.md#peek) answers.

```csharp
public class AtomsStack<T> : AtomsCollectionBase, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsCollectionBase`](../arlecchino.atoms.collections/AtomsCollectionBase.md)  
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
| [`Value`](#value) | What is on the stack now, top first, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Takes everything off. An empty stack changes nothing. |
| [`GetEnumerator()`](#getenumerator) | Walks the stack from the top down, so `foreach` over it reads the way it does over a `Stack<T>`. Reach for [`AtomsStack.Value`](../arlecchino.atoms.collections/AtomsStack-1.md#value) where a sequence is what is wanted, LINQ included. |
| [`InsertSilently(int, T[])`](#insertsilently-int-t) | Puts items on without reaching [`AtomsStack.OnInserted`](../arlecchino.atoms.collections/AtomsStack-1.md#oninserted-int-t). |
| [`OnInserted(int, T[])`](#oninserted-int-t) | Called once items went on, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved). |
| [`OnRemoved(int, T[])`](#onremoved-int-t) | Called once items came off, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved). |
| [`OnReset(T[], T[])`](#onreset-t-t) | Called once the contents were swapped wholesale, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved). |
| [`Peek()`](#peek) | Reads the one on top without taking it, and throws when the stack is empty. |
| [`Pop()`](#pop) | Takes the one on top, and throws when the stack is empty. |
| [`Push(T)`](#push-t) | Puts something on top. |
| [`Push(IReadOnlyList<T>)`](#push-ireadonlylist-t) | Puts several on at once, the last of them ending up on top — what a loop of [`AtomsStack.Push`](../arlecchino.atoms.collections/AtomsStack-1.md#push-t) would leave, in one notification and one undo step. |
| [`RemoveSilently(int, int)`](#removesilently-int-int) | Takes items off without reaching [`AtomsStack.OnRemoved`](../arlecchino.atoms.collections/AtomsStack-1.md#onremoved-int-t). |
| [`Reset(IReadOnlyList<T>)`](#reset-ireadonlylist-t) | Replaces what is on the stack in one go, for a stack that is rebuilt rather than unwound. Contents equal to what is already there change nothing. |
| [`ResetSilently(T[])`](#resetsilently-t) | Swaps the contents without reaching [`AtomsStack.OnReset`](../arlecchino.atoms.collections/AtomsStack-1.md#onreset-t-t). |
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

### `Value` {#value}

```csharp
public IReadOnlyList<T> Value { get; }
```

What is on the stack now, top first, as a live view rather than a copy. It is read-only all the way down, so every change goes through the members below.

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

Walks the stack from the top down, so `foreach` over it reads the way it does over a `Stack<T>`. Reach for [`AtomsStack.Value`](../arlecchino.atoms.collections/AtomsStack-1.md#value) where a sequence is what is wanted, LINQ included.

**Returns** `Enumerator<T>`&lt;`T`&gt; — The enumerator, which throws when the stack changes while it is being walked.

### `InsertSilently(int, T[])` {#insertsilently-int-t}

```csharp
public void InsertSilently(int index, T[] items);
```

Puts items on without reaching [`AtomsStack.OnInserted`](../arlecchino.atoms.collections/AtomsStack-1.md#oninserted-int-t).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where they go, counting from the top. |
| `items` | `T`\[\] | What to put on, top first. |

### `OnInserted(int, T[])` {#oninserted-int-t}

```csharp
public virtual void OnInserted(int index, T[] items);
```

Called once items went on, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where they went, counting from the top. |
| `items` | `T`\[\] | What went on, top first. |

### `OnRemoved(int, T[])` {#onremoved-int-t}

```csharp
public virtual void OnRemoved(int index, T[] items);
```

Called once items came off, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where they were, counting from the top. |
| `items` | `T`\[\] | What came off, top first. |

### `OnReset(T[], T[])` {#onreset-t-t}

```csharp
public virtual void OnReset(T[] previous, T[] current);
```

Called once the contents were swapped wholesale, and only while [`AtomsCollectionBase.IsObserved`](../arlecchino.atoms.collections/AtomsCollectionBase.md#isobserved).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `T`\[\] | What was on the stack, top first. |
| `current` | `T`\[\] | What is on it now, top first. |

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

Puts several on at once, the last of them ending up on top — what a loop of [`AtomsStack.Push`](../arlecchino.atoms.collections/AtomsStack-1.md#push-t) would leave, in one notification and one undo step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What to put on. Putting none on changes nothing. |

### `RemoveSilently(int, int)` {#removesilently-int-int}

```csharp
public void RemoveSilently(int index, int count);
```

Takes items off without reaching [`AtomsStack.OnRemoved`](../arlecchino.atoms.collections/AtomsStack-1.md#onremoved-int-t).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` | Where to start, counting from the top. |
| `count` | `int` | How many. |

### `Reset(IReadOnlyList<T>)` {#reset-ireadonlylist-t}

```csharp
public void Reset(IReadOnlyList<T> items);
```

Replaces what is on the stack in one go, for a stack that is rebuilt rather than unwound. Contents equal to what is already there change nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `IReadOnlyList<T>`&lt;`T`&gt; | What should be on it instead, top first. |

### `ResetSilently(T[])` {#resetsilently-t}

```csharp
public void ResetSilently(T[] items);
```

Swaps the contents without reaching [`AtomsStack.OnReset`](../arlecchino.atoms.collections/AtomsStack-1.md#onreset-t-t).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `T`\[\] | What the stack should hold, top first. |

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

