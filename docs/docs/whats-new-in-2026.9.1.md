---
title: What's new in 2026.9.1
sidebar_label: What's new in 2026.9.1
description: A shortcut is reached by where its key sits while typing stays the layout's own, a number asked for through the state takes a fraction, and the help under a form row wraps instead of being cut.
---

# What's new in 2026.9.1

A release about which key a shortcut is reached by, and about two things a dialog and a form were
holding their user to. Reading the keys by position used to cost the language they are typed in, and
now it costs nothing. The one break is in [Migrating to 2026.9.1](migrating-to-2026.9.1.md), and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202691) is the full record.

## Shortcuts by position, typing by layout

`UseKeysByPosition()` used to take every character from where its key sits, which reached the shortcuts
on a Cyrillic layout by making that language untypable — a file manager whose `/` works and whose
rename dialog cannot spell a Russian name.

It now fills in the character the key sits at *beside* the one the layout typed. A binding named after
a character answers to the position; a field takes what the layout made of the key, as it always did:

```csharp
builder.UseKeysByPosition();   // Ж opens the palette, and still types Ж into a field
```

`KeyText` is where the two meet, and it is a static class now — three methods, nothing to resolve from
the container:

| Method | What it answers |
|---|---|
| `KeyText.Resolve(key)` | What the press types |
| `KeyText.ByPosition(key)` | The character the key sits at |
| `KeyText.Shortcut(key)` | What a command is looked up under |

`KeyPress` carries the position as `ByPosition`, filled in by the framework and `'\0'` for an
application that did not ask for it. See [Keyboard layouts](keyboard.md#keyboard-layouts).

## A number with a fraction

`ArlecchinoState.RequestNumber` takes the digits to keep after the separator, and the dialog opens on
the starting value written out to that many:

```csharp
state.RequestNumber("Weight", 1.5m, 0m, 10m, Weigh, decimals: 2);
```

It defaults to zero, which is what the method was held to before and which also refuses the separator
itself — so a field that means to take a fraction has to say how precise it is.

## Help that wraps

The help under a [form](forms.md) row took one row, and whatever did not fit on it was lost, which is a
poor way to say the one thing a row of a form gets to say about itself. It now takes as many rows as it
needs, keeps a line break where the application wrote one, and the fields below move down by as many
rows as it took.

## Packing outside the repository

`pack` takes a version and a folder: `pack 2026.8.7-local.1 --to ../feed` builds the five packages at
that version into a feed of your own, which is how an application is tried against a change before it
is released. See [Packages and building](packages-and-building.md).
