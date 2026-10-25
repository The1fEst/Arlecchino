---
title: Migrating to 2026.8.5
sidebar_label: Migrating to 2026.8.5
description: A sweep of renames the compiler names for you, and one change of habit — the console is caught now, so clearing the logging providers empties the log overlay instead of protecting the frame.
---

# Migrating to 2026.8.5

Every break in this release is a rename. The type, the shape and the meaning are what they were, so
the compiler names each member that moved and the fix is the new name. One thing that still compiles
changed underneath, though, and it is the one worth reading: `builder.Logging.ClearProviders()` used
to keep a console provider off the frame, and now it is the way to end up with an empty log overlay.

| What changed | What to do |
|---|---|
| `ListBox<T>.Selected`, `Table<T>.Selected`, `Tabs.Selected`, `Tree<T>.Selected`, `Form.Selected` | `SelectedIndex` |
| `Table<T>.SortedBy` | `SortedColumn` — it is the index of a column, not the order the rows are in |
| `ScrollWindow.Around(selected, …)` | `Around(selectedIndex, …)` |
| `TextCompleter.Chosen` | `ChosenIndex` |
| `Theme.Selected`, `Theme.ActiveSelected`, and both on `ThemePalette` | `Selection`, `ActiveSelection` |
| `Theme.Muted`, `ThemePalette.Muted` | `Secondary` |
| `EntryLook.Selected` | `EntryLook.Selection` |
| `KeyStroke.Typed`, `KeyBinding.Typed` | `Character`, matching the `KeyPress.Character` they are compared against |
| `Matches(pressed)`, `Opens(pressed)`, `Closes(pressed)`, `CommandRegistry.Send/TryFind` | The parameter is `press` |
| `Notification.Since` | `RaisedAt` |
| `Notification.Filled()` | `Fraction()` |
| `MultiChoiceModal.Selected` | `SelectedKeys` — it holds keys, not options |
| `MultiChoiceModal.IsSelected(option)`, `Toggle(option)`, `OptionListModal.Take(frame, picked)` | The parameter is `choice` |
| `ArlecchinoState.RequestMultiChoice(…, selected, …)` | `selectedKeys` |
| `ArlecchinoState.RequestConfirmation(title, onConfirmed)` | `onYes` |
| `CompletionAsk.Before`, `CompletionAsk.After` | `Prefix`, `Suffix` |
| `Margin(int all)`, `SurfaceRegion.Inset(int all)` | The parameter is `size` |
| `Joinery.Draw(into, style)` | The parameter is `region` |
| `TerminalProbe.Ask(terminal, within)` | The parameter is `timeout` |
| `ArlecchinoStrings.TerminalNeeded` | `TerminalMinimum` |
| `FakeTerminal.Written`, `FakeTerminal.Copied` | `WrittenText`, `CopiedText` (`Arlecchino.Testing`) |
| `PictureLimits.Most`, `PictureLimits.Enough` | `MostPixels`, `EnoughPixels`, positionally as well (`Arlecchino.Pictures`) |
| `builder.Logging.ClearProviders()` | Delete the line — see below |

Everything else the sweep touched is inside the packages — locals, fields and the parameters of
members no caller can see — and needs nothing from an application.

## The renames in practice

Named arguments are the only place a parameter rename bites, and it bites at compile time:

```csharp
// 2026.8.4
var list = new ListBox<string>(keymap) { Items = items, Selected = 4 };
var window = ScrollWindow.Around(selected: 4, itemCount: items.Count, rows: 10);
row.Fill(chosen ? Theme.ActiveSelected : Theme.Muted);

// 2026.8.5
var list = new ListBox<string>(keymap) { Items = items, SelectedIndex = 4 };
var window = ScrollWindow.Around(selectedIndex: 4, itemCount: items.Count, rows: 10);
row.Fill(chosen ? Theme.ActiveSelection : Theme.Secondary);
```

A palette written against the old names moves the same way — `Selected` to `Selection`,
`ActiveSelected` to `ActiveSelection`, `Muted` to `Secondary` — and nothing about what they draw has
changed. See [Theming](theming.md#roles).

## The console is caught now

A line written to standard output used to land on the frame and scroll it away, which is why
`ClearProviders` was the advice. `AddArlecchino` now stands in front of standard output and standard
error: while a frame is on the screen, what is written there is caught and logged under `stdout` or
`stderr`, visible in the [log overlay](diagnostics.md#the-log-overlay) with escape sequences taken out
of it. Before the terminal is taken and after it is given back, the console works as it always did, so
`--help`, a startup failure and the host's own shutdown lines still print.

Arlecchino no longer registers a logging provider of its own. The overlay draws what a provider writes
to the console, and the default host already has one, so `ILogger` reaches the overlay through it:

```csharp
// 2026.8.4 — the console provider had to go, or it wrote over the frame
builder.Logging.ClearProviders();

// 2026.8.5 — leave it alone; it is how a line reaches the overlay
```

An application that clears every provider is told so in the overlay rather than shown a panel that
stays empty whatever happens; `ArlecchinoStrings.LogWithoutProviders` is the sentence, and it
translates like the rest.

## Nothing to do, but worth knowing

`Ctrl+Shift+C` copies on Windows instead of stopping the application, and a copy now also goes down a
clipboard program's standard input where the terminal drops OSC 52 — both are on
[Escape sequences](ansi.md#what-goes-out).

A palette can be worked out against the background the terminal turned out to be, through
`ArlecchinoOptions.PaletteForBackground` and the `Oklch`, `Contrast` and `Shade` arithmetic behind it.
An application that says nothing keeps the palette it was given. See
[A palette for the terminal you landed on](theming.md#a-palette-for-the-terminal-you-landed-on).
