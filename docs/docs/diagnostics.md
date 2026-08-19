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

A console logger cannot work here — a line written to standard output lands on the frame and scrolls it
away. `AddArlecchino` therefore stands in front of standard output and standard error: while a frame is
on the screen, text written there is caught into a `LogBuffer` in memory instead, and `Ctrl+L` — the
`ToggleLog` [binding](keyboard.md#the-keymap) — shows it over the bottom half of the screen:

```
╭─ Log (7) ───────────────────────────────────────────╮
│ 14:02:11 fail Screen: The view at route Mods failed │
│ 14:02:11 warn CommandConflicts: Ctrl+S is claimed…  │
│ ↑↓ scroll · End latest · Backspace clear · Esc close │
╰─────────────────────────────────────────────────────╯
```

Warnings and errors are colored by [role](theming.md#roles), the newest line is at the bottom, and
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

### Where the lines come from

Arlecchino registers no logging provider of its own. What the overlay draws is what a provider writes
to the console, which the default host already does — `builder.Logging` keeps its console provider, and
`ILogger` reaches the overlay through it rather than around it.

```csharp
builder.Logging.ClearProviders();   // now the way to end up with nothing in the overlay at all
```

That line used to be how a console provider was kept off the frame. It no longer is: the console is
caught, so a provider writing to it is exactly how a line arrives. An application that has cleared every
provider is told so in the overlay — `ArlecchinoStrings.LogWithoutProviders` is what it says — rather
than left looking at a panel that stays empty whatever happens.

A line caught off standard output is logged under `stdout`, one off standard error under `stderr`, with
escape sequences taken out of it so nothing drawn into the overlay can move the frame around. Text
written before the terminal is taken or after it is given back goes to the console as it always did:
`--help`, a failure during startup, and the host's own shutdown lines all still print.

### What ends up in it

| Written by | When |
|---|---|
| `Screen` | A view threw while drawing — one line on the [output row](state.md#the-output-line), the whole story here |
| `Screen` | Posted work threw before a frame |
| `Screen` | A collection shrank mid-frame and cut a widget short |
| `CommandConflicts` | A view command shadows an application command, or binds the same key twice |
| Your application | Anything you log |
| Anything at all | A stray `Console.WriteLine`, from your code or from a library that never knew it was in a terminal application |

## Notifications

The [output line](state.md#notifications) is one row and clears itself after a few seconds. What was
said stays in `Notifications` for much longer, so opening the screen still shows what went past while
the user was looking elsewhere.

```csharp
private readonly ArlecchinoState _state;

_state.Notifications.Notify("could not reach the server", NotificationLevel.Failure);
```

| Member | Meaning |
|---|---|
| `Notify(text, level)` | Says something; the newest line replaces whatever the output row was showing |
| `Raise(entry)` | Says something that carries more than a line, and hands the entry back to keep |
| `Settle(entry, text, level)` | Turns a line that was reporting work into what came of it, in place |
| `Withdraw(entry)` | Takes one entry back, for work whose line is not worth keeping at all |
| `Entries` | Everything still held, newest first |
| `Current` | The line the output row shows, or `null` once it has timed out |
| `Recent` | Everything worth showing right now: still running, or ended recently |
| `Clear()` | Throws everything away, the output row included — except work still running |
| `Capacity` | How many are kept; 200 by default |

`NotificationLevel` is `Information`, `Warning` or `Failure`, and decides the color. `Ctrl+N`, or a
click on the output row, opens `Routes.Notifications`: newest first, `Enter` opens the entry in full,
`Backspace` clears, `Esc` goes back.

Both timeouts are counted by the [`Ticker`](frame-loop.md#work-on-a-clock) — nothing here runs on a
thread of its own.

### Work that takes a while

A copy of four hundred files is not one message. Build the entry yourself and give it a
`ProgressText`, which is read every frame, and a `Progress` between `0` and `1` for the bar drawn
beside it:

```csharp
var entry = _state.Notifications.Raise(
    new(DateTimeOffset.Now, NotificationLevel.Information, "Copying")
    {
        ProgressText = () => $"Copying {copy.Done} of {copy.Total}",
        Progress = () => copy.Done / (double)copy.Total,
        Detail = () => string.Join('\n', copy.Failures),
        Actions = [new(() => "Stop", copy.Cancel)],
    });
```

While `ProgressText` is set and the work has not been settled the entry `IsRunning`: it stays on the output
row past `NotificationTimeout`, is never expired by `NotificationLifetime`, and survives `Clear()` — a
copy does not stop because its line was cleared.

`Settle` ends it in place. The entry keeps its spot and its identity, so a dialog someone already has
open turns from "copying" into what was copied rather than going stale, and it starts aging from the
moment it finished rather than the moment it began:

```csharp
_state.Notifications.Settle(entry, $"Copied {copy.Done} files", NotificationLevel.Information);
```

`Detail` is the whole story shown when the entry is opened — the errors a copy collected, the output of
a command — and `Actions` are `NotificationAction(Label, Run)` offers made alongside it. Settling
clears the actions, since stopping something that is over is not an offer worth making.

### Showing more than the newest line

`Current` answers what one row at the bottom of the screen should say, and one row can only hold the
newest. An application that shows its work as a stack of cards in the corner wants all of it, and wants
a copy that is still going to stay up however long it takes. That is `Recent`, newest first: everything
still running whatever its age, plus everything that ended within `NotificationTimeout`.

```csharp
foreach (var entry in _state.Notifications.Recent)
{
    card.WriteLine(0, entry.Line, entry.Level switch
    {
        NotificationLevel.Failure => Theme.Error,
        NotificationLevel.Warning => Theme.Warning,
        _ => Theme.Default,
    });
}
```

`Line` is the single line to draw — what came of the work, what is happening now, or what was said, in
that order — `Level` is how loud it is now, which is what it was raised as until the work it reports
ends as something else, and `Fraction()` is how full a bar for it should be, or `null` when there is
nothing to draw.

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
version: 2026.8.1
runtime: .NET 10.0.10
platform: Microsoft Windows 10.0.26200 (X64)

[Terminal]
implementation: SystemTerminal
size: 120×34
frame: 116×32
color: TrueColor
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
    .AddArlecchino(options => options.Hints = HintsShown.Never)
    .WithoutNotifications();
```

— which leaves the bottom row to the screen's own [status bar](status-bar.md).
