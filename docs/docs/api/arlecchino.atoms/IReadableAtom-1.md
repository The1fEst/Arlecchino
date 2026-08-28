---
title: "IReadableAtom<T>"
sidebar_label: "IReadableAtom<T>"
---

# IReadableAtom&lt;T&gt; interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

Something that holds a value and tells interested parties when it changes. Implemented by [`Atom`](../arlecchino.atoms/Atom-1.md) and [`Computed`](../arlecchino.atoms/Computed-1.md).

```csharp
public interface IReadableAtom<T> : IReadableAtom
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md)

## Properties

| Member | Summary |
|---|---|
| [`Value`](#value) | The current value. Reading it inside a [`Computed`](../arlecchino.atoms/Computed-1.md) registers the dependency, so derived values need no dependency list. |

## Methods

| Member | Summary |
|---|---|
| [`ToString()`](#tostring) | The value written out, which is what a widget shows when it is given no wording of its own. |

## Properties in detail

### `Value` {#value}

```csharp
public abstract T Value { get; }
```

The current value. Reading it inside a [`Computed`](../arlecchino.atoms/Computed-1.md) registers the dependency, so derived values need no dependency list.

**Type** `T`

## Methods in detail

### `ToString()` {#tostring}

```csharp
public abstract string ToString();
```

The value written out, which is what a widget shows when it is given no wording of its own.

**Returns** `string` — The text.

