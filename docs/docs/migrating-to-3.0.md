---
title: Migrating to 3.0
sidebar_label: Migrating to 3.0
description: What 3.0 removed and the edits an application written against 2.x needs.
---

# Migrating to 3.0

Five members went, and all five are about typing. Everything else 3.0 added — pictures, terminal
probing, shared pane borders — is new surface that an existing application gets without asking. Most
applications need one deletion and nothing else.

| What changed | What to do |
|---|---|
| `UseNativeInput()` is gone | Delete the call — it is what an application gets by default now |
| `UseLatinOnlyInput()` is `UseKeysByPosition()` | Rename it |
| `TextInputMode.LatinOnly` is `TextInputMode.ByPosition` | Rename it |
| `KeyText.LatinOnly` is `KeyText.ByPosition` | Rename it |
| `Notifications` takes a different constructor | Nothing, unless you build one by hand instead of resolving it |

## Any language can be typed without asking

`TextInputMode.Native` is the default. An application that called `UseNativeInput()` to get there
should delete the call:

```csharp
builder.Services
    .AddArlecchino()
    .AddGeneratedViews()
    .UseMouse()
    .UseNativeInput()   // delete this line
    .StartAt(ViewKind.Default);
```

## The other mode is named for what it does

What was `UseLatinOnlyInput()` is `UseKeysByPosition()`, and it now does without exception what it
used to do only sometimes: every character comes from where its key sits on the keyboard rather than
from what the layout makes of it, so the key left of `S` types `a` whether the layout says `a`, `ф`
or `α`.

```csharp
builder.Services.AddArlecchino().UseKeysByPosition();
```

The old name described what the mode accepted; the new one describes how it decides. It used to make
an exception for characters that were already ASCII, which meant a layout that moves letters around
was read inconsistently — the position decides on its own now.

:::caution

The price is unchanged and worth stating plainly: in this mode the languages those layouts exist for
cannot be typed at all. Reach for it when the layout must not decide — a game, a modal editor — and
leave it alone otherwise.

:::

## What arrives without being asked for

Three things change how an application looks or behaves without a line being edited. None of them is
a break, but all three are visible.

**Panes that touch share a line.** A `PaneTree` with `Gaps(inner: 0)` used to put `╮╭` where the eye
expects `┬`. The tree now records its boxes and paints them together — see
[Layout](layout.md).

**The terminal is asked what it can do.** Before the first frame the framework asks the terminal
whether it speaks the kitty graphics protocol or sixel, how large a cell is, and what colour is
behind the text. It costs `TerminalAnswer` — 120 ms by default — only when the terminal says nothing
at all, and nothing a person typed is swallowed. Turn it off with `AskTerminal = false` if you would
rather decide yourself.

**Pictures.** `Picture` draws an image in cells by default, and sends the pixels themselves where the
terminal takes them — see [Pictures](pictures.md).
