---
title: Async atoms
sidebar_label: Async atoms
description: AsyncAtom and ViewLifetime — loading on a background thread, landing the result on the frame loop, and stopping when the screen goes away.
---

# Async atoms

A terminal application has one drawing thread and no way to block it. `AsyncAtom<T>` is the shape work
that happens elsewhere takes: it wraps a load in progress, lands the result on the frame loop through
[`FrameThread`](frame-loop.md#coming-back-from-a-background-task), and exposes the status as an atom so
a view can draw a spinner without knowing anything about tasks.

```csharp
private readonly AsyncAtom<IReadOnlyList<Mod>> _mods = new();
private readonly ModsService _service;

_mods.Load(async token => await _service.LoadAsync(token));
```

| Member | Meaning |
|---|---|
| `Value` | Last loaded value; `default` until one arrives |
| `Status` | `Idle`, `Loading`, `Loaded`, `Failed` — an `IReadableAtom<LoadStatus>` |
| `IsLoading` | Shorthand for the status being `Loading` |
| `Error` | The exception of the last failure, as a readable atom |
| `Load(load)` | Starts a load, canceling the one in flight |
| `Cancel()` | Cancels without starting another |
| `Subscribe(listener)` | Notified when the value changes |
| `SubscribeToStatus(listener)` | Notified when the status changes |

A failed load is kept as `Failed` + `Error` rather than thrown at the render loop. Canceling keeps the
last value but drops the status back to `Idle`, so a spinner bound to it stops.

## Drawing one

```csharp
private readonly Surface _surface;
private readonly Spinner _spinner = new();
private readonly Table<Mod> _table;

public void Draw()
{
    if (_mods.IsLoading)
    {
        _surface.AppendLine($"{_spinner.Frame()} loading…", Theme.Muted, Align.Center);
        return;
    }

    if (_mods.Status.Value == LoadStatus.Failed)
    {
        _surface.AppendLine(_mods.Error.Value?.Message ?? "", Theme.Error, Align.Center);
        return;
    }

    _table.Rows = _mods.Value ?? [];
    _table.Draw(_surface.Content);
}
```

The [`Spinner`](status-bar.md#spinner) advances on a [`Ticker`](frame-loop.md#work-on-a-clock), which is
what makes it move while nothing else is happening.

## Tying work to the screen

Work outlives the screen that started it unless something stops it. `ViewLifetime` is that something:
it is scoped, so [each screen gets its own](views-and-navigation.md), and navigating away cancels it.

```csharp
public sealed class ModsView : IArlecchinoView
{
    private readonly AsyncAtom<IReadOnlyList<Mod>> _mods;

    public ModsView(ViewLifetime lifetime, ModService service)
    {
        _mods = lifetime.Loading<IReadOnlyList<Mod>>();
        lifetime.Track(_mods.Subscribe(Redraw));

        _mods.Load(token => service.LoadAsync(token));
    }
}
```

| Member | Does |
|---|---|
| `Loading<T>(initial)` | An `AsyncAtom<T>` that is canceled when the screen goes away |
| `Track(resource)` | Disposes a subscription, timer or handle with the screen; returns it back |
| `OnClose(action)` | Runs something as the screen goes |
| `Closing` | The token to pass into work you start yourself; readable after the screen has gone |

The view no longer needs `IDisposable` for any of this. What it does still need it for is anything it
wants to do *before* its scope is released — the view is disposed first, then the scope.

Releasing happens once, over a snapshot of what was registered, so a resource whose own `Dispose`
reaches back into the lifetime does not break the screen it is closing. Anything handed to `Track`
after that point is disposed immediately rather than held by a screen that is already gone.

## Reloading

`Load` cancels whatever is in flight, so a reload command is one line and pressing it twice does not
leave two loads racing to write the same atom:

```csharp
ViewCommand.For(ConsoleKey.R, () => "reload", () => _mods.Load(token => _service.LoadAsync(token)))
```

## Doing it by hand

`AsyncAtom` is a convenience over one rule, not a requirement. Work that does not fit it posts its own
result:

```csharp
private IReadOnlyList<Mod> _rows = [];

Task.Run(async () =>
{
    var loaded = await _service.LoadAsync(lifetime.Closing);
    FrameThread.Post(() => _rows = loaded);
});
```

The rule is the same either way: nothing touches a view, a widget or an atom except the drawing
thread, and `Post` is the door.

## A worked example

`samples/Arlecchino.Processes` is this page as a running program — it reads the process list on a
background thread through an `AsyncAtom`, shows a spinner while it loads, sorts and filters what came
back, and reloads on `r`. See [Showcase](showcase.md).
