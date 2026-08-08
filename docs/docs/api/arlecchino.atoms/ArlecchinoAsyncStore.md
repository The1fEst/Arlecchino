---
title: "ArlecchinoAsyncStore"
sidebar_label: "ArlecchinoAsyncStore"
---

# ArlecchinoAsyncStore class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino`

A store that has to fetch something before it holds the truth — settings read from disk, a session restored from a server, a catalogue that lives in a file. Derive from it, override [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken), and the framework starts the load as the application starts and keeps the bookkeeping: no worker of its own, and no `TaskCompletionSource` written by hand.

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

Reading the file is the application's own code — the framework has nothing to do with disks, formats or paths. The first frame is drawn without waiting: a terminal that hangs black on a slow disk is worse than a screen that says it is loading. A view draws from [`ArlecchinoAsyncStore.Status`](../arlecchino.atoms/ArlecchinoAsyncStore.md#status), which is an atom and so redraws by itself; code that is not a view — a worker, a command that must not run early — awaits [`ArlecchinoAsyncStore.Ready`](../arlecchino.atoms/ArlecchinoAsyncStore.md#ready).

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
| [`Ready`](#ready) | Completes when the store is loaded, faults with whatever [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken) threw, and is canceled when the application stopped before the load finished. This is the one to await outside a view; a view reads [`ArlecchinoAsyncStore.Status`](../arlecchino.atoms/ArlecchinoAsyncStore.md#status) instead, because it draws every frame rather than waiting. |
| [`Status`](#status) | How the load is going, as an atom, so a view that reads it redraws when it changes. |

## Methods

| Member | Summary |
|---|---|
| [`LoadAsync(CancellationToken)`](#loadasync-cancellationtoken) | Fetches what the store needs. It runs off the drawing thread, so what it loads reaches the atoms through `Post` — writing `Value` from here throws, and says so. Throwing is a normal outcome: the status turns to failed, the exception is kept for a view to draw and for [`ArlecchinoAsyncStore.Ready`](../arlecchino.atoms/ArlecchinoAsyncStore.md#ready) to hand to whoever awaits it, and the application carries on with whatever the atoms already hold. |

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

Completes when the store is loaded, faults with whatever [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken) threw, and is canceled when the application stopped before the load finished. This is the one to await outside a view; a view reads [`ArlecchinoAsyncStore.Status`](../arlecchino.atoms/ArlecchinoAsyncStore.md#status) instead, because it draws every frame rather than waiting.

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

Fetches what the store needs. It runs off the drawing thread, so what it loads reaches the atoms through `Post` — writing `Value` from here throws, and says so. Throwing is a normal outcome: the status turns to failed, the exception is kept for a view to draw and for [`ArlecchinoAsyncStore.Ready`](../arlecchino.atoms/ArlecchinoAsyncStore.md#ready) to hand to whoever awaits it, and the application carries on with whatever the atoms already hold.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `token` | `CancellationToken` | Canceled when the application is shutting down. |

**Returns** `Task` — A task that completes when the store is ready.

