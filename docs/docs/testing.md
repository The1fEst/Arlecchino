---
title: Testing
sidebar_label: Testing
description: ArlecchinoTestHost, ScreenGrid, FakeTerminal and FrameText — driving an application headlessly and asserting on the screen it left.
---

# Testing

```bash
dotnet add package Arlecchino.Testing
```

A terminal application is testable in a way a GUI rarely is: what it drew is a string.

```csharp
using var app = new ArlecchinoTestHost(80, 24,
    builder => builder.AddGeneratedViews().AddGeneratedStores().StartAt(ViewKind.Mods));

app.Press(ConsoleKey.DownArrow);
app.Type("wide");
app.Press(ConsoleKey.Enter);

Assert.Contains("Widebody kit", app.Frame());
```

## ArlecchinoTestHost

It builds the container exactly as `AddArlecchino` would, minus the hosted service, and draws into a
`FakeTerminal`. Nothing touches a real console, so the tests run anywhere.

### Driving it

| Member | Use |
|---|---|
| `Press(key, shift, alt, control)` | Presses a key, routed exactly as a real one would be |
| `Type(text)` | Types characters one at a time, carrying a character but no key — what a terminal reports for ordinary typing |
| `Click(row, column, button)` | Clicks a cell, in the terminal's own coordinates |
| `Scroll(row, column, down)` | Turns the wheel over a cell |
| `ReadFromTerminal(sequence)` | Feeds raw characters through the reader that recognises escape sequences — the way to test what a real terminal sends for arrows, function keys and mouse reports |
| `DrainInput()` | Routes whatever the reader queued; `ReadFromTerminal` and `Frame` do it for you |
| `Advance(span)` | Moves the clock forward and runs whatever fell due, exactly as the frame loop would |

### Reading what came out

