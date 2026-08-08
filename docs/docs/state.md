---
title: Application state
sidebar_label: Application state
description: ArlecchinoState — the output line, the modal stack, the pending file-picker request, and the notifications behind the output row.
---

# Application state

`ArlecchinoState` is a singleton holding what outlives a view instance:

| Member | Meaning |
|---|---|
| `Output` | The status line at the bottom of the frame; empty renders as a blank line |
| `Modal` | The modal currently open, or `null` |
| `Modals` | The whole [stack](modals.md#stacking), bottom first |
| `FilePicker` | A pending [file picker](file-picker.md) request |
| `PickerLastFolder` | The folder the picker ended in, so the next request can resume there |
| `Notifications` | The list behind the output row |
| `Invalidate()` | Asks for a repaint without changing anything |

Take it as a constructor parameter in views and commands. Deriving from it is allowed — the class is
not sealed — which is the usual place to hang application state that every view reads. For state that
is really *data*, a [store](stores.md) of atoms is the better home.

## The output line

```csharp
private readonly ArlecchinoState _state;

_state.Output = $"picked: {path}";
```

Rendered on the last row above a rule, styled `Warning` when it carries text. It is drawn only while
`options.ShowOutputLine` is on.

## Notifications

Writing to `Output` raises a **notification**. The row shows the newest one for
`options.NotificationTimeout` (5 seconds by default) and then goes quiet by itself, so a message no
longer sits on screen for the rest of the session. An empty string clears the row at once.

The message outlives the row. It stays in the list for `options.NotificationLifetime` (10 minutes) — or
until `Notifications.Capacity` (200) newer ones have pushed it out, so an application that reports in a
loop cannot grow the list without limit.

The list is a screen of its own: `Ctrl+N`, or a click on the output row, opens `Routes.Notifications` —
newest first, `Information` / `Warning` / `Failure` colored by role, `Backspace` clears it, `Esc` goes
back.

```csharp
_state.Notifications.Notify("could not reach the server", NotificationLevel.Failure);
```

That is the whole of it for a message that fits on a line. Work that takes a while reports its own
progress and then settles into what came of it, and an application that shows its work as a stack of
cards rather than one row reads `Notifications.Recent` instead of `Current` — see
[Diagnostics](diagnostics.md#work-that-takes-a-while).

Both timeouts and the key that opens the screen are set in one call — see
[Hosting and options](hosting-and-options.md):

```csharp
builder.Services.AddArlecchino().UseNotifications(timeout: TimeSpan.FromSeconds(3));
```

`WithoutNotifications()` leaves the row off entirely; the hints box is turned off separately with
`options.ShowHints = false`.

## Asking for a modal

The `Request*` helpers cover the common cases and close the modal for you on submit or cancel:

```csharp
_state.RequestText("Rename", current, validate: null, onSubmit: value => Rename(value));
_state.RequestConfirmation("Delete the profile?", () => Delete(profile));
```

Assigning `State.Modal` directly gives access to every property of a modal type, and `PushModal` opens
one over another. Every kind is on [Modals](modals.md).

## Invalidate

Assigning `Output`, a modal or the file picker asks for a repaint by itself. Anything else that changes
what a view draws says so:

```csharp
_state.Invalidate();
```

[The frame loop](frame-loop.md#frames-are-drawn-on-request) is the full list of who already asks.

## Deriving from it

```csharp
public sealed class AppState : ArlecchinoState
{
    public AppState(Repaint repaint, Notifications notifications) : base(repaint, notifications) { }

    public string ProfileName { get; set; } = "";
}
```

Register the derived type in place of the base one and take `AppState` where you need it. Anything the
framework itself writes — the output line, the modal stack — keeps working, because it is written
against the base class.
