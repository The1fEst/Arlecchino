---
title: "ArlecchinoAsyncStore"
sidebar_label: "ArlecchinoAsyncStore"
---

# ArlecchinoAsyncStore class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino`

A store that has to fetch something before it holds the truth. Override [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken), and the load is started as the application starts and its bookkeeping kept.

```csharp
public sealed class SettingsStore : ArlecchinoAsyncStore
{
public TrackedAtom<string> Server { get; } = new("127.0.0.1");

protected override async Task LoadAsync(CancellationToken token)
{
await using var fs = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read);
var saved = await JsonSerializer.DeserializeAsync<Saved>(fs, cancellationToken: token);

Server.Post(saved.Server);
}
}

```

```csharp
public abstract class ArlecchinoAsyncStore : IArlecchinoStore
```

**Implements** [`IArlecchinoStore`](../arlecchino.atoms/IArlecchinoStore.md)

## Constructors

| Member | Summary |
|---|---|
| [`ArlecchinoAsyncStore()`](#arlecchinoasyncstore) | Creates the store. |

## Properties

| Member | Summary |
|---|---|
| [`Error`](#error) | What the load threw, or `null` while it has not failed. |
| [`Failed`](#failed) | Whether the load threw. What it threw is in [`ArlecchinoAsyncStore.Error`](../arlecchino.atoms/ArlecchinoAsyncStore.md#error). |
| [`IsLoaded`](#isloaded) | Whether the load finished and the atoms hold what it fetched. |
| [`IsLoading`](#isloading) | Whether the load is still running. |
| [`Ready`](#ready) | Completes when the store is loaded, faults with whatever [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken) threw, and cancels when the application stopped first. A view reads [`ArlecchinoAsyncStore.Status`](../arlecchino.atoms/ArlecchinoAsyncStore.md#status) instead of awaiting it. |
| [`Status`](#status) | How the load is going, as an atom, so a view that reads it redraws when it changes. |

## Methods

| Member | Summary |
|---|---|
| [`LoadAsync(CancellationToken)`](#loadasync-cancellationtoken) | Fetches what the store needs, off the drawing thread, so what it loads reaches the atoms through `Post`. Throwing is a normal outcome and turns the status to failed. |

## Constructors in detail

### `ArlecchinoAsyncStore()` {#arlecchinoasyncstore}

```csharp
public ArlecchinoAsyncStore();
```

Creates the store.

## Properties in detail

### `Error` {#error}

```csharp
public IReadableAtom<Exception?> Error { get; }
```

What the load threw, or `null` while it has not failed.

**Type** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`Exception`&gt;

### `Failed` {#failed}

```csharp
public bool Failed { get; }
```

Whether the load threw. What it threw is in [`ArlecchinoAsyncStore.Error`](../arlecchino.atoms/ArlecchinoAsyncStore.md#error).

**Type** `bool`

### `IsLoaded` {#isloaded}

```csharp
public bool IsLoaded { get; }
```

Whether the load finished and the atoms hold what it fetched.

**Type** `bool`

### `IsLoading` {#isloading}

```csharp
public bool IsLoading { get; }
```

Whether the load is still running.

**Type** `bool`

### `Ready` {#ready}

```csharp
public Task Ready { get; }
```

Completes when the store is loaded, faults with whatever [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken) threw, and cancels when the application stopped first. A view reads [`ArlecchinoAsyncStore.Status`](../arlecchino.atoms/ArlecchinoAsyncStore.md#status) instead of awaiting it.

**Type** `Task`

### `Status` {#status}

```csharp
public IReadableAtom<LoadStatus> Status { get; }
```

How the load is going, as an atom, so a view that reads it redraws when it changes.

**Type** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;[`LoadStatus`](../arlecchino.atoms/LoadStatus.md)&gt;

## Methods in detail

### `LoadAsync(CancellationToken)` {#loadasync-cancellationtoken}

```csharp
public abstract Task LoadAsync(CancellationToken token);
```

Fetches what the store needs, off the drawing thread, so what it loads reaches the atoms through `Post`. Throwing is a normal outcome and turns the status to failed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `token` | `CancellationToken` | Canceled when the application is shutting down. |

**Returns** `Task` — A task that completes when the store is ready.

