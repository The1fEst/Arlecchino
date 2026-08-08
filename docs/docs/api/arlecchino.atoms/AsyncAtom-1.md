---
title: "AsyncAtom<T>"
sidebar_label: "AsyncAtom<T>"
---

# AsyncAtom&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino`

A value produced by background work, with its progress exposed as state, so the view can draw a spinner or an error without knowing anything about the task. Results are handed back on the UI thread, and a new load cancels the one before it, so a slow reply can never overwrite a newer one. Nothing here is recorded in history, because loading is not something the user undoes.

```csharp
public sealed class AsyncAtom<T> : IReadableAtom<T>
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`T`&gt;

## Constructors

| Member | Summary |
|---|---|
| [`AsyncAtom(T)`](#asyncatom-t) | Creates the state, without starting anything. |

## Properties

| Member | Summary |
|---|---|
| [`Error`](#error) | What the last load threw, or `null` when it did not fail. |
| [`IsLoading`](#isloading) | Whether a load is running right now. |
| [`Status`](#status) | Progress of the last load, for showing a spinner or an error. |
| [`Value`](#value) | The last loaded value. It stays put while a new load runs, so the view keeps its content. |

## Methods

| Member | Summary |
|---|---|
| [`Cancel()`](#cancel) | Abandons the running load. Whatever was loaded before stays put — the user asked to stop waiting, not to lose what the screen shows — but the state stops reporting itself as loading, so a spinner bound to it does not spin forever. |
| [`Load(Func<CancellationToken, Task<T>>)`](#load-func-cancellationtoken-task-t) | Starts work in the background, canceling whatever was already running. Returns at once; the result, or the failure, arrives later on the UI thread. |
| [`Subscribe(Action)`](#subscribe-action) | Watches for a new value. Progress changes on their own do not notify. |
| [`SubscribeToStatus(Action)`](#subscribetostatus-action) | Watches progress, which is what a spinner or an error line needs. |

## Constructors in detail

### `AsyncAtom(T)` {#asyncatom-t}

```csharp
public AsyncAtom(T initial);
```

Creates the state, without starting anything.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `T` | What to hold until the first load finishes. |

## Properties in detail

### `Error` {#error}

```csharp
public IReadableAtom<Exception> Error { get; }
```

What the last load threw, or `null` when it did not fail.

**Type** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`Exception`&gt;

### `IsLoading` {#isloading}

```csharp
public bool IsLoading { get; }
```

Whether a load is running right now.

**Type** `bool`

### `Status` {#status}

```csharp
public IReadableAtom<LoadStatus> Status { get; }
```

Progress of the last load, for showing a spinner or an error.

**Type** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;[`LoadStatus`](../arlecchino.atoms/LoadStatus.md)&gt;

### `Value` {#value}

```csharp
public T Value { get; }
```

The last loaded value. It stays put while a new load runs, so the view keeps its content.

**Type** `T`

## Methods in detail

### `Cancel()` {#cancel}

```csharp
public void Cancel();
```

Abandons the running load. Whatever was loaded before stays put — the user asked to stop waiting, not to lose what the screen shows — but the state stops reporting itself as loading, so a spinner bound to it does not spin forever.

### `Load(Func<CancellationToken, Task<T>>)` {#load-func-cancellationtoken-task-t}

```csharp
public void Load(Func<CancellationToken, Task<T>> load);
```

Starts work in the background, canceling whatever was already running. Returns at once; the result, or the failure, arrives later on the UI thread.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `load` | `Func<T, TResult>`&lt;`CancellationToken`, `Task<TResult>`&lt;`T`&gt;&gt; | The work to run, given a token that is canceled when a newer load starts. |

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Watches for a new value. Progress changes on their own do not notify.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | Called after the value changes. |

**Returns** `IDisposable` — Dispose to stop listening.

### `SubscribeToStatus(Action)` {#subscribetostatus-action}

```csharp
public IDisposable SubscribeToStatus(Action listener);
```

Watches progress, which is what a spinner or an error line needs.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | Called after the status changes. |

**Returns** `IDisposable` — Dispose to stop listening.

