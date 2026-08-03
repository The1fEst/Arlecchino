---
title: Migrating to 4.0
sidebar_label: Migrating to 4.0
description: The usings a namespace split needs, the one behaviour change worth reading, and what 4.0 adds that costs nothing.
---

# Migrating to 4.0

One break and one behaviour change. The break is namespaces, which the compiler points at and a `using`
fixes; the behaviour change is quiet and worth two minutes of reading. Everything else 4.0 added — the
localization generator, layouts, dialogs of your own, `PaintRow`, `Notifications.Recent` — is new
surface an existing application gets without asking.

| What changed | What to do |
|---|---|
| Four namespaces split into sub-namespaces | Add the sub-namespace to the `using` the compiler names |
| A disabled `ViewCommand` no longer swallows its key | Nothing, unless a view relied on the key disappearing |
| `Alt+Esc` now arrives as one key rather than two Escapes | Nothing, unless a view counted on the two |

## Namespaces follow their folders

Nothing was renamed and nothing was removed. Four namespaces had grown past the point where a name
could be found in them by looking, so each was split by what its files are for:

| Was | Now | Holding |
|---|---|---|
| `Arlecchino.Modals` | `Arlecchino.Modals.Asking` | `TextModal`, `TextAreaModal`, `NumberModal`, `NumericModal`, the text interfaces |
| | `Arlecchino.Modals.Choosing` | `ChoiceModal`, `MultiChoiceModal`, `OptionListModal`, `CommandModal` |
| | `Arlecchino.Modals.Setting` | `SliderModal`, `ToggleModal`, `ColorModal`, `DateModal`, `TimeModal`, `SegmentedModal` |
| | `Arlecchino.Modals.Telling` | `MessageModal`, `NotificationModal` |
| `Arlecchino.Widgets` | `Arlecchino.Widgets.Lists` | `ListBox<T>`, `Table<T>`, `Tree<T>`, `Tabs`, `ScrollPane`, `ScrollBar`, `ScrollWindow` |
| | `Arlecchino.Widgets.Pictures` | `Picture` |
| | `Arlecchino.Widgets.Readouts` | `Sparkline`, `BarChart<T>`, `AreaChart`, `Gauge`, `ProgressBar`, `Spinner`, `StatusBar`, `TextView` |
| `Arlecchino.Rendering` | `Arlecchino.Rendering.Colors` | `Theme`, `ThemePalette`, `TermColor`, `RgbTermColor`, `Rgb`, `TerminalColor`, `ColorSupport`, `IArlecchinoColor`, `TextStyle` |
| | `Arlecchino.Rendering.Text` | `TextWidth`, `Joinery`, `Glyphs`, `GraphSymbols` |
| | `Arlecchino.Rendering.Terminals` | `TerminalProbe`, `TerminalCapabilities`, `ImageProtocol` |
| `Arlecchino.Atoms` | `Arlecchino.Atoms.Local` | `LocalAtom<T>` and the local collections |
| | `Arlecchino.Atoms.Tracked` | `TrackedAtom<T>` and the tracked collections |
| | `Arlecchino.Atoms.Collections` | `AtomsList<T>`, `AtomsMap<K,V>`, `AtomsSet<T>`, `AtomsQueue<T>`, `AtomsStack<T>` |

A file that draws a list and a chart now imports two namespaces where it imported one:

```csharp
using Arlecchino.Widgets.Lists;
using Arlecchino.Widgets.Readouts;
```

What stayed where it was is the vocabulary every file reaches for anyway — `Modal` and `ModalFrame` in
`Arlecchino.Modals`, `Surface`, `SurfaceRegion`, `Margin` and `Align` in `Arlecchino.Rendering`, `Atom`
and the store interfaces in `Arlecchino.Atoms`. A `using` for the parent namespace keeps working for
those, which is why most files need one line added rather than one line changed.

:::tip

There is nothing to think about here. Build, and add the namespace each `CS0246` names — the type it
cannot find is in the sub-namespace with the matching job. An IDE's "add using" fixes the whole file at
once.

:::

## A disabled command lets its key through

`ViewCommand.IsEnabled` used to mean two things at once: greyed out on the key screen, and a key that
silently does nothing. The second meaning left a view unable to give the key another job for exactly
the times its command is off.

```csharp
new ViewCommand
{
    Binding = new(ConsoleKey.Escape),
    Label = () => "stop what is running",
    IsEnabled = () => operations.IsBusy,
    Run = () => { operations.Cancel(); return ViewRoute.None; },
}
```

With nothing running, `Esc` used to disappear. It now reaches the commands available everywhere and
then the view's own `Handle` — to leave a search, to clear a filter, to go back — as if nothing had
claimed it.

This is the only change in 4.0 that a compiler cannot point at, so it is worth grepping for
`IsEnabled` and asking of each one whether the key had a second job. An application that wanted the key
eaten should bind it and do nothing rather than disable it:

```csharp
new ViewCommand
{
    Binding = new(ConsoleKey.Escape),
    Label = () => "stop what is running",
    Run = () => { if (operations.IsBusy) { operations.Cancel(); } return ViewRoute.None; },
}
```

## What arrives without being asked for

Nothing here needs an edit; all of it is worth knowing about.

**Text can have a name.** Drop a TOML file in `Localization/`, add it to `AdditionalFiles`, and the
generator emits a `LocString` for every entry and a `Loc` that resolves it. Existing literals go on
working — this is something to move to, a screen at a time. See
[Localization](localization.md#text-with-a-name).

**A dialog can be your own.** `Modal` is abstract over `Draw(ModalFrame)` and
`Handle(ModalFrame, ConsoleKeyInfo)`, so a dialog an application writes is a subclass like every dialog
the framework brings. Assigning `state.Modal = new TextModal { … }` is unchanged. See
[Modals](modals.md#a-dialog-of-your-own).

**A list row can be painted.** `ListBox<T>.PaintRow` draws a row in as many styles as it needs, instead
of one string in one style. `Render` and `ItemStyle` still work and are still what most lists want. See
[Lists](lists.md).

**Notifications can be shown as a stack.** `Notifications.Recent` is everything running plus everything
that ended recently, newest first, for an application that shows its work as cards rather than one
line. See [Diagnostics](diagnostics.md#showing-more-than-the-newest-line).

**Every view can share one frame.** `UseLayout<T>()` draws a band, a bar, or both around every screen,
and `Surface.Content` hands each view the room the layout left it — so an application moves its header
out of the views without editing one of them. See
[Views and navigation](views-and-navigation.md#a-layout-around-every-view).

**A key can be named from the localization.** The generator writes `Bind` beside `LocString`, so
`Bind.To(new(ConsoleKey.F5), LocString.Copy, files.Copy)` replaces the lambda. Existing
`ViewCommand.For(binding, () => "copy", …)` keeps working. See
[Localization](localization.md#naming-a-key).
