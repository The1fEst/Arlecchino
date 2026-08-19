---
title: ANSI and the terminal
sidebar_label: ANSI and the terminal
description: What the framework asks a terminal for and how it reads the answers — escape sequences in, escape sequences out, the alternate screen, and IArlecchinoTerminal.
---

# ANSI and the terminal

Everything between Arlecchino and the console is escape sequences. This page is what goes out, what
comes in, and where the seam is if you want to put something else there.

## What goes out

One frame is one call to `IArlecchinoTerminal.Write`. Inside it are cursor jumps and SGR sequences,
emitted only where the style changes — see [Rendering](rendering.md#what-a-frame-costs). Around the
frames sit a handful of mode switches:

| Sequence | When | Member |
|---|---|---|
| Alternate screen, cursor hidden | The application starts | `EnterFullScreen()` |
| Normal screen, cursor shown | It stops, including on a crash | `LeaveFullScreen()` |
| `?1000` `?1002` `?1006` — SGR mouse reporting | `UseMouse()` is on | `EnableMouse()` / `DisableMouse()` |
| `?2004` — bracketed paste | On by default | `EnablePaste()` / `DisablePaste()` |
| OSC 52 | `Ctrl+Insert` in a field | `CopyToClipboard(text)` |
| Control keys taken and given back | The terminal is taken and handed back | `TakeControlKeys()` / `GiveBackControlKeys()` |

OSC 52 is the reason a copy works over SSH: the sequence carries the text to whatever is *showing* the
terminal, so it lands on the clipboard of the machine the user is sitting at. Terminals may refuse it
and none acknowledge it, so there is nothing to report back — which is why the text also goes down the
standard input of the first clipboard program the machine has: `pbcopy`, `termux-clipboard-set`,
`wl-copy`, `xclip`, `xsel`, tried in that order, and only on Linux, macOS and the BSDs. A program that
is not installed fails to start and costs nothing.

`TakeControlKeys` is what lets an application see `Ctrl+C` as a key rather than as a signal, and it
matters on Windows, where the console decides for itself whether `Ctrl+C` and `Ctrl+Shift+C` are the
same thing. Both members default to doing nothing, so a terminal of your own needs neither until it has
something to say about them; the keys go back to the console the moment the terminal is lent to another
program.

When [color support](colors.md#what-the-terminal-can-actually-do) is `None`, no style sequence is
emitted at all — not even the per-line reset — and the alternate screen is left alone. An application
in that state prints plain text instead of spraying escape codes at a console that cannot read them.

## What comes in

Arrows, function keys and mouse reports arrive as escape sequences, so an escape has to be read
together with whatever follows it. `TerminalInputReader` is what does that:

1. It collects the sequence.
2. Mouse reports go to the escape-sequence parser inside the package and come back as
   [`MouseEvent`](mouse.md).
3. Cursor and function keys it decodes itself.
4. Anything it does not recognize is replayed key by key — which is what makes a plain `Escape` work
   even though it starts the same way.

| Member | Meaning |
|---|---|
| `Read(key)` | Handles one key press, reading further keys itself when it looks like the start of a sequence |
| `ReadPending()` | Reads everything waiting and returns without blocking; drains mouse events too |

## The escape timeout

The rest of a sequence does not always arrive with its escape. Over ssh or a loaded terminal an arrow
can land a few milliseconds later, and reading only what is already buffered turns it into `Esc`, `[`,
`A`. So the reader waits `options.EscapeTimeout` (25 ms) for the continuation.

That wait is also what a lone `Esc` costs before it is delivered, which is the trade every terminal
editor makes:

```csharp
options.EscapeTimeout = TimeSpan.FromMilliseconds(10);   // local terminal
options.EscapeTimeout = TimeSpan.FromMilliseconds(60);   // slow link
```

## Two ways a mouse arrives

`MouseAvailable` and `ReadMouse()` exist for terminals that deliver the mouse **outside** the key
stream. In practice that is the Windows console, which reads `ReadConsoleInput` records instead of SGR
reports; everywhere else mouse events are escape sequences among the keys and `MouseAvailable` stays
`false`. [Mouse](mouse.md#why-windows-is-different) has the detail.

## IArlecchinoTerminal

The whole seam is one interface:

| Member | Meaning |
|---|---|
| `Width` / `Height` | Size of the window |
| `KeyAvailable` / `ReadKey()` | The key stream |
| `MouseAvailable` / `ReadMouse()` | The out-of-band mouse stream, where there is one |
| `Write(text)` | Composed output; a frame arrives as one call |
| `EnterFullScreen()` / `LeaveFullScreen()` | The alternate screen |
| `EnableMouse()` / `DisableMouse()` | Mouse reporting |
| `EnablePaste()` / `DisablePaste()` | Bracketed paste markers |
| `CopyToClipboard(text)` | OSC 52 |

`SystemTerminal` is the real one. [`FakeTerminal`](testing.md) is the other implementation that ships,
and `.UseTerminal<T>()` is how a third goes in — a remote session, a recording harness, a pipe.

## Windows

`SystemTerminal` turns on `ENABLE_VIRTUAL_TERMINAL_PROCESSING` as it starts. If the console refuses —
an old `conhost` — color drops to `None`, the alternate screen is not entered, and the application
degrades to plain text.

Turning on virtual-terminal *input* is a different flag, and enabling it stops `Console.ReadKey` from
delivering keys at all. That is why the Windows mouse path reads the console event queue rather than
asking for SGR reports.
