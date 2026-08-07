---
title: Migrating to 5.0
sidebar_label: Migrating to 5.0
description: One type replaces another everywhere a key is handled, why it had to, and the two-line change most applications need.
---

# Migrating to 5.0

One break, and it is a wide one: a key press is now `KeyPress` rather than `ConsoleKeyInfo`. The
compiler names every place, the fix is the same in each, and there is nothing subtle to watch for
afterwards.

| What changed | What to do |
|---|---|
| `Handle` takes `KeyPress` | Change the parameter type; the members are `Key`, `Modifiers` and `Character` |
| `KeyChar` is `Character` | Rename it |
| `ConsoleModifiers` is `KeyModifiers` | Rename it; `Shift`, `Alt` and `Control` keep their meaning |
| `Press(key, shift, alt, control)` in tests takes modifiers | `Press(ConsoleKey.C, KeyModifiers.Control)` |
| A session tape written by 4.x does not load | Record it again |

## Why the console type had to go

`ConsoleKeyInfo` stores Shift, Alt and Control as three booleans, and its constructor takes those three
and nothing else. There is no fourth slot, and the key next to the space bar — Command on a Mac, the
Windows key elsewhere — needs one. A terminal reports it in the same modifier field as the rest, one
bit further up, so the choice was between dropping that bit and having a type with room for it.

The bit is not a curiosity. On a Mac terminal, Option is spoken for by the characters it types, so
`Alt` never reaches an application at all, and every binding built on it is unreachable. Command is
the modifier that keyboard has going spare.

## Changing a view

```csharp
// 4.x
public ViewRoute Handle(ConsoleKeyInfo key) =>
    key.Key == ConsoleKey.Escape ? ViewKind.Default : ViewRoute.None;

// 5.0
public ViewRoute Handle(KeyPress key) =>
    key.Key == ConsoleKey.Escape ? ViewKind.Default : ViewRoute.None;
```

`KeyPress` lives in `Arlecchino.Input`, which most files already import for `KeyBinding` or
`MouseEvent`. The same change applies to `IArlecchinoFocusable.Handle` on a widget of your own, to
`Modal.Handle` on a dialog of your own, and to a `FocusablePane` built from a delegate.

Three members, one of them renamed:

| 4.x | 5.0 |
|---|---|
| `key.Key` | `key.Key` |
| `key.Modifiers` | `key.Modifiers`, now `KeyModifiers` |
| `key.KeyChar` | `key.Character` |

Code that still holds a `ConsoleKeyInfo` — something reading the console itself — converts with
`KeyPress.From(consoleKey)`. Nothing is lost by it: the console cannot report Command in the first
place.

## Bindings

`KeyBinding` takes `KeyModifiers`, which is `ConsoleModifiers` with one more value:

```csharp
new KeyBinding(ConsoleKey.S, KeyModifiers.Control)      // as before, renamed
new KeyBinding(ConsoleKey.C, KeyModifiers.Super)        // new
```

The first three keep the values the console gave them, so a binding built from a number rather than a
name still means what it meant.

## What to do about Command

Most applications need nothing: the framework's own history keys already answer to Command and to Alt
both, and everything else in the [keymap](keyboard.md#the-keymap) is on Control, which every terminal
sends.

An application with bindings of its own on `Alt` — and Mac users — wants one line:

```csharp
.UseKeymap(new ArlecchinoKeymap().Replacing(KeyModifiers.Alt, KeyModifiers.Super))
```

`KeyBinding.Replacing` does the same for a single binding, which is what a
[view command](commands.md) built on `Alt` wants.

:::caution[Command does not arrive everywhere]

Which modifiers reach an application is decided by the terminal, not by the operating system. Several
Mac terminals keep Command for their own menus and never send it, exactly as several never send `Alt`.
Bind both where it matters — a `KeyBinding` carries a second combination for precisely this — rather
than moving an action onto Command alone.

:::

## Tests

`ArlecchinoTestHost.Press` and `SessionTape.Key` take modifiers instead of three booleans, which is
also the only way to press the one the booleans had no room for:

```csharp
app.Press(ConsoleKey.LeftArrow, KeyModifiers.Super);
```

A tape written by 4.x does not load: the key line carries one number for the modifiers where it used
to carry three flags. Record it again — that is faster than editing it, and a tape is a recording
rather than a source file.

## What came with it

Two things that were broken before the modifier had anywhere to go, and are fixed by its arrival:

- A cursor key held with Command used to read as the bare key — `Cmd+←` moved the cursor left instead
  of going back.
- A letter held with Command used to arrive as text. There is no legacy spelling for `Cmd+J`, so a
  terminal falls back to a shape the reader did not understand and replayed a character at a time,
  putting `[106;9u` into whatever was being edited.
