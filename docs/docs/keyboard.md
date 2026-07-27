---
title: Keyboard
sidebar_label: Keyboard
description: How a key travels from the terminal to a view, the keymap every framework key lives in, the keys screen, keyboard layouts, and paste and copy.
---

# Keyboard

## How a key travels

A reader thread polls `IArlecchinoTerminal` every `InputPollInterval` (8 ms by default). What it reads
is queued, and the [frame loop](frame-loop.md) drains the queue on the drawing thread before composing
the next frame, handing each event to `InputRouter`.

The router resolves a key in this order:

1. **An open modal.** The key goes to the modal and nothing else sees it.
2. **The command-palette key** (`:` by default), when at least one command is registered.
3. **History keys** — `Alt+←` / `Alt+→`.
4. **Commands of the current view** — see [Commands](commands.md#commands-of-a-view).
5. **Application commands with a modifier.**
6. **`IArlecchinoView.Handle`** — everything else: typing, arrows, list filters.

So a view never has to check for the palette key or guard against typing into a modal, and a key that
belongs to a command never reaches `Handle`.

## KeyBinding

`KeyBinding` matches the key *and* the exact modifiers, so `Ctrl+S` never fires on a bare `S`:

```csharp
new KeyBinding(ConsoleKey.S, ConsoleModifiers.Control)
```

Its `ToString()` is what the palette, the hints box and the file-picker legend display — `Ctrl+S`,
`Alt+←`, `Esc` — so a remapped key relabels itself everywhere it is shown.

A binding can carry a second combination for actions the platforms disagree about. That is what
`AlsoKey` and `AlsoModifiers` are for, and why `Copy` answers to both `Ctrl+Insert` and `Ctrl+Shift+C`.
`ToString()` shows the first one, so hints stay short.

## The keymap

Every key the framework itself reacts to is a `KeyBinding` on `ArlecchinoKeymap`, not a constant buried
in the router:

| Action | Default | Used by |
|---|---|---|
| `Back` / `Forward` | `Alt+←` / `Alt+→` | History |
| `Confirm` / `Cancel` | `Enter` / `Esc` | Every modal, the file picker |
| `NextField` / `PreviousField` | `Tab` / `Shift+Tab` | [Focus rings](focus.md), segments, colour channels, picker panes |
| `MoveUp` / `MoveDown` / `MoveLeft` / `MoveRight` | arrows | Lists, sliders, number steps, segments |
| `JumpUp` / `JumpDown` | `PgUp` / `PgDn` | Large steps and page moves |
| `First` / `Last` | `Home` / `End` | Ends of a slider, channel or list |
| `Erase` | `Backspace` | Text, filters, typed segments |
| `DeleteForward` | `Delete` | Text fields |
| `EraseWord` / `EraseToStart` | `Ctrl+Backspace` / `Ctrl+U` | Text fields |
| `WordLeft` / `WordRight` | `Ctrl+←` / `Ctrl+→` | Text fields |
| `Copy` | `Ctrl+Insert` or `Ctrl+Shift+C` | Text fields and the multi-line dialog |
| `Submit` | `Ctrl+Enter` | Confirms the [multi-line text dialog](modals.md), where `Enter` breaks the line |
| `ToggleLog` | `Ctrl+L` | The [log overlay](diagnostics.md) |
| `Notifications` | `Ctrl+N` | The [notifications screen](diagnostics.md) |
| `Help` | `F1` | The keys screen below |
| `Mark` | `Space` | Multi-choice, toggle |
| `PickCurrentFolder` | `Ctrl+Enter` | [File picker](file-picker.md) |

```csharp
builder.Services
    .AddArlecchino()
    .UseKeymap(new ArlecchinoKeymap
    {
        Back = new KeyBinding(ConsoleKey.Backspace),
        Cancel = new KeyBinding(ConsoleKey.Q, ConsoleModifiers.Control),
    });
```

The command-palette key stays a character (`options.CommandPaletteKey`) rather than a binding: it is
resolved through `KeyText`, so it keeps working on a layout where `:` sits somewhere else.

## The keys screen

The hints box has room for a handful of keys and the palette lists commands only, so there is a screen
that lists everything: `F1` — the `Help` binding — opens `Routes.Help`. It shows

1. every key the framework answers to, with what it does;
2. the commands of the screen it was opened from, under that screen's route;
3. the application's own commands with their icon and label — and says so plainly when none are
   registered.

`Esc` or `F1` again goes back.

The middle section is the one worth knowing about: a view's `Commands()` are the keys that only work
there, so they are the ones somebody pressing `F1` is usually looking for. A screen that registers
none gets no section at all rather than an empty heading.

The wording is localisable like everything else: `HelpKeys` on
[`ArlecchinoStrings`](localization.md) is a delegate that receives the keymap and returns the pairs to
list, so the descriptions can be translated or the order changed without touching the screen.

## Keyboard layouts

Text input — modal fields, list filters, the palette key — goes through `KeyText`, which turns a
`ConsoleKeyInfo` into a character.

| Mode | Behaviour |
|---|---|
| `TextInputMode.LatinOnly` (default) | ASCII characters are taken as typed; anything else falls back to the physical key position, so a Cyrillic layout still produces `q` for the `Q` key |
| `TextInputMode.Native` | Any non-control character is taken as typed |

```csharp
.UseNativeInput()      // or .UseLatinOnlyInput(), or options.TextInput = TextInputMode.Native
```

The fallback covers letters, digits (with shifted symbols), the numpad, space and the OEM punctuation
keys.

:::tip

`KeyText` is registered as a singleton. A view that reads typed characters itself should take it as a
constructor parameter rather than reading `ConsoleKeyInfo.KeyChar` — that is what keeps filters
working on a non-latin layout.

:::

## Paste

Bracketed paste is on by default (`options.BracketedPaste`). The terminal wraps pasted text in
markers, the reader takes the whole block, and it arrives as one edit rather than a burst of key
presses — so a pasted token cannot trip a shortcut or fire validation halfway through.

Where it lands follows what typing would do:

| What is open | Where the paste goes |
|---|---|
| A text or number field | At the caret, dropping characters the field would refuse anyway; only the first line reaches a single-line field |
| A choice modal | Extends its filter |
| Nothing | `IArlecchinoView.HandlePaste`, which does nothing unless the view overrides it |

```csharp
public ViewRoute HandlePaste(string text)
{
    _query.Value += text;
    return ViewRoute.None;
}
```

## Copy

`Ctrl+Insert` and `Ctrl+Shift+C` — the `Copy` binding — copy the field being edited, or the whole text
of the [multi-line dialog](modals.md). It goes through `IArlecchinoTerminal`, which encodes it as an
OSC 52 sequence: that reaches the clipboard of the machine the user is sitting at even over SSH.

`Ctrl+C` is deliberately left alone — it is how the application is stopped. Terminals never
acknowledge a copy, and many have the feature switched off, so there is nothing to report back.

## Replacing the terminal

`IArlecchinoTerminal` is the whole surface between Arlecchino and the console — size, key availability,
`ReadKey`, `Write`, entering or leaving the alternate screen, the mouse, bracketed paste and the
clipboard. `SystemTerminal` is the default; swap it with `.UseTerminal<T>()` to drive a test harness or
a remote session. See [Hosting and options](hosting-and-options.md).
