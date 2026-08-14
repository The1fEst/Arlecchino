---
title: "ViewLifetime"
sidebar_label: "ViewLifetime"
---

# ViewLifetime class

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

How long the screen is on. Take it in a view's constructor to tie background work and subscriptions to the screen, and navigating away cancels the token and releases everything registered here.

```csharp
public sealed class ViewLifetime : IDisposable
```

**Implements** `IDisposable`

## Constructors

| Member | Summary |
|---|---|
| [`ViewLifetime()`](#viewlifetime) | Creates the lifetime. Resolved once per screen. |

## Properties

| Member | Summary |
|---|---|
| [`Closing`](#closing) | Canceled when the screen goes away, to be passed into work started by the view. It stays readable afterward, so work coming back late can see the screen has gone. |

## Methods

| Member | Summary |
|---|---|
| [`Dispose()`](#dispose) | Cancels the token and releases everything tracked. Called by the container when the screen's scope ends; there is no need to call it from a view. |
| [`Loading<T>(T)`](#loading-t-t) | Creates background state that stops when the screen does — the usual way to load something for one screen. |
| [`OnClose(Action)`](#onclose-action) | Runs something when the screen goes away, before the scope is released. |
| [`Track<T>(T)`](#track-t-t) | Hands something over to the screen's lifetime — a subscription, a timer, a file handle. It is disposed when the screen goes away, in the order it was handed over. |

## Constructors in detail

### `ViewLifetime()` {#viewlifetime}

```csharp
public ViewLifetime();
```

Creates the lifetime. Resolved once per screen.

## Properties in detail

### `Closing` {#closing}

```csharp
public CancellationToken Closing { get; }
```

Canceled when the screen goes away, to be passed into work started by the view. It stays readable afterward, so work coming back late can see the screen has gone.

**Type** `CancellationToken`

## Methods in detail

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Cancels the token and releases everything tracked. Called by the container when the screen's scope ends; there is no need to call it from a view.

### `Loading<T>(T)` {#loading-t-t}

```csharp
public AsyncAtom<T> Loading<T>(T initial);
```

Creates background state that stops when the screen does — the usual way to load something for one screen.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `T` | What to hold until the first load finishes. |

**Returns** [`AsyncAtom`](../arlecchino.atoms/AsyncAtom-1.md)&lt;`T`&gt; — The state, already tied to this screen.

### `OnClose(Action)` {#onclose-action}

```csharp
public void OnClose(Action action);
```

Runs something when the screen goes away, before the scope is released.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `action` | `Action` | What to run. |

### `Track<T>(T)` {#track-t-t}

```csharp
public T Track<T>(T resource);
```

Hands something over to the screen's lifetime — a subscription, a timer, a file handle. It is disposed when the screen goes away, in the order it was handed over.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `resource` | `T` | What to look after. |

**Returns** `T` — The same object, so it can be assigned in one line.

