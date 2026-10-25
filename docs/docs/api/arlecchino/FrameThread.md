---
title: "FrameThread"
sidebar_label: "FrameThread"
---

# FrameThread class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino.Core`

Which thread draws, claimed by the frame loop as it starts. Views, widgets, atoms and the surface are written without locks, and this is what turns that convention into something the framework checks.

```csharp
public static class FrameThread
```

## Properties

| Member | Summary |
|---|---|
| [`HasPending`](#haspending) | Whether anything posted is still waiting to run. |
| [`IsCurrent`](#iscurrent) | Whether the calling thread is the one drawing, or nothing has claimed drawing yet. |

## Methods

| Member | Summary |
|---|---|
| [`Claim(Action)`](#claim-action) | Claims the calling thread as the one that draws. An application running the frame loop itself calls this too, so the checks know which thread is meant. |
| [`DiscardPending()`](#discardpending) | Drops what was posted and never run. An application going away calls it, and so does a test host as it is disposed, so that work left over by one does not run inside the next. |
| [`Post(Action)`](#post-action) | Hands work to the drawing thread, to run just before the next frame in the order it was posted. With no thread drawing it waits for [`FrameThread.RunPending`](../arlecchino/FrameThread.md#runpending-action-exception). |
| [`Post(Func<Task>)`](#post-func-task) | Hands asynchronous work to the drawing thread. It starts there, and every `await` inside it that was not told otherwise comes back there, so what it reads and writes is what a frame draws. |
| [`RunPending(Action<Exception>)`](#runpending-action-exception) | Runs what was posted before this call. Called by the frame loop; work posted by that work waits for the next frame, so an action that posts itself is a loop you can leave. |
| [`Verify(string)`](#verify-string) | Throws unless the caller is on the drawing thread. This is what a member that changes what a frame draws calls before changing anything. |

## Properties in detail

### `HasPending` {#haspending}

```csharp
public static bool HasPending { get; }
```

Whether anything posted is still waiting to run.

**Type** `bool`

### `IsCurrent` {#iscurrent}

```csharp
public static bool IsCurrent { get; }
```

Whether the calling thread is the one drawing, or nothing has claimed drawing yet.

**Type** `bool`

## Methods in detail

### `Claim(Action)` {#claim-action}

```csharp
public static IDisposable Claim(Action? wake = null);
```

Claims the calling thread as the one that draws. An application running the frame loop itself calls this too, so the checks know which thread is meant.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `wake` | `Action` | Asks for a frame, called whenever something is posted. The frame loop passes its repaint signal, so posted work is drawn without the caller having to ask. |

**Returns** `IDisposable` — A scope that gives the claim up again. Giving up the last claim drops what is still posted, since no frame is left for it to run before.

### `DiscardPending()` {#discardpending}

```csharp
public static void DiscardPending();
```

Drops what was posted and never run. An application going away calls it, and so does a test host as it is disposed, so that work left over by one does not run inside the next.

### `Post(Action)` {#post-action}

```csharp
public static void Post(Action action);
```

Hands work to the drawing thread, to run just before the next frame in the order it was posted. With no thread drawing it waits for [`FrameThread.RunPending`](../arlecchino/FrameThread.md#runpending-action-exception).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `action` | `Action` | What to run where it is safe to change what a frame draws. |

### `Post(Func<Task>)` {#post-func-task}

```csharp
public static void Post(Func<Task> work);
```

Hands asynchronous work to the drawing thread. It starts there, and every `await` inside it that was not told otherwise comes back there, so what it reads and writes is what a frame draws.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `work` | `Func<TResult>`&lt;`Task`&gt; | What to run. Whatever it throws, before an `await` or after one, reaches the frame loop the way a posted action's failure does; being canceled is not a failure. |

### `RunPending(Action<Exception>)` {#runpending-action-exception}

```csharp
public static void RunPending(Action<Exception> onError);
```

Runs what was posted before this call. Called by the frame loop; work posted by that work waits for the next frame, so an action that posts itself is a loop you can leave.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `onError` | `Action<T>`&lt;`Exception`&gt; | What to do with an action that threw. |

### `Verify(string)` {#verify-string}

```csharp
public static void Verify(string member);
```

Throws unless the caller is on the drawing thread. This is what a member that changes what a frame draws calls before changing anything.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `member` | `string` | What was called, named in the message. |

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | The caller is on another thread. |

