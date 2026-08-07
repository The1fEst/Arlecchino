---
title: "Computed<T>"
sidebar_label: "Computed<T>"
---

# Computed&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A value derived from other atoms. It re-evaluates lazily and tracks whatever it read while doing so, including other computed values and branches taken only sometimes — reading `other.Value` inside the lambda is the subscription.

```csharp
public sealed class Computed<T> : IReadableAtom<T>
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`T`&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Computed(Func<T>)`](#computed-func-t) | Creates a derived value. |

## Properties

| Member | Summary |
|---|---|
| [`Value`](#value) | The derived value, recomputed on the first read after any dependency changed. |

## Methods

| Member | Summary |
|---|---|
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the derived value goes stale. |

## Constructors in detail

### `Computed(Func<T>)` {#computed-func-t}

```csharp
public Computed(Func<T> compute);
```

Creates a derived value.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `compute` | `Func<TResult>`&lt;`T`&gt; | How to work it out. Reads of other atoms inside become the dependencies, so it may read different ones on different runs. |

## Properties in detail

### `Value` {#value}

```csharp
public T Value { get; }
```

The derived value, recomputed on the first read after any dependency changed.

**Type** `T`

## Methods in detail

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Calls back whenever the derived value goes stale.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening.

