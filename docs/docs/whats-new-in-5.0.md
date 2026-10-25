---
title: What's new in 5.0
sidebar_label: What's new in 5.0
description: A key press becomes a type with room for Command, and a binding is built rather than listed — alternatives added one at a time, and a second keystroke turning it into a chord.
---

# What's new in 5.0

A release about the keyboard. A key press stops being the console's own type and becomes one with room
for the modifier that terminal reports and the console type could not hold; a binding stops being a
list of positions and becomes something built. The break is the type itself —
[Migrating to 5.0](migrating-to-5.0.md) is the two-line change most applications need, and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#500) is the full record.

## A key press has room for Command

`ConsoleKeyInfo` stores Shift, Alt and Control as three booleans, and there is no fourth slot. The key
next to the space bar — Command on a Mac, the Windows key elsewhere — needs one, and a terminal does
report it: one more bit in the same modifier field as the rest.

```csharp
public ViewRoute Handle(KeyPress key) =>
    key is { Key: ConsoleKey.LeftArrow, Modifiers: KeyModifiers.Super } ? ViewKind.Default : ViewRoute.None;
```

That bit is not a curiosity. On a Mac terminal, Option is spoken for by the characters it types, so
`Alt` never reaches an application at all and every binding built on it is unreachable. Command is what
that keyboard has going spare, and a binding wearing it relabels itself for the machine it runs on:
`Cmd+←` on a Mac, `Win+←` elsewhere.

A whole keymap moves onto it at once, because rewriting thirty bindings by hand is how an application
ends up with twenty-eight of them rewritten:

```csharp
builder.UseKeymap(new ArlecchinoKeymap().Replacing(KeyModifiers.Alt, KeyModifiers.Super));
```

## A binding is built, not listed

`KeyBinding` used to carry its second combination as two more positional parameters, which allowed
exactly one alternative and read as four keys in a row at the call site. Now the binding is the
combination it is named after, and everything else is added to it:

```csharp
new KeyBinding(ConsoleKey.Insert, KeyModifiers.Control)
    .AddAlternative(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);
```

Call `AddAlternative` as often as there are habits to answer to. What is written on the keys screen is
the combination the binding is named after; the alternatives are matched and never drawn, so a screen
lists one key per command however many reach it.

## Two keystrokes, one command

A terminal hands on a handful of combinations and no more. Option is spoken for, Command belongs to the
window, and what is left on a Mac are the letters held with Control — and there are not thirty of those.
A leader spends one of them and hands back the alphabet behind it:

```csharp
new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T);
```

While a leader is half typed, the hints box stops listing the keys that are out of reach and lists what
finishes the chord instead, so the second key is read rather than remembered. An application that turned
the hints box off still gets this one: turning it off says something about the keys of a screen, not
about a key half pressed.

`Opens` and `Closes` ask about the two halves, and `Matches` answers `false` for a chord, so a leader on
its own runs nothing. [Keyboard](keyboard.md) is where the whole of it is written down.
