---
title: FrameThread
sidebar_label: FrameThread
---

# FrameThread class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino.Core`

Which thread draws. Views, widgets, atoms and the surface are written without locks because one thread touches them, and this is what turns that from a convention into something the framework can check: the frame loop claims the thread it runs on, and everything that must happen there asks before it changes anything. Nothing claims it outside a running application — a headless host, a test, a single `DrawOnce` — so the checks stay quiet there and cost a null comparison.

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
| [`Claim(Action)`](#claim-action) | Claims the calling thread as the one that draws. Called by the frame loop as it starts; an application that runs the loop itself calls it too, so that the checks know where "here" is. |
| [`DiscardPending()`](#discardpending) | Drops what was posted and never run. An application going away calls it, and so does a test host as it is disposed, so that work left over by one does not run inside the next. |
| [`Post(Action)`](#post-action) | Hands work to the drawing thread, from wherever you are. It runs just before the next frame, in the order it was posted, and a frame is asked for by itself. With nobody drawing — a test, a headless render — it waits until something runs it, which is what [`FrameThread.RunPending`](../arlecchino/FrameThread.md#runpending-action-exception) is for. |
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

Claims the calling thread as the one that draws. Called by the frame loop as it starts; an application that runs the loop itself calls it too, so that the checks know where "here" is.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `wake` | `Action` | Asks for a frame, called whenever something is posted. The frame loop passes its repaint signal, so posted work is drawn without the caller having to ask. |

**Returns** `IDisposable` — A scope that gives the claim up again. Giving up the last claim drops what is still posted: with nobody drawing there is no frame left for it to run before.

### `DiscardPending()` {#discardpending}

```csharp
public static void DiscardPending();
```

Drops what was posted and never run. An application going away calls it, and so does a test host as it is disposed, so that work left over by one does not run inside the next.

### `Post(Action)` {#post-action}

```csharp
public static void Post(Action action);
```

Hands work to the drawing thread, from wherever you are. It runs just before the next frame, in the order it was posted, and a frame is asked for by itself. With nobody drawing — a test, a headless render — it waits until something runs it, which is what [`FrameThread.RunPending`](../arlecchino/FrameThread.md#runpending-action-exception) is for.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `action` | `Action` | What to run where it is safe to change what a frame draws. |

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

