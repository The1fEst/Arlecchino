---
title: Testing
sidebar_label: Testing
description: ArlecchinoTestHost, FakeTerminal and FrameText — driving an application headlessly and asserting on the frame it drew.
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
| `Frame()` | Draws a frame and returns it as plain text, styling stripped |
| `FrameLines()` | The same, as rows |
| `FrameContains(text)` | Whether a frame holds some text anywhere — text split across rows will not be found |
| `FrameLineContaining(text)` | The first row holding some text, which is how a test reads what was drawn beside a label |
| `Styles()` | The colour sequences in the frame, in order |

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
