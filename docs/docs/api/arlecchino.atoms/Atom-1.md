---
title: "Atom<T>"
sidebar_label: "Atom<T>"
---

# Atom&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

One piece of application state that notifies what reads it and marks the frame stale by itself.

```csharp
public class Atom<T> : IReadableAtom<T>, IReadableAtom
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`T`&gt;, [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md)

## Constructors

| Member | Summary |
|---|---|
| [`Atom(T, IEqualityComparer<T>)`](#atom-t-iequalitycomparer-t) | Creates an atom holding a starting value. |

## Properties

| Member | Summary |
|---|---|
| [`Value`](#value) | The value. Writing an equal value changes nothing; any other write notifies subscribers and asks for a repaint. |

## Methods

| Member | Summary |
|---|---|
| [`OnWritten(T, T)`](#onwritten-t-t) | Called once the value has changed and before subscribers hear of it. The atom does nothing with it; it is where a subclass keeps a record of its own, and [`Atom.WriteSilently`](../arlecchino.atoms/Atom-1.md#writesilently-t) does not reach it. |
| [`Post(T)`](#post-t) | Hands a value to the drawing thread, to be written just before the next frame in the order it was posted. Nothing is written when this returns, and atoms that must change together belong in one `FrameThread.Post`. |
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the value changes. |
| [`ToString()`](#tostring) |  |
| [`WriteSilently(T)`](#writesilently-t) | Writes without reaching [`Atom.OnWritten`](../arlecchino.atoms/Atom-1.md#onwritten-t-t). It is how a subclass puts a value back: replaying what it recorded must not be recorded again. |

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

### `Value` {#value}

```csharp
public T Value { get; set; }
```

The value. Writing an equal value changes nothing; any other write notifies subscribers and asks for a repaint.

**Type** `T`

## Methods in detail

### `OnWritten(T, T)` {#onwritten-t-t}

```csharp
public virtual void OnWritten(T previous, T current);
```

Called once the value has changed and before subscribers hear of it. The atom does nothing with it; it is where a subclass keeps a record of its own, and [`Atom.WriteSilently`](../arlecchino.atoms/Atom-1.md#writesilently-t) does not reach it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `T` | What the atom held. |
| `current` | `T` | What it holds now. |

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

### `ToString()` {#tostring}

```csharp
public virtual string ToString();
```

**Returns** `string`

### `WriteSilently(T)` {#writesilently-t}

```csharp
public void WriteSilently(T value);
```

Writes without reaching [`Atom.OnWritten`](../arlecchino.atoms/Atom-1.md#onwritten-t-t). It is how a subclass puts a value back: replaying what it recorded must not be recorded again.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `T` | The value to write. |

