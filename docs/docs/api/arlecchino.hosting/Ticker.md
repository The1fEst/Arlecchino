---
title: Ticker
sidebar_label: Ticker
---

# Ticker class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Work on a clock, run on the frame loop. A terminal application redraws when something asks it to, so anything that changes on its own — a spinner, a clock, a list that refreshes itself, a message that fades — needs someone to say when. That someone is this: schedule an action and it runs between frames, on the same thread as drawing and input, with a repaint asked for afterwards. Every schedule returns the handle that cancels it. Hand it to [`ViewLifetime.Track`](../arlecchino.navigation/ViewLifetime.md#track-t-t) and the work stops when the screen goes away. Missed time is not made up for: an action runs at most once per pass, so a loop that was held up — a window that came back from being minimised, a long operation, a debugger — resumes with a single run rather than firing everything it slept through.

```csharp
public sealed class Ticker
```

## Constructors

| Member | Summary |
|---|---|
| [`Ticker(TimeProvider, Repaint)`](#ticker-timeprovider-repaint) | Creates the ticker. |

## Properties

| Member | Summary |
|---|---|
| [`NextDue`](#nextdue) | When the next scheduled action is due, or `null` when nothing is scheduled. |

## Methods

| Member | Summary |
|---|---|
| [`After(TimeSpan, Action)`](#after-timespan-action) | Runs an action once, after the delay. |
| [`Every(TimeSpan, Action)`](#every-timespan-action) | Runs an action over and over, waiting the interval between runs. |
| [`Run(Action<Exception>)`](#run-action-exception) | Runs whatever is due. Called by the frame loop; a headless host calls it after moving its own clock forward. |

## Constructors in detail

### `Ticker(TimeProvider, Repaint)` {#ticker-timeprovider-repaint}

```csharp
public Ticker(TimeProvider time, Repaint repaint);
```

Creates the ticker.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `time` | `TimeProvider` | Where the current time comes from; a test host supplies its own. |
| `repaint` | [`Repaint`](../arlecchino/Repaint.md) | Asked for a frame after anything runs. |

## Properties in detail

### `NextDue` {#nextdue}

```csharp
public Nullable<DateTimeOffset> NextDue { get; }
```

When the next scheduled action is due, or `null` when nothing is scheduled.

**Type** `Nullable<T>`&lt;`DateTimeOffset`&gt;

## Methods in detail

### `After(TimeSpan, Action)` {#after-timespan-action}

```csharp
public IDisposable After(TimeSpan delay, Action action);
```

Runs an action once, after the delay.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delay` | `TimeSpan` | How long to wait; anything below a millisecond is raised to one. |
| `action` | `Action` | What to run. |

**Returns** `IDisposable` — Dispose it to cancel before it runs.

### `Every(TimeSpan, Action)` {#every-timespan-action}

```csharp
public IDisposable Every(TimeSpan interval, Action action);
```

Runs an action over and over, waiting the interval between runs.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `interval` | `TimeSpan` | How long to wait each time; anything below a millisecond is raised to one. |
| `action` | `Action` | What to run. |

**Returns** `IDisposable` — Dispose it to stop.

### `Run(Action<Exception>)` {#run-action-exception}

```csharp
public void Run(Action<Exception> onError);
```

Runs whatever is due. Called by the frame loop; a headless host calls it after moving its own clock forward.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `onError` | `Action<T>`&lt;`Exception`&gt; | What to do with an action that threw; the rest still run. |

