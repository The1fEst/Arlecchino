---
title: Migrating to 2026.8.6
sidebar_label: Migrating to 2026.8.6
description: The package an application that undoes now references, the atoms it renames, the form fields it rebuilds through Fields, one color type spelled in capitals, and one line worth changing so the first frame comes up seconds sooner.
---

# Migrating to 2026.8.6

Undo left `Arlecchino.Core` for a package of its own, which is the change an application feels first: an
application that undoes references `Arlecchino.Atoms.History` and renames its atoms, and one that does
not gets smaller. Beside it, a form field is built over its atom, `Rgb` is `RGB`, and the line that
builds the host is worth replacing even though the old one still compiles.

| What changed | What to do |
|---|---|
| `IArlecchinoStartup.Start()` | `StartAsync()` returning `Task<ViewRoute>` — `Task.FromResult` when there is nothing to await |
| `TrackedAtom<T>` | `HistoryAtom<T>` from `Arlecchino.Atoms.History`, with a key — see below |
| `LocalAtom<T>` | `Atom<T>` — the plain type is what "local" meant |
| `TrackedAtomsList/Map/Set/Stack/Queue<T>` | `HistoryAtomsList/Map/Set/Stack/Queue<T>`, with a key |
| `LocalAtomsList/Map/Set/Stack/Queue<T>` | `AtomsList/Map/Set/Stack/Queue<T>` |
| `new AtomHistory(…)` | Take it from the container; the constructor is no longer public outside the package |
| `AtomsList<T>.Touch()` | `Refresh()` |
| `Rgb` | `RGB`, and every signature around it moves with the name |
| `Field` and its factories | `Fields.Text`, `Fields.Number`, … returning `Field<T>` over the atom |
| `HistoryFile.Writes<T>(write)` | `Handles<T>(write, read)` — a kind it writes is one it has to read |
| `ArlecchinoReport(…, AtomHistory)` | The constructor takes five arguments; the history is gone from it |
| `ArlecchinoTestHost.History` | Same member, now an `Arlecchino.Atoms.History.AtomHistory` — add the `using` |
| `Arlecchino.Atoms.IAtomEdit` | `Arlecchino.Atoms.History.IAtomEdit` |
| `Host.CreateApplicationBuilder(args)` | `ArlecchinoHost.CreateBuilder(args)` — see the last section |

## Undo is a package

An application that never undid anything does nothing here, and carries less than it did. One that did
takes the package:

```
dotnet add package Arlecchino.Atoms.History
```

Then the atoms are renamed, and every atom that goes on the stack is given a key. The key is what
carries an atom across a run, so it is required rather than worked out from the property it sits in — a
name derived from a property would quietly orphan a kept history the first time that property was
renamed:

```csharp
// 2026.8.5
public Atom<string> Server { get; } = new TrackedAtom<string>("127.0.0.1");
public Atom<int> Cursor { get; } = new LocalAtom<int>(0);

// 2026.8.6
private const string HistoryKey = "Settings";

public HistoryAtom<string> Server { get; } = new("127.0.0.1", $"{HistoryKey}.Server");
public Atom<int> Cursor { get; } = new(0);
```

`AtomHistory` is registered by `UseHistory` and taken from the container; its constructor is internal to
the package, so one cannot be built by hand any more. Calling `UseHistory` twice throws, since atoms
announce their steps to the whole process and a second history would collect the same steps into a
second stack:

```csharp
builder.Services.UseHistory(options => options.FileName = "session.undo");
```

`FileName` is the whole of it: name a file and the history is written down, outlives the run and is what
the next one comes up in; leave it unnamed and the history is kept in memory for as long as the process
runs. `Capacity` is how many steps stay in memory, 200 by default.

## A startup answers with a `Task`

What decides the first route is often what a startup has to go and read, so `Start` is `StartAsync` and
answers with a `Task<ViewRoute>`. One with nothing to await says so:

```csharp
// 2026.8.5
public ViewRoute Start() => _settings.Exists ? ViewKind.Default : ViewKind.Setup;

// 2026.8.6
public Task<ViewRoute> StartAsync() =>
    Task.FromResult(_settings.Exists ? ViewKind.Default : ViewKind.Setup);
```

The startups still run in registration order, each applied to the navigator before the terminal is
taken. See [Startup routes](hosting-and-options.md#startup-routes).

## A form field is built over its atom

The factories moved to `Fields` and take the atom itself rather than a pair of delegates around it. What
a field says about itself is added on afterwards:

```csharp
// 2026.8.5
new Field(
    label: () => "Profile",
    value: () => settings.Profile.Value,
    onConfirmed: text => settings.Profile.Value = text,
    …)

// 2026.8.6
Fields.Text(() => "Profile", settings.Profile)
    .WithValidation(text => !string.IsNullOrWhiteSpace(text))
    .WithHelp(() => "shown in the title bar")
```

Every factory is narrower for it, and what each used to be handed is added on where it is wanted:
`render` is `WithTemplate`, the `minimum` and `maximum` of a number or a slider are `WithItems` — the
ends of the track are the lowest and the highest of them — `validate` is `WithValidation`, and `help` is
`WithHelp`. `Fields.MultiChoice` takes the `AtomsList<string>` it edits rather than an
`Atom<IReadOnlyList<string>>`, and `Field.PathFrom` is `Fields.Path` with `WithStart` for the folder it
opens in.

`Validation` answers whether a value is allowed rather than handing a message back;
`ArlecchinoStrings.NotAnAllowedValue` is what the form says instead, and it translates like the rest of
the wording. A row with no atom is a button — `Fields.Action` — and the rows a value's own type cannot
tell apart are named: `Fields.Secret`, `Fields.Slider`, `Fields.Path`. `IField` is what a form holds, so
rows over different kinds of value stand in one list. See [Forms](forms.md).

## A color is an `RGB`

A rename the compiler names for you, everywhere the type is written out — `RGB.FromHsl`,
`RGB.TryParseHex`, `Oklch.Of`, `Oklch.ToRgb`, `Contrast`, `Shade`,
`TerminalCapabilities.Background`, `TermColor.ExactForeground` and `ExactBackground`,
`ArlecchinoState.RequestColor` and `ColorModal`, and `Raster` in `Arlecchino.Pictures`. Nothing about
what a color is or does has changed.

## Worth doing: start from `ArlecchinoHost.CreateBuilder`

```diff
- var builder = Host.CreateApplicationBuilder(args);
+ var builder = ArlecchinoHost.CreateBuilder(args);
```

Nothing breaks if you don't — the old line still compiles and still runs. What it also does is reload
configuration from the folder the process was started in, and a folder watched that way is watched along
with everything under it. Started from a home directory that is one watch per folder under it, taken
before the first frame is drawn: on a directory of 340,131 folders, five and a half seconds of it.

`ArlecchinoHost.CreateBuilder` is the same host without that part. Settings still come from the
environment and the command line, and a console logging provider is still registered, so the log overlay
is unaffected. What changes is where a settings file is looked for: next to the application rather than
next to the user, since the content root is the folder the application lives in. An application that
reads one from the working directory on purpose should say so itself — see
[Building the host](hosting-and-options.md#building-the-host).
