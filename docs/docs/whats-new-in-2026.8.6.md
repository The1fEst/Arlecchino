---
title: What's new in 2026.8.6
sidebar_label: What's new in 2026.8.6
description: Undo leaves the core for a package of its own and learns to outlive the run, a form field is built over the atom it edits, and an application draws its first frame seconds sooner.
---

# What's new in 2026.8.6

A release about undo, which left the core for a package of its own and learned to outlive the run; about
a form field built over the atom it edits rather than over seven delegates handed to it; and about the
seconds an application spent before drawing anything at all. The breaks are in
[Migrating to 2026.8.6](migrating-to-2026.8.6.md), and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202686) is the full record.

## Undo is a package now

`Arlecchino.Core` no longer knows what a history is. A plain atom holds a value, tells what reads it and
forgets what it held; what records changes lives in `Arlecchino.Atoms.History` as a subclass —
`HistoryAtom<T>`, and `HistoryAtomsList/Map/Set/Stack/Queue<T>` — along with `AtomHistory` itself.

```csharp
public HistoryAtom<string> Server { get; } = new("127.0.0.1", "SettingsStore.Server");
public Atom<string> Filter { get; } = new("");   // not on the stack, and not meant to be
```

It is taken the way `Arlecchino.Pictures` is: `Arlecchino` does not reference it, so an application that
wants no undo does not carry it, and the atoms that could feed a history allocate nothing for one that is
not there. What the core keeps is the seams the package is built on — `Atom<T>.OnWritten` and
`WriteSilently`, and an `IsObserved` gate on the collections beside `OnInserted`, `OnRemoved`,
`OnReplaced`, `OnReset` — none of which says anything about undo.

`LocalAtom<T>` and `TrackedAtom<T>` are gone with the split, and the five `LocalAtoms*` and
`TrackedAtoms*` with them: the plain type is what "local" used to mean, and the tracked one is now the
history atom by name.

## A history that outlives the run

Nothing records steps until `UseHistory` is called, and one call sets up the stack, the file and the
service that readies both before the first frame:

```csharp
builder.Services.UseHistory(options =>
{
    options.Resume = true;                    // come up where the last run left off
    options.FileName = "history/session.undo";
    options.Capacity = 1000;                  // steps kept before the oldest fall off
});
```

The file is one JSON array of edits per line, so a run that is killed leaves everything up to the last
complete line readable rather than a document that parses as nothing. A run that resumes indexes what it
finds and appends to it; memory holds the last `Capacity` steps, and older ones are seeked to and rebuilt
only when an undo asks for them, so a history of any length costs one offset and one length per step.

What carries an atom across a run is the name it was given, since a reference means nothing to the next
run. The name is required and written in full rather than worked out from the property it sits in — a
name that followed a rename would quietly orphan a history that was kept. `Resume` then starts a named
atom holding what the file last saw it hold, asked as the atom gives its name rather than once at
startup, since a screen's store is built when the screen is.

Types the file already knows are `string`, `bool`, `int`, `long`, `double`, `decimal`, `Guid`,
`DateOnly`, `TimeOnly` and `DateTimeOffset`; a type of your own is taught by hand, because a trimmed
program has no reflection to derive it with:

```csharp
file.Handles<Uri>(
    static (writer, value) => writer.WriteStringValue(value.ToString()),
    static element => new(element.GetString()!));
```

## A form field over its atom

`Field` held seven delegates and answered every question through them, so the same atom was read through
a lambda the field was handed and written through another, and each factory spelled out which dialog to
open. A field is now `Field<T>` over the atom itself: the form reads the value off it, puts it back, and
opens the dialog from the shape of the field. The factories moved to `Fields`, one per kind, and what a
field says about itself is added on afterwards:

```csharp
Fields.Text(() => "Profile", settings.Profile)
    .WithValidation(text => !string.IsNullOrWhiteSpace(text))
    .WithHelp(() => "shown in the title bar")
```

`Validation` answers whether a value is allowed rather than handing back a message, and
`ArlecchinoStrings.NotAnAllowedValue` is what stands in for the reason — a field says which values it
takes rather than why the rest are refused. `Items` is what a value may be, which is what makes a row one
that is picked from, and for a number it is the range it is held between. Rows a value's own type cannot
tell apart say which they are — `SecretField`, `SliderField`, `PathField` — and a row with no atom at all
is a button, which is what `ActionField` is.

## The first frame, seconds sooner

An application started from a home directory took five and a half seconds to draw anything, and none of
them were spent on drawing. `Host.CreateApplicationBuilder` reloads configuration from the folder the
process was started in, and a folder watched that way is watched along with everything under it. On one
home directory of 340,131 folders, that is 340,131 watches taken before the first frame — two thirds of
what the kernel allows a user at once — for a settings file the application never had.

It is the right trade for a service, which is started from the folder it was deployed to. A terminal
application is started wherever its user happens to be, so `ArlecchinoHost.CreateBuilder` is the host
without that part:

```csharp
var builder = ArlecchinoHost.CreateBuilder(args);
```

The same thirteen milliseconds wherever it is started from. Settings still come from the environment and
the command line, a console logging provider is still registered, and settings files are left to the
application — anything read from one is found next to the application rather than next to the user. See
[Building the host](hosting-and-options.md#building-the-host).

## Smaller things

`IReadableAtom` without a type argument is what a widget usually needs: it hears that an atom changed and
writes it out, without knowing what it holds. Every atom overrides `ToString`, and one holding nothing
reads as `IReadableAtom.UndefinedValue`. `IResettableAtom` is beside it, and deliberately not on every
atom — an atom that keeps no record of its own cannot tell the value it was built with from the one it
holds now.

`HistoryAtom<T>.TryReset()` writes back what the atom held before the first edit the history took. It is
written rather than unwound, so the step it makes is itself the next one to be taken back, and no other
atom moves.

A color is an `RGB`. The three letters are read out as letters and stand for the three channels the type
holds, so `Rgb` spelled them as a word nobody says; the type, its members and its meaning are what they
were.

`AtomsList<T>.Touch()` is `Refresh()`, and the five collection atoms share `AtomsCollectionBase`, which
is where `IsObserved` and `Subscribe` now live.

`IArlecchinoStartup.Start` is `StartAsync` and answers with a `Task<ViewRoute>`. What decides the first
route is often what a startup has to go and read, and a synchronous signature left an application
blocking a thread inside it or racing the first frame with work it could not await.
