---
title: What's new in 4.0
sidebar_label: What's new in 4.0
description: Every string gets a name the compiler checks, a dialog draws itself, a list row is painted rather than written, and namespaces follow the folders they are in.
---

# What's new in 4.0

A release about names. Text gets one, so the same sentence cannot be typed twice and drift; the
framework's own namespaces get better ones, which is the whole of the break —
[Migrating to 4.0](migrating-to-4.0.md) is the short list of what needs an edit, and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#400) is the full record.

## Every string has a name

Text written where it is drawn gets written twice. The same sentence appears in a dialog and in the log
line that follows it, and the day one of them is reworded the two quietly disagree — nothing fails,
nobody notices, and the application says two different things about one event.

Put the text in a TOML file and a generator gives each entry a name:

```toml
# Localization/Localization.toml
[localization]
language = "en"

[strings]
Copy = "Copy"
CopyManyTitle = "Copy {0} items"
```

```csharp
Title = sources.Count == 1 ? Loc(LocString.Copy) : Loc(LocString.CopyManyTitle, sources.Count);
```

The second mention is now a name the compiler checks rather than a sentence somebody retyped, and the
file is the one place to read the application's whole voice at once. Translation falls out of the same
machinery — every other file in the folder is a translation of the default, a string it leaves out
falls back rather than leaving a hole, and one it invents is an error — but a single language is reason
enough. See [Localization](localization.md#text-with-a-name).

## A dialog draws itself

The dialogs that ship know what a number looks like and what a choice looks like. An application with a
look of its own wanted neither, and could not have anything else: the framework drew and routed every
dialog from a switch over the kinds it knew, so a kind an application wrote would match no branch,
never be drawn, and swallow every key.

`Modal` is now abstract over `Draw` and `Handle`. The kinds the framework brings are nothing more than
its first few subclasses, and one you write is the next — the same slot, the same stack, the same
rules:

```csharp
public sealed class ConfirmModal : Modal
{
    public required Action<bool> OnAnswer { get; init; }

    public override void Draw(ModalFrame frame) =>
        (Box, _) = frame.Box(Title, [[new Piece("Really?", Theme.Warning)]], "y · n");

    public override void Handle(ModalFrame frame, ConsoleKeyInfo key)
    {
        frame.Close();
        OnAnswer(key.Key == ConsoleKey.Y);
    }
}
```

A dialog is a value, so it cannot be handed services when it is built. `ModalFrame` carries them for as
long as it is on screen: the screen, the words, the keys to obey, `Close`, `Copy`, and `Box` — the
titled box with its hints under a rule that every dialog the framework brings is drawn through, which
is what makes one you wrote read as the same application. See
[Modals](modals.md#a-dialog-of-your-own).

## One frame around every view

A band along the top, a bar along the bottom, whatever a screen of this application always has around
it. `IArlecchinoLayout` is Razor's `_Layout.cshtml` with `@RenderBody()`: it is handed the room there
is and a delegate that draws the view, and where it calls that delegate is where the view goes.

```csharp
public sealed class Chrome : IArlecchinoLayout
{
    public void Draw(SurfaceRegion frame, Action<SurfaceRegion> body)
    {
        _tabs.Draw(frame.Rows(0, 1));
        body(frame.Rows(1, frame.Height - 2));
        _bar.Draw(frame.Rows(frame.Height - 1, 1));
    }
}

builder.Services.AddArlecchino().UseLayout<Chrome>();
```

No view has to be edited: it asks the `Surface` for its content as it always did and is handed the
room the layout left it. One instance serves the whole application, so what it holds outlives the
view — a row of tabs keeps its scroll position when a screen is left and come back to. A screen that
wants the whole terminal answers `false` to `IArlecchinoView.UsesLayout`. See
[Views and navigation](views-and-navigation.md#a-layout-around-every-view).

## A key named from the localization

The generator writes `Bind` beside the `LocString` it emits, so a key is named out of the same file as
everything else:

```csharp
Bind.To(new(ConsoleKey.F5), LocString.Copy, files.Copy)
Bind.Going(new(ConsoleKey.F3), LocString.View, files.Read)
Bind.When(new(ConsoleKey.Escape, ConsoleModifiers.Alt), LocString.Stop, () => work.IsBusy, work.Cancel)
```

`ViewCommand` takes a `Func<string>` and can take nothing else — a label is read every frame so that
changing language changes the screen — and it cannot take a `LocString`, because there is no such type
until an application is compiled. So the shorthand is written into the application beside the enum it
names, which is the only place both are in scope.

## A list row can be painted

`Render` and `ItemStyle` write a row as one string in one style, which is right for most lists and
wrong for any where a name, a size and a date each want their own. `PaintRow` hands over one row of the
list to draw in, with the scrolling, the wheel and the clicks already worked out:

```csharp
var files = new ListBox<FileEntry>(keymap)
{
    Render = static entry => entry.Name,
    PaintRow = (row, entry, chosen) =>
    {
        row.Fill(chosen ? Theme.ActiveSelected : Theme.Default);
        row.Write(0, 0, entry.Name, chosen ? Theme.ActiveSelected : Theme.Accent);
        row.WriteLine(0, Sizes.Brief(entry.Size), Theme.Muted, Align.Right);
    },
};
```

`Render` is still worth setting: it is what filtering and keyboard search read. See
[Lists](lists.md).

## More than the newest line

`Notifications.Current` answers what one row at the bottom of the screen should say, and one row can
only hold the newest. `Recent` is everything worth showing right now — everything still running
whatever its age, and everything that ended within `NotificationTimeout` — newest first, so an
application that shows its work as a stack of cards can show all of it, and a copy that takes an hour
stays up for the hour. See [Diagnostics](diagnostics.md#showing-more-than-the-newest-line).

## A disabled command no longer eats its key

`ViewCommand.IsEnabled` meant two things at once: greyed out on the key screen, and a key that silently
does nothing. The second one left a view unable to give the key a second meaning for exactly the times
its command is off — `Esc` bound to "stop what is running" made `Esc` do nothing at all whenever
nothing was running.

An unavailable command is now skipped, and the key carries on to the commands available everywhere and
then to the view's own `Handle`, as if nothing had claimed it. This one is a behaviour change rather
than a compile error; see [Migrating to 4.0](migrating-to-4.0.md#a-disabled-command-lets-its-key-through).

## Namespaces follow their folders

Four namespaces had grown past the point where a name could be found in them by looking. Each was split
by what its files are for, and this time the split is a real namespace rather than a folder nobody
outside the repository can see:

| Was | Now |
|---|---|
| `Arlecchino.Modals` | `.Asking`, `.Choosing`, `.Setting`, `.Telling` |
| `Arlecchino.Widgets` | `.Lists`, `.Pictures`, `.Readouts` |
| `Arlecchino.Rendering` | `.Colors`, `.Text`, `.Terminals` |
| `Arlecchino.Atoms` | `.Local`, `.Tracked`, `.Collections` |

Nothing was renamed and nothing was removed, so every fix is adding a sub-namespace to a `using`. What
stayed put is the vocabulary every file reaches for anyway: `Modal` and `ModalFrame`, `Surface` and
`SurfaceRegion`, `Margin` and `Align`, `Atom` and the store interfaces.

## Added

| What | Where |
|---|---|
| The localization generator, `LocString`, `Loc`, `Bind` | [Localization](localization.md#text-with-a-name) |
| `IArlecchinoLayout`, `UseLayout<T>()`, `IArlecchinoView.UsesLayout` | [Views and navigation](views-and-navigation.md#a-layout-around-every-view) |
| `Modal.Draw` / `Modal.Handle`, `ModalFrame`, `Piece` | [Modals](modals.md#a-dialog-of-your-own) |
| `ListBox<T>.PaintRow` | [Lists](lists.md) |
| `Notifications.Recent` | [Diagnostics](diagnostics.md#showing-more-than-the-newest-line) |
| `ArlecchinoLocalizationFolder`, `ArlecchinoLocalizationLanguage` | [Source generator](source-generator.md#msbuild-switches) |

## Fixed

- **`Alt+Esc` reached an application as two plain Escapes**, which left it impossible to bind. Holding
  Alt puts an escape in front of the key, and this key is itself an escape; the runtime folds that
  prefix back for every other key and left `\e\e` as it found it. There is no such fix for `Ctrl+Esc`
  and there cannot be one — a terminal has no encoding for it, so nothing is sent at all.

## What came just before it

`3.1.0` was all `Arlecchino.Testing`. A test used to read the bytes a frame wrote; `ScreenGrid` reads
the cells instead, so an assertion is about what is on the screen rather than about the escape
sequences that put it there. See [Testing](testing.md).
