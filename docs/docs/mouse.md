---
title: Mouse
sidebar_label: Mouse
description: Turning mouse reporting on, what a MouseEvent carries, how a view hit-tests it, and why Windows reads the console queue instead.
---

# Mouse

Mouse reporting is off until you ask for it:

```csharp
builder.Services.AddArlecchino().UseMouse();   // or options.MouseInput = true
```

The hosted service then turns reporting on while it runs and off on the way out — button presses,
releases, drags and the wheel.

## MouseEvent

| Member | Meaning |
|---|---|
| `Action` | `Pressed`, `Released`, `Moved` (drag), `ScrolledUp`, `ScrolledDown` |
| `Button` | `Left`, `Middle`, `Right`, or `None` for the wheel |
| `Row`, `Column` | Zero-based cell in the frame — the same coordinates `Surface.WriteAt` takes |
| `Modifiers` | Shift, Alt, Control held at the time |
| `IsScroll` | Whether this is a wheel event |
| `IsLeftClick` | Shorthand for a left button press |

## Handling one

A view opts in by implementing one method, and navigates by returning a route just as `Handle` does:

```csharp
public ViewRoute HandleMouse(MouseEvent mouse)
{
    if (mouse.IsScroll)
    {
        _offset += mouse.Action == MouseAction.ScrolledDown ? 1 : -1;
        return ViewRoute.None;
    }

    return mouse.IsLeftClick && mouse.Row == _runRow ? ViewKind.Run : ViewRoute.None;
}
```

Because `Row` and `Column` are frame cells, a view that draws with absolute coordinates already knows
where its rows are — hit-testing is comparing numbers.

## Hit-testing with regions

A view built from [regions](layout.md#regions) does not compare numbers at all. The same region that
positioned a pane answers whether a click landed in it:

```csharp
public ViewRoute HandleMouse(MouseEvent mouse)
{
    if (!_list.Contains(mouse.Row, mouse.Column))
    {
        return ViewRoute.None;
    }

    var (row, _) = _list.ToLocal(mouse.Row, mouse.Column);
    _index = _first + row;
    return ViewRoute.None;
}
```

A view with several panes should reach for a [focus ring](focus.md) instead: it offers the event to
each element and moves the focus to whichever one claims it, so a click both selects a pane and acts
inside it.

## While a modal is open

The wheel scrolls a list or choice modal; other events are swallowed rather than reaching the view
behind it. The modals that have clickable parts — the slider track, the toggle chips, the colour
channels, the palette rows — publish the [regions](layout.md#regions) they were drawn into as they are
drawn, which is what makes them clickable at all.

## Why Windows is different

How mouse reporting is done differs by platform, and only `SystemTerminal` knows the difference.

Everywhere but Windows it is SGR reporting (`?1000`, `?1002`, `?1006`) mixed into the key stream. On
Windows the console cannot do that: turning on virtual-terminal *input* is what delivers SGR reports,
and with that flag `Console.ReadKey` stops delivering keys at all. So Windows reads the console's own
event queue instead — `ReadConsoleInput` with `ENABLE_MOUSE_INPUT`, keys and mouse records out of the
same stream, translated into the same `MouseEvent`.

Quick-edit mode is switched off while it runs, otherwise the console swallows clicks as text
selection, and the previous mode is put back when the mouse is turned off.

That is the one place `IArlecchinoTerminal.MouseAvailable` and `ReadMouse()` matter: they exist for
terminals that deliver the mouse outside the key stream. `TerminalInputReader.ReadPending()` drains
both.

:::note

Mouse reporting has not been exercised inside a multiplexer. If it misbehaves there,
[an issue](https://github.com/The1fEst/Arlecchino/issues) with the terminal and `TERM` is useful.

:::
