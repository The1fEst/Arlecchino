---
title: Migrating to 2.0
sidebar_label: Migrating to 2.0
description: What 2.0 changed and the edits an application written against 1.x needs.
---

# Migrating to 2.0

Three changes, all of them announced in `1.x` and collected into one release rather than trickled out.
An application that already draws through `Place`, hands background work to a dispatcher and takes the
framework's palette needs a rename and a delete; one that does none of those has a little more to do.

| What changed | What to do |
|---|---|
| `IArlecchinoWidget.Place` is now `Draw`, and the old `void Draw` is gone | Rename `Place` to `Draw` at both ends and return the region left over |
| `UiDispatcher` is gone; posting is `FrameThread.Post` | Delete the field and the constructor parameter, call the static |
| The framework's own colours are the default palette | Nothing, or `UseTheme(ThemePalette.Basic)` to keep the old sixteen |

## Widgets draw and say what is left

`IArlecchinoWidget` has one member now:

```csharp
public interface IArlecchinoWidget
{
    SurfaceRegion Draw(SurfaceRegion region);
}
```

A widget written for `1.x` implemented `void Draw`, or implemented `Place` and inherited a `Draw` that
forwarded to it. Both shapes are gone. Rename `Place` to `Draw`; a widget that still has the old `void
Draw` gains a return value:

```csharp
// 1.x
public void Draw(SurfaceRegion region) => region.WriteLine(0, Label(), Theme.Muted);

// 2.0
public SurfaceRegion Draw(SurfaceRegion region)
{
    region.WriteLine(0, Label(), Theme.Muted);

    return region.Rows(1, region.Height - 1);
}
```

What to return is the same rule the built-in widgets follow: a widget that fills whatever it is given —
a list, a pane, a tree — returns `region.Rows(region.Height, 0)`, an empty region. One that owns a
known number of rows returns the rest, which is what lets a view stack things without counting:

```csharp
private readonly StatusBar _header;
private readonly Tabs _tabs;
private readonly ListBox<Mod> _list;
private readonly Surface surface;

var rest = _header.Draw(surface.Content);
var below = _tabs.Draw(rest);

_list.Draw(below);
```

Callers rename the same way — `_list.Place(region)` becomes `_list.Draw(region)` — and the diagnostic
id `ARL0001`, which existed only to let the deprecation be silenced on its own, is gone with it. A
`#pragma warning disable ARL0001` left behind is harmless but no longer disables anything, so delete
it. See [Widgets](widgets.md).

## Posting work is a static call

`UiDispatcher` is gone. The queue it held lives in [`FrameThread`](frame-loop.md), the type that already
knew which thread draws, so handing a result back is a static call and nothing has to be injected:

```csharp
// 1.x
public sealed class ModsView : IArlecchinoView
{
    private readonly UiDispatcher _dispatcher;
    private readonly ModsService _mods;
    private IReadOnlyList<Mod> _rows = [];

    public ModsView(UiDispatcher dispatcher) => _dispatcher = dispatcher;

    public void Reload() => Task.Run(async () =>
    {
        var loaded = await _mods.LoadAsync();
        _dispatcher.Post(() => _rows = loaded);
    });
}

// 2.0
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

Everything else about it is unchanged: `Post` is safe from any thread, runs actions in the order they
were posted just before the next frame, asks for that frame by itself, and reports an action that
threw on the output line without dropping the rest. Work posted by posted work still belongs to the
next frame.

Two constructors lost the dispatcher they took:

| 1.x | 2.0 |
|---|---|
| `new AsyncAtom<T>(dispatcher, initial)` | `new AsyncAtom<T>(initial)` |
| `new ViewLifetime(dispatcher)` | `new ViewLifetime()` |

A test host or a headless frame that drove the queue by hand calls the statics instead —
`FrameThread.RunPending(onError)`, `FrameThread.HasPending` — and `FrameThread.DiscardPending()` drops
work that will never run because nothing is drawing any more. `ArlecchinoTestHost` calls it as it is
disposed, so one test's leftovers cannot run inside the next.

Resolving `UiDispatcher` from the container fails now, since nothing registers it.

## The default palette is the framework's own

`new ThemePalette()` is the harlequin mask in colours — crimson titles, bone text, ash borders, an ink
cursor row — instead of the terminal's plain sixteen. An application that never called `UseTheme` gets
new colours without changing a line, which is the visual half of this release.

Two ways back, depending on what you want:

```csharp
builder.UseTheme(ThemePalette.Basic);                     // the 1.x defaults, unchanged
builder.UseTheme(new ThemePalette { Header = ... });      // your own, on top of the new defaults
```

`ThemePalette.Basic` is exactly what `new ThemePalette()` used to be: bright magenta titles, bright
blue column headers, cyan borders, black on green for the cursor row, and no exact colours behind any
of them. A palette of your own is still partial — what it does not override now comes from the
framework's colours rather than the terminal's. See [Theming](theming.md).

`UseTheme(ThemePalette.Arlecchino)` written against `1.x` still compiles and still means the same
thing; it is only redundant now.

## Checklist

1. Rename every `Place` to `Draw`, in widgets and in the views that call them; give any remaining
   `void Draw` a `SurfaceRegion` to return.
2. Delete `UiDispatcher` fields, constructor parameters and registrations; call `FrameThread.Post`.
3. Drop the dispatcher argument from `new AsyncAtom<T>(...)` and `new ViewLifetime(...)`.
4. Decide about colour: nothing to do to take the new palette, `UseTheme(ThemePalette.Basic)` to keep
   the old one.
5. Delete `#pragma warning disable ARL0001` and any `NoWarn` carrying it.

Nothing else in the surface moved — the full list is in the [changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#200).
