---
title: The frame loop
sidebar_label: The frame loop
description: Screen, Repaint, FrameThread and Ticker — when a frame is drawn, which thread draws it, and how work from anywhere else gets back onto it.
---

# The frame loop

`Screen` owns the loop. It runs on a thread of its own called `arlecchino-frames`, and that thread is
the only one allowed to touch a view, a widget, an atom or the surface. Everything on this page is
about when that thread does something and how work from elsewhere reaches it.

## One pass of the loop

The loop wakes at `TargetFramesPerSecond` (60 by default) and does four things:

1. **Drains input.** Keys, mouse events and pastes read by the reader thread are routed here, on the
   drawing thread, before anything is drawn. See [Keyboard](keyboard.md).
2. **Runs the ticker.** Anything scheduled with [`Ticker`](#work-on-a-clock) that is due runs now.
3. **Draws a frame, if one is owed.** `Repaint.TakeRequested()` consumes the request; a change of
   terminal size counts as one too.
4. **Sleeps** for whatever is left of the interval, on the cancellation token's wait handle, so
   shutdown does not wait out the tick.

An idle application draws nothing and writes nothing to the terminal.

## What one frame is made of

`DrawFrame` composes the whole screen in this order:

1. `FrameThread.RunPending` — work handed over from other threads.
2. `Surface.StartFrame()` — reads the terminal size, reallocates the planes if it changed, clears
   every cell to a space styled `Theme.Default`, and skips `VerticalPadding` rows.
3. If the terminal is smaller than `MinimumWidth` × `MinimumHeight`, a size notice is drawn and the
   frame ends here — the view is not asked to draw at all.
4. The current view's `Draw()`, through the [navigator](views-and-navigation.md).
5. The output line, when `ShowOutputLine` is on.
6. The hints box, when `ShowHints` is on and no modal is open.
7. The [log overlay](diagnostics.md), while it is visible.
8. Every open [modal](modals.md), innermost last, each offset three columns and one row from the one
   below it.
9. `Surface.Build()` — the composed frame goes to the terminal as one write.

Nothing reaches the terminal until `Build`, so a half-drawn frame is never visible.

:::note[A view that throws does not take the process down]

`Draw` is called inside a `try`. A view that throws is logged and reported on the output line through
`ArlecchinoStrings.ViewFailed`, and the rest of the frame — output line, hints, modals — is still
composed. A dead process is harder to recover from than a broken screen.

:::

## Frames are drawn on request

The loop does not repaint every tick; it repaints when something asks. `Repaint.Request()` is called
for you:

| Who asks | When |
|---|---|
| the input router | after every key, mouse event and paste |
| the navigator | on every route change |
| `ArlecchinoState` | when `Output`, a modal or the file picker is assigned |
| every atom | on every write that actually changed the value — `Repaint` subscribes to `AtomChanges.Written` |
| `Ticker` | after anything scheduled has run |
| `FrameThread.Post` | as the work is queued |
| the loop itself | when the terminal changed size |

Anything else that changes what a view draws has to say so:

```csharp
_state.Invalidate();   // or Repaint.Request() from the service itself
```

A view that animates can call it from its own `Draw`, which effectively opts back into drawing every
tick.

`Repaint` starts out requested, so the first frame is always drawn.

## Drawing everything again

`Build` writes only the cells that differ from the previous frame, jumping the cursor to each changed
run. That breaks down when something outside the framework has written over the terminal — a process
that was suspended and resumed, a child process that printed something. Two calls fix it:

| Call | Effect |
|---|---|
| `Screen.RedrawEverything()` | Safe from any thread. Marks the next frame as a full send and asks for one. |
| `Screen.DrawOnce()` | Draws one full frame right now, on the calling thread. This is what headless rendering uses. |

`Surface.ForgetPreviousFrame()` is the surface-level version of the same thing.

## Which thread draws

`FrameThread` turns "one thread touches this" from a convention into something the framework checks.
The loop claims the thread as it starts:

```csharp
using var drawing = FrameThread.Claim(_repaint.Request);
```

From then on, a member that changes what a frame draws calls `FrameThread.Verify(nameof(Member))`
first, and throws from anywhere else:

> `Atom.Value` was called from thread 7, but frames are drawn on thread 4. Views, widgets and atoms
> are not thread-safe: hand the change over with `FrameThread.Post`, which runs it just before the
> next frame.

Nothing claims the thread outside a running application — a headless host, a test, a single
`DrawOnce` — so the checks stay quiet there and cost one comparison.

| Member | Meaning |
|---|---|
| `FrameThread.IsCurrent` | Whether this is the drawing thread, or nothing is drawing at all |
| `FrameThread.Claim(wake)` | Claims the calling thread; dispose the result to give it back |
| `FrameThread.Post(action)` | Hands work over from any thread |
| `FrameThread.HasPending` | Whether anything posted is still waiting |
| `FrameThread.RunPending(onError)` | Runs what was posted; the loop calls this each frame |
| `FrameThread.DiscardPending()` | Drops what was posted and never ran |
| `FrameThread.Verify(member)` | Throws unless the caller is on the drawing thread |

## Coming back from a background task

Work that finishes on another thread hands its result back through `Post`:

```csharp
public sealed class ModsView : IArlecchinoView
{
    private readonly ModsService _mods;
    private IReadOnlyList<Mod> _rows = [];

    public void Reload() => Task.Run(async () =>
    {
        var loaded = await _mods.LoadAsync();
        FrameThread.Post(() => _rows = loaded);
    });
}
```

`Post` is safe from any thread, queues in order, and asks for a repaint by itself. An action that
throws is logged and reported on the output line — the remaining actions still run.

A frame runs what was waiting when it started, and no more. Work posted by that work belongs to the
next frame, so an action that posts itself is a once-a-frame loop rather than a frame that never
ends — which is the shape "carry on next frame" naturally takes.

For loading data this way without writing the plumbing, see [Async atoms](async-atoms.md).

:::warning[A collection that shrinks mid-frame]

Widgets read their rows while drawing. If a background thread replaces a list halfway through a
frame, the widget stops early and `DrawFaults` counts the cut-short rows; `Screen` logs a warning
naming the route. It is a symptom of a change that skipped `Post`, not something to tune around.

:::

## Work on a clock

`Ticker` is a service in the container. Schedule an action and it runs between frames, on the drawing
thread, with a repaint asked for afterwards:

```csharp
public sealed class ClockView : IArlecchinoView
{
    private readonly Ticker _ticker;
    private readonly ViewLifetime _lifetime;

    public ClockView(Ticker ticker, ViewLifetime lifetime)
    {
        _ticker = ticker;
        _lifetime = lifetime;
        _lifetime.Track(_ticker.Every(TimeSpan.FromSeconds(1), () => _now = DateTime.Now));
    }
}
```

| Member | Meaning |
|---|---|
| `Every(interval, action)` | Repeats, waiting the interval between runs |
| `After(delay, action)` | Runs once |
| `NextDue` | When the next action is due, or `null` |
| `Run(onError)` | Runs whatever is due; the loop calls it, a headless host calls it after moving its clock |

Both schedules return the handle that cancels them. Hand it to
[`ViewLifetime.Track`](views-and-navigation.md) and the work stops when the screen goes away.

Missed time is not made up for: an action runs at most once per pass, so a loop that was held up — a
window restored from being minimised, a long operation, a debugger — resumes with a single run rather
than firing everything it slept through.

`Ticker` takes its time from `TimeProvider`, which is why the [test host](testing.md) can move the
clock forward instead of sleeping.

## Running the loop yourself

`AddArlecchino` registers a hosted service that calls `Screen.Run`. An application that would rather
drive it — a single frame to stdout, a frame per input event — resolves `Screen` and calls `DrawOnce`,
and claims the thread itself if it wants the checks to work. [Hosting and options](hosting-and-options.md)
has the wiring.
