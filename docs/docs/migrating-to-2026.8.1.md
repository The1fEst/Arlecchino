---
title: Migrating to 2026.8.1
sidebar_label: Migrating to 2026.8.1
description: The two breaks a 5.0 application meets — the notification, and the switch that showed the keys — and the one line of each that changes.
---

# Migrating to 2026.8.1

Two breaks, both narrow. The compiler names every place either of them touches except one, which is
the paragraph worth reading twice: `Notification.Progress` still exists and no longer means what it
meant.

| What changed | What to do |
|---|---|
| `Notification` is a class rather than a record | Nothing, until you used `with`, `Deconstruct` or value equality |
| `Progress` is the fraction; the line of text is `ProgressText` | Rename the text, and read the caution below |
| `Share` is `Progress` | Rename it |
| `Loudness` is `Level` | Rename it; there is only one level now |
| `Time` is `Since` | Rename it |
| `Ended` is `EndedText` | Rename it |
| `options.ShowHints = false` | `options.Hints = HintsShown.Never` |

This release is also where the version numbers change shape: `2026.8.1` is the release that would have
been `6.0.0`, and the number now says the month it was cut in rather than what it broke — see
[Versioning](packages-and-building.md#versioning).

## The notification stops shadowing itself

A notification was a positional record with two properties that answered the same question. `Level`
was fixed at the moment of raising, so the level an entry *turned out* to be had to be read from a
second property, `Loudness` — and code that read `Level` on a copy that failed was quietly reading the
optimism it was raised with. A class settles both into one `Level` the framework writes and a caller
reads. `Time` goes the same way into `Since`, which was already the moment the timeouts are counted
from.

What is left is named after what it holds:

| 5.0 | 2026.8.1 |
|---|---|
| `Progress`, the line of text | `ProgressText` |
| `Share`, the fraction | `Progress` |
| `Loudness` | `Level` |
| `Time` | `Since` |
| `Ended` | `EndedText` |

```csharp
// 5.0
var entry = _state.Notifications.Raise(
    new(DateTimeOffset.Now, NotificationLevel.Information, "Copying")
    {
        Progress = () => $"Copying {copy.Done} of {copy.Total}",
        Share = () => copy.Done / (double)copy.Total,
    });

// 2026.8.1
var entry = _state.Notifications.Raise(
    new(DateTimeOffset.Now, NotificationLevel.Information, "Copying")
    {
        ProgressText = () => $"Copying {copy.Done} of {copy.Total}",
        Progress = () => copy.Done / (double)copy.Total,
    });
```

`Raise`, `Settle`, `Withdraw`, `Current`, `Recent` and `Entries` are untouched, and so are `Line`,
`Detail`, `Actions`, `IsRunning`, `Whole()` and `Filled()` — a view that draws the entries rather than
building them needs the `Loudness` rename and nothing else.

:::caution[`Progress` is the one rename that reads as no rename]

The name stayed and the meaning moved: it was `Func<string>` and is now `Func<double?>`. Every use is
a compile error rather than a wrong line on the screen — the delegate types cannot be confused — but
the error will point at the property you thought you had already migrated. Rename the text to
`ProgressText` first, then the fraction into `Progress`, in that order.

:::

Being a record was never used for what a record is for. Entries are held by reference and settled in
place, so `with`, `Deconstruct` and `ToString` had nothing to do; they are gone with the record, and
so is value equality. An application that compared two entries with `==` now compares references,
which is what comparing notifications always meant in practice: hold the entry `Raise` handed back and
compare against that.

## The keys box has three answers

`ShowHints` was a bool, and a bool could not say the one thing applications asked for: draw the keys
only when they are needed. `HintsShown` has three values, and `Always` is the default, so an
application that never touched the switch is unaffected.

```csharp
// 5.0
options.ShowHints = false;

// 2026.8.1
options.Hints = HintsShown.Never;
```

`WhileWaiting` is the new one: the box stays off until a leader key is half typed, and appears to say
what finishes the chord.

`Never` is a promise rather than a switch. A leader with nothing on screen behind it is a key nobody
presses twice, which is why the box appeared for a chord even when `ShowHints` was false — so an
application that turns the box off entirely should draw the keys in its own shape. `CommandKeys` now
says publicly what the framework's own box was reading:

```csharp
if (keys.IsWaiting)
{
    DrawTheKeysMyOwnWay(keys.Hints());
}
```

See [Hosting and options](hosting-and-options.md) for where the setting lives and
[Keyboard](keyboard.md) for what a chord is.

## What came with it

Nothing below needs an edit; it is what the release added around the two breaks.

- **A fourth package.** [`Arlecchino.Pictures`](pictures.md) reads PNG, JPEG, BMP, Netpbm, QOI and
  Targa into the pixels `Picture` draws, so an application no longer writes its own decoder.
- **The terminal can be lent out.** `Handover.Run` parks the frame loop, gives the modes back and
  starts an editor or a pager with the screen to itself — see [The frame loop](frame-loop.md).
- **A ring inside a ring.** `FocusRing` is focusable itself, so `Tab` walks into a widget made of
  parts and out the far side, and a nested ring remembers where it was left — see [Focus](focus.md).
- **The hints box follows the focus.** An element states its own keys through
  `IArlecchinoFocusable.Hints`, and a view points at whatever holds the focus.
- **A click goes to the pane it landed in** rather than being offered to every widget in turn — see
  [Layout](layout.md).
- **A key can be a character.** `new KeyBinding('!')` answers wherever that character can be typed,
  forgives the Shift that types it, and writes itself on the key screen as `!`.
