---
title: Repaint
sidebar_label: Repaint
---

# Repaint class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino`

The "this frame is stale" signal the render loop waits on. Input, navigation, state changes and atom writes raise it for you; raise it yourself when something else changes what a view draws.

```csharp
public sealed class Repaint : IDisposable
```

**Implements** `IDisposable`

## Constructors

| Member | Summary |
|---|---|
| [`Repaint()`](#repaint) | Starts listening for atom writes, so any of them marks the frame stale. |

## Properties

| Member | Summary |
|---|---|
| [`IsRequested`](#isrequested) | Whether a frame is owed. Reading it does not consume the request. |

## Methods

| Member | Summary |
|---|---|
| [`Dispose()`](#dispose) | Stops listening for atom writes. |
| [`Request()`](#request) | Asks for a frame. Safe to call from any thread and cheap to call repeatedly. |
| [`TakeRequested()`](#takerequested) | Consumes the request; the render loop calls this once per tick. |

## Constructors in detail

### `Repaint()` {#repaint}

```csharp
public Repaint();
```

Starts listening for atom writes, so any of them marks the frame stale.

## Properties in detail

### `IsRequested` {#isrequested}

```csharp
public bool IsRequested { get; }
```

Whether a frame is owed. Reading it does not consume the request.

**Type** `bool`

## Methods in detail

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Stops listening for atom writes.

### `Request()` {#request}

```csharp
public void Request();
```

Asks for a frame. Safe to call from any thread and cheap to call repeatedly.

### `TakeRequested()` {#takerequested}

```csharp
public bool TakeRequested();
```

Consumes the request; the render loop calls this once per tick.

**Returns** `bool` — `true` when a frame was owed.