| Member | Use |
|---|---|
| `Frame()` | Draws a frame and returns **what is on screen** afterwards, styling stripped |
| `FrameLines()` | The same, as rows |
| `Screen` | The screen itself, as a [`ScreenGrid`](#screengrid) — cells, styles and the cursor |
| `FrameContains(text)` | Whether a frame holds some text anywhere — text split across rows will not be found |
| `FrameLineContaining(text)` | The first row holding some text, which is how a test reads what was drawn beside a label |
| `Styles()` | The colour sequences in the frame, in order |

:::note[What a frame writes and what it leaves are not the same thing]

Frames are written as [the difference from the last one](rendering.md#what-a-frame-costs): an idle
frame writes nothing at all, and a frame that changed one cell writes one cell. `Frame()` draws that
way — the path a running application takes — and then returns the **screen**, so an assertion reads
the whole picture however little of it was written.

`Styles()` is the exception: it draws a whole frame on purpose, because a diffed frame only restates
the colours of the cells it rewrote.

:::

### Reaching inside

`State`, `Navigator`, `Surface`, `Options`, `History`, `Repaint`, `Clock`, `Terminal` and `Services`
are all exposed, for a test that wants to open a dialog, force a route, resize mid-test or assert that
something actually asked for a frame.

:::note[Colour is pinned]

Colour is pinned to `ColorSupport.TrueColor` as the host is built, so a build agent that sets
`NO_COLOR` does not quietly strip the styling a test asserts on. Set `TerminalCapabilities.Color`
after building to test another level — and remember it is
[process-wide](theming.md#swapping-the-palette).

:::

## ScreenGrid

The screen a terminal would be holding, rather than the bytes that got it there: a grid that output is
applied to, obeying the escapes instead of stripping them. A cursor jump moves the cursor, a style
sticks to the cells that follow, a wide symbol takes two columns.

```csharp
app.Press(ConsoleKey.DownArrow);
app.Frame();

Assert.Equal("Widebody kit", app.Screen.Line(3).Trim());
Assert.Equal(Theme.Selected.Ansi, app.Screen.StyleAt(3, 2));
```

| Member | Use |
|---|---|
| `Line(row)` / `Lines()` | A row, or every row, as it reads |
| `CellAt(row, column)` | The symbol on a cell — empty for the second half of a wide one |
| `StyleAt(row, column)` | The style in force on a cell; compare against `TermColor.Ansi` |
| `CursorRow` / `CursorColumn` | Where the cursor was left |
| `IsCursorVisible` | Whether it was left showing |
| `Matches(other)` | Whether two screens hold the same symbols in the same styles |
| `Apply(output)` | Applies more output; `Resize(width, height)` keeps what fits |

A symbol written into the last column while wrapping is on leaves `CursorColumn` one past the right
edge, waiting to wrap — which is what a terminal reports too.

:::tip[The screen is held against the frame on every draw]

Every frame built against a `FakeTerminal` is compared, cell by cell, with what the surface composed:
same symbol, same style. A difference means the frame written left something on screen that the frame
drawn did not have, and the draw throws with both pictures in the message.

It costs a pass over the cells and no second render, and it applies to a bare `Surface` over a
`FakeTerminal` as much as to the host — so a widget of your own that draws outside its region, or
draws differently the second time, is caught by whichever test happens to draw it twice.

:::

## Time without waiting

`Advance` moves the [ticker](frame-loop.md#work-on-a-clock)'s clock and runs what fell due, so a
five-second timeout costs nothing:

```csharp
app.State.Output = "saved";
Assert.Contains("saved", app.Frame());

app.Advance(TimeSpan.FromSeconds(6));
Assert.DoesNotContain("saved", app.Frame());
```

`Advance` does not draw. Ask for a frame afterwards.

## FakeTerminal

The terminal underneath records what an application asked of it, which is how the chrome is tested:

| Member | Asserts |
|---|---|
| `Written` | Everything written so far, escape sequences included |
| `Clear()` | Throws away what has been written, so the next assertion sees one frame rather than all of them |
| `IsFullScreen` | Whether the application took over the screen and gave it back |
| `IsMouseEnabled` / `IsPasteEnabled` | Whether it asked for the mouse and for bracketed paste |
| `Copied` | The last text copied |
| `Width` / `Height` | Assigning simulates a resize |
| `Enqueue`, `EnqueueText`, `EnqueueMouse` | Queue input the way a real terminal delivers it |
| `Screen` | What is on screen, surviving `Clear()` the way a real screen survives forgetting what you typed |

`EnqueueText` names the key where a console names it: Enter, Tab, Backspace, the space bar, a letter,
a digit and a control chord all arrive carrying their `ConsoleKey`, because that is what
`Console.ReadKey` hands an application. Escape sequences still arrive a character at a time, which is
the other shape a console produces — the one the reader has to make sense of on its own.

```csharp
app.Terminal.Width = 40;
Assert.Contains("terminal is too small", app.Frame());
```

## FrameText

The helper behind the frame assertions, public for tests that do their own:

| Member | Use |
|---|---|
| `WithoutStyles(text)` | Strips the escape sequences |
| `Lines(text)` | Splits a frame into rows |
| `StylesIn(text)` | The colour sequences, in order |
| `CursorJumpsIn(text)` | The cursor moves, for asserting on the differential write |
| `BoxWidth(lines)` | Checks that a box is rectangular |

## SessionTape

A tape is a session written down: every event that goes in, how long the application waits for it,
and where a frame is worth looking at. It is for writing a test as the session it describes, rather
than as a dozen calls with the assertions lost among them.

```csharp
var frames = new SessionTape()
    .Type(":")
    .Shot()
    .Type("copy")
    .Wait(200)
    .Shot()
    .Play(host);

Assert.Contains("Copy files", frames[^1], StringComparison.Ordinal);
```

`Play` returns one frame per `Shot`, in order. Playing a tape draws the same frames every time: a
screen here is a function of state, state only changes on an event, and the time comes from a
provider rather than from the clock on the wall — so `Wait` moves the clock instead of sleeping.

| Step | What it does |
|---|---|
| `Key(key, shift, alt, control)` | One key press |
| `Type(text)` | Each character as its own key |
| `Click(row, column, button)` / `Scroll(row, column, down)` | Mouse |
| `Paste(text)` | A block arriving through bracketed paste |
| `Wait(milliseconds)` | Moves the clock forward |
| `Shot()` | Marks a frame worth keeping |

A tape holds what the terminal reported rather than what it meant, so it replays the same whatever
the keyboard layout, and it holds no application state at all — only what was done to it. `ToString`
writes it out and `Read` takes it back, so a tape travels as a file.

:::caution[Not for recording a real session]

`RecordKey` and `RecordMouse` exist for a harness building a tape from events it already has. Do not
point them at a running application: the framework has a password modal and a paste step, so a tape
captured that way would hold whatever a person typed into them.

:::

## What a test is usually about

| Question | How |
|---|---|
| Did the screen draw what it should? | `Assert.Contains(…, app.Frame())` |
| Did a key do the right thing? | `app.Press(…)` then read the frame |
| Did navigation go where it should? | `app.Navigator.CurrentRoute` |
| Did a modal open with the right title? | `app.State.Modal` |
| Was the value undoable? | `app.History.Undo()` then read the store |
| Did a colour survive? | `app.Styles()` |
| Did a resize reflow the layout? | `app.Terminal.Width = …` |

## Testing the framework itself

`dotnet test tests/Arlecchino.Tests` runs the suite on `Arlecchino.Testing` — the same package
applications use, so it is exercised by every run — and it runs twice, once per target framework:

```bash
dotnet test tests/Arlecchino.Tests -f net8.0
```

In this repository `ProbeView` / `OtherView` also keep the [source generator](source-generator.md)
under test: the routes they produce are used by the navigation tests.

### Held against a real terminal

`ScreenGrid` and the code that writes the frames were written by the same head, so a wrong idea about
the edge of a row or the width of a symbol would be held by both, cancel out, and leave every test
green with the picture wrong. The repository settles that from outside: frames are played into a real
`tmux` pane and the screen it ends up with is compared against the screen `ScreenGrid` ended up with —
symbols, the colour of every cell, and where the cursor was left — and keys are pressed in a pane so
the bytes a terminal really sends are the ones the reader is asked to make sense of. Both found real
defects; `CONTRIBUTING.md` says how to run them.

Terminals do not always agree with each other either. A wide symbol written into the last column with
wrapping off is dropped by tmux and shifted a column inwards by kitty, and the emulator can only follow
one of them — so where that is the case, the code says which and why nothing turns on it.

### Two corners tested differently

The real terminal is tested by replacing `Console.Out` and asserting on the bytes that come out of
it — the alternate screen, bracketed paste, mouse reporting, `OSC 52` copying — because those
sequences are the whole behaviour, and a fake terminal never emits them. The parts that only exist on
one platform are asserted per platform: away from Windows the mouse is asked for with escape
sequences, on Windows nothing reaches the output at all, since the console is read record by record
instead.

One corner is deliberately left uncovered: the P/Invoke half of `WindowsConsoleInput`, which asks the
Windows console for its mode and reads input records out of its queue. Nothing but a real console on a
real Windows session executes it, and a mock of `kernel32` would only assert that the mock was called.
What it hands over — turning a console record into a key or a mouse event — is a type of its own that
the suite drives on either platform, so the untested part is the syscalls themselves.

## Looking at a frame by eye

Tests assert; sometimes you want to look. Every sample renders a single frame headlessly:

```bash
dotnet run --project samples/Arlecchino.Sample -- --frame picker 130x30
```

[Rendering](rendering.md#rendering-without-a-terminal) is how that works, and
[Hosting and options](hosting-and-options.md#running-without-the-hosted-service) is the wiring.
