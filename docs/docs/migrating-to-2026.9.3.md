---
title: Migrating to 2026.9.3
sidebar_label: Migrating to 2026.9.3
description: The two names that gained a Try, and the two terminal members behind them.
---

# Migrating to 2026.9.3

One break, and it is a rename. An application that never asked for keys by position has nothing to
do.

| What changed | What to do |
|---|---|
| `UseKeysByPosition()` | `TryUseKeysByPosition()` |
| `ArlecchinoOptions.KeysByPosition` | `ArlecchinoOptions.TryKeysByPosition` |

## Why the name says trying

The setting used to be something the framework could deliver on its own: it read the position off the
key the terminal named. Outside a Windows console a terminal names no key for a letter that is not
Latin, so what it delivered there was nothing at all.

It now asks the terminal to name its keys, and a terminal is free to ignore the request. The name
says so rather than promising what only some terminals can give:

```csharp
// before
builder.UseKeysByPosition();

// after
builder.TryUseKeysByPosition();
```

Where the terminal answers, a shortcut is reached from the key it sits on whatever the script.
Where it does not, the shortcuts stay where the layout puts them, as they did before the request was
made. Nothing else about the behaviour moved, typing least of all — see
[Keyboard layouts](keyboard.md#keyboard-layouts).

## A terminal of your own

`IArlecchinoTerminal` grew `EnableKeyNames` and `DisableKeyNames`. Both have a body that does
nothing, so an implementation of the interface compiles and runs unchanged; implement them to speak
the kitty keyboard protocol, as `SystemTerminal` does by pushing `CSI > 29 u` and popping it with
`CSI < u`.

`FakeTerminal` records the request as `AreKeyNamesAsked`, for a test that wants to hold an
application to asking.
