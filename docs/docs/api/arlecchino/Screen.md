---
title: Screen
sidebar_label: Screen
---

# Screen class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino`

Draws the frames: the current view first, then the output line, the hints and any dialog on top. A view that throws while drawing is reported on the output line instead of taking the application down, since a half-drawn frame is easier to recover from than a dead process.

```csharp
public class Screen
```

## Methods

| Member | Summary |
|---|---|
| [`DrawOnce()`](#drawonce) | Draws one full frame, forgetting what was on screen first. Redrawing everything is what makes this usable outside the loop — in tests, or after something else has written to the terminal. |
| [`RedrawEverything()`](#redraweverything) | Asks for the next frame to be drawn from scratch rather than as a difference. Safe from any thread, and needed whenever something outside the framework has written over the screen — coming back from a suspended process, for one. |
| [`Run(CancellationToken)`](#run-cancellationtoken) | Draws until stopped, at the configured rate. A frame is only built when something asked for one or the terminal changed size, so an idle application costs nothing. |

## Methods in detail

### `DrawOnce()` {#drawonce}

```csharp
public void DrawOnce();
```

Draws one full frame, forgetting what was on screen first. Redrawing everything is what makes this usable outside the loop — in tests, or after something else has written to the terminal.

### `RedrawEverything()` {#redraweverything}

```csharp
public void RedrawEverything();
```

Asks for the next frame to be drawn from scratch rather than as a difference. Safe from any thread, and needed whenever something outside the framework has written over the screen — coming back from a suspended process, for one.

### `Run(CancellationToken)` {#run-cancellationtoken}

```csharp
public Task Run(CancellationToken stoppingToken);
```

Draws until stopped, at the configured rate. A frame is only built when something asked for one or the terminal changed size, so an idle application costs nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `stoppingToken` | `CancellationToken` | Cancelled when the application is shutting down. |

**Returns** `Task` — A task that completes once drawing has stopped.

