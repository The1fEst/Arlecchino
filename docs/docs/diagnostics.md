---
title: Diagnostics
sidebar_label: Diagnostics
description: The log overlay, the notifications screen, and the report worth attaching to a bug — how a terminal application says something went wrong without writing into the frame.
---

# Diagnostics

A terminal application cannot print. Anything written to standard output lands in the middle of a
frame, so every way this framework has of saying something goes somewhere the renderer controls: a
buffer behind an overlay, a list behind the output row, or the clipboard.

## The log overlay

A console logger cannot work here. `AddArlecchino` therefore registers a logger provider of its own,
which keeps the last lines in a `LogBuffer` in memory, and `Ctrl+L` — the `ToggleLog`
[binding](keyboard.md#the-keymap) — shows them over the bottom half of the screen:

```
╭─ Log (7) ───────────────────────────────────────────╮
│ 14:02:11 fail Screen: The view at route Mods failed │
│ 14:02:11 warn CommandConflicts: Ctrl+S is claimed…  │
│ ↑↓ scroll · End latest · Backspace clear · Esc close │
╰─────────────────────────────────────────────────────╯
```

Warnings and errors are coloured by [role](theming.md#roles), the newest line is at the bottom, and
`↑`/`↓` scroll back through the buffer while `End` pins it to the newest again. Only those keys are
taken while the overlay is open, so the screen underneath keeps working — it is something to read, not
a mode to get stuck in.

`LogBuffer.Capacity` sets how much is kept; 200 lines by default. Oldest lines are dropped once it is
full.

:::note[Logging happens off the drawing thread]

A log line is written by whatever thread did the work, so the buffer is a concurrent queue and the
overlay draws from a snapshot rather than from the live collection. Trimming is done under a lock,
because the check and the removal have to be one step or two threads trimming at once take the buffer
below its capacity.

:::

### Adding providers of your own

`AddArlecchino` calls `AddLogging()`, so `ILogger` is always resolvable and a file or Seq provider goes
in the usual way. Drop any provider that writes to standard output.

```csharp
builder.Logging.ClearProviders();   // removes this one too, overlay included
```

### What ends up in it

| Written by | When |
|---|---|
| `Screen` | A view threw while drawing — one line on the [output row](state.md#the-output-line), the whole story here |
| `Screen` | Posted work threw before a frame |
| `Screen` | A collection shrank mid-frame and cut a widget short |
| `CommandConflicts` | A view command shadows an application command, or binds the same key twice |
| Your application | Anything you log |

## Notifications

The [output line](state.md#notifications) is one row and clears itself after a few seconds. What was
said stays in `Notifications` for much longer, so opening the screen still shows what went past while
the user was looking elsewhere.

```csharp
_state.Notifications.Notify("could not reach the server", NotificationLevel.Failure);
```

| Member | Meaning |
|---|---|
| `Notify(text, level)` | Says something; the newest line replaces whatever the output row was showing |
| `Clear()` | Throws everything away, the output row included |
| `Capacity` | How many are kept; 200 by default |

`NotificationLevel` is `Information`, `Warning` or `Failure`, and decides the colour. `Ctrl+N`, or a
click on the output row, opens `Routes.Notifications`: newest first, `Backspace` clears, `Esc` goes
back.

Both timeouts are counted by the [`Ticker`](frame-loop.md#work-on-a-clock) — nothing here runs on a
thread of its own.

## A report to attach to a bug

The overlay says what happened; `ArlecchinoReport` says *where* it happened. Resolve it and call
`Describe()`:

```csharp
public sealed class ReportCommand : IArlecchinoCommand
{
    private readonly ArlecchinoReport _report;
    private readonly IArlecchinoTerminal _terminal;

    public ReportCommand(ArlecchinoReport report, IArlecchinoTerminal terminal)
    {
        _report = report;
        _terminal = terminal;
    }

    public KeyBinding Binding => new(ConsoleKey.F12);

    public string Icon => "";

    public string Label => "Copy diagnostics";

    public ViewRoute Execute()
    {
        _terminal.CopyToClipboard(_report.Describe());
        return ViewRoute.None;
    }
}
```

What comes out is a page of `key: value` lines under four headings — the framework version and
runtime, what the terminal said it can do, the screen being shown, and the modals above it:

```
[Arlecchino]
version: 2.0.0
runtime: .NET 10.0.10
platform: Microsoft Windows 10.0.26200 (X64)

[Terminal]
implementation: SystemTerminal
size: 120×34
frame: 116×32
colour: TrueColor
TERM: xterm-256color
COLORTERM: unset
NO_COLOR: unset
WT_SESSION: unset
redirected: in False, out False
```

Because it goes through [`CopyToClipboard`](ansi.md#what-goes-out) it reaches the clipboard of the
machine the user is sitting at even over SSH, which is the point: a report a user can paste is worth
more than one they have to describe.

## Failures that do not stop the application

| Failure | What happens |
|---|---|
| A view throws in `Draw` | Logged, reported on the output line, the rest of the frame still composed |
| Posted work throws | Logged and reported; the remaining posted actions still run |
| Scheduled work throws | Logged and reported; the rest of the schedule still runs |
| A widget's collection shrinks mid-frame | The frame ends early and a warning names the route |

None of them take the process down. A dead process is harder to recover from than a broken screen, and
harder to get a report out of.

## Turning diagnostics off

There is nothing to turn off: the overlay costs a bounded in-memory buffer, and the notifications list
is capped. What can be turned off is the chrome —

```csharp
builder.Services
    .AddArlecchino(options => options.ShowHints = false)
    .WithoutNotifications();
```

— which leaves the bottom row to the screen's own [status bar](status-bar.md).
