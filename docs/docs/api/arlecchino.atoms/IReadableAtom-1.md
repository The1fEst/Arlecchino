---
title: IReadableAtom&lt;T&gt;
sidebar_label: IReadableAtom&lt;T&gt;
---

# IReadableAtom&lt;T&gt; interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

Something that holds a value and tells interested parties when it changes. Implemented by [`Atom`](../arlecchino.atoms/Atom-1.md) and [`Computed`](../arlecchino.atoms/Computed-1.md).

```csharp
public interface IReadableAtom<T>
```

## Properties

| Member | Summary |
|---|---|
| [`Value`](#value) | The current value. Reading it inside a [`Computed`](../arlecchino.atoms/Computed-1.md) also registers the dependency, which is why derived values need no dependency list. |

## Methods

| Member | Summary |
|---|---|
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the value changes. |

## Properties in detail

### `Value` {#value}

```csharp
public abstract T Value { get; }
```

The current value. Reading it inside a [`Computed`](../arlecchino.atoms/Computed-1.md) also registers the dependency, which is why derived values need no dependency list.

**Type** `T`

## Methods in detail

### `Subscribe(Action)` {#subscribe-action}

```csharp
public abstract IDisposable Subscribe(Action listener);
```

Calls back whenever the value changes.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening; a view that subscribes must do so when it goes away.

