---
title: "Atom<T>"
sidebar_label: "Atom<T>"
---

# Atom&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

One piece of application state that notifies what reads it and marks the frame stale by itself. Whether an edit can be undone is decided by creating a [`TrackedAtom`](../arlecchino.atoms.tracked/TrackedAtom-1.md) or a [`LocalAtom`](../arlecchino.atoms.local/LocalAtom-1.md).

```csharp
public abstract class Atom<T> : IReadableAtom<T>
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`T`&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Atom(T, IEqualityComparer<T>)`](#atom-t-iequalitycomparer-t) | Creates an atom holding a starting value. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) | Whether edits of this atom enter the undo history. |
| [`Value`](#value) | The value. Writing an equal value changes nothing; any other write notifies subscribers, asks for a repaint, and records an undo step when the atom is undoable. |

## Methods

| Member | Summary |
|---|---|
| [`Post(T)`](#post-t) | Hands a value to the drawing thread, to be written just before the next frame in the order it was posted. Nothing is written when this returns, and atoms that must change together belong in one `FrameThread.Post`. |
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the value changes. |

## Constructors in detail

### `Atom(T, IEqualityComparer<T>)` {#atom-t-iequalitycomparer-t}

```csharp
public Atom(T initial, IEqualityComparer<T> comparer);
```

Creates an atom holding a starting value.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `T` | The value to start with. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How to decide that writing to it changed nothing; the default comparer for `T` is used when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public abstract bool RecordsHistory { get; }
```

Whether edits of this atom enter the undo history.

**Type** `bool`

### `Value` {#value}

```csharp
public T Value { get; set; }
```

The value. Writing an equal value changes nothing; any other write notifies subscribers, asks for a repaint, and records an undo step when the atom is undoable.

**Type** `T`

## Methods in detail

### `Post(T)` {#post-t}

```csharp
public void Post(T value);
```

Hands a value to the drawing thread, to be written just before the next frame in the order it was posted. Nothing is written when this returns, and atoms that must change together belong in one `FrameThread.Post`.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `T` | The value to write on the drawing thread. |

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Calls back whenever the value changes.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening.

