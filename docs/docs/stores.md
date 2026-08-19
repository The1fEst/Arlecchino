---
title: Stores
sidebar_label: Stores
description: IArlecchinoStore, IArlecchinoScopedStore and ArlecchinoAsyncStore — a class of atoms that registers itself, what outlives a screen, and a store that loads before it holds the truth.
---

# Stores

A class of [atoms](atoms.md) is a **store**. Marking it `IArlecchinoStore` is all the wiring there is:

```csharp
public sealed class SettingsStore : IArlecchinoStore
{
    public Atom<string> Profile { get; } = new TrackedAtom<string>("");
    public Atom<decimal> Volume { get; } = new TrackedAtom<decimal>(60);
    public Atom<bool> Fullscreen { get; } = new TrackedAtom<bool>(true);
    public Atom<int> Cursor { get; } = new LocalAtom<int>(0);

    public Computed<bool> IsComplete { get; }

    public SettingsStore() => IsComplete = new(() => Profile.Value.Length > 0);
}
```

The [generator](source-generator.md#stores) finds it and `.AddGeneratedStores()` puts it in the
container, so views and commands take it as a constructor parameter like any other service — nothing
to register by hand and nothing to forget when a store is added.

## Two lifetimes

| Interface | Lifetime | For |
|---|---|---|
| `IArlecchinoStore` | Singleton | State the whole application shares |
| `IArlecchinoScopedStore` | Scoped — one per screen | State that belongs to one screen and should start fresh next time |

Both are empty marker interfaces; the choice of which to implement is the whole configuration.

A scoped store lives as long as the [view scope](views-and-navigation.md) does. Navigating away and
coming back builds a new one, so a filter kept in a scoped store resets, and a filter kept in a
singleton store does not. That is usually the decision being made.

## Why a store rather than fields on a view

A view's fields die with the view, which is right for a cursor and wrong for anything a second screen
reads. A store is the place for state with more than one reader:

- the draft two screens both show,
- the settings a command writes and a view draws,
- what a background load produced, so the screen that started it can be left and returned to.

## Composing stores

Stores are plain classes resolved from the container, so a store can take another store:

```csharp
public sealed class DraftStore : IArlecchinoStore
{
    private readonly SettingsStore _settings;

    public DraftStore(SettingsStore settings)
    {
        _settings = settings;
        Title = new Computed<string>(() => $"{_settings.Profile.Value} — {Name.Value}");
    }

    public Atom<string> Name { get; } = new TrackedAtom<string>("");
    public Computed<string> Title { get; }
}
```

A singleton store cannot take a scoped one — that is the standard container rule, and the container
says so at startup rather than at the first navigation.

## A store that loads itself

Settings read from disk, a session restored from a server, a catalogue that lives in a file: a store
that has to fetch something before it holds the truth derives from `ArlecchinoAsyncStore` instead, and
the framework starts the load as the application starts.

```csharp
public sealed class SettingsStore : ArlecchinoAsyncStore
{
    private const string SettingsPath = "settings.json";

    private sealed record Saved(string Server, decimal Port);

    public TrackedAtom<string> Server { get; } = new("127.0.0.1");
    public TrackedAtom<decimal> Port { get; } = new(40000);

    protected override async Task LoadAsync(CancellationToken token)
    {
        if (!File.Exists(SettingsPath))
        {
            return;
        }

        await using var fs = new FileStream(
            SettingsPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None,
            4096,
            true);

        if (fs.Length == 0)
        {
            return;
        }

        var saved = await JsonSerializer.DeserializeAsync<Saved>(fs, cancellationToken: token);
        if (saved == null)
        {
            return;
        }

        Server.Post(saved.Server);
        Port.Post(saved.Port);
    }
}
```

Reading the file is the application's own code — the framework has nothing to do with disks, formats
or paths. What it gives is the rest: no `BackgroundService` to write, no `TaskCompletionSource` to
hand around, and the token is the application's, so a load still running when the user quits is
canceled rather than left behind.

A file that is not there yet is not an error here, and neither is an empty one: both checks return
early and the atoms stay on the defaults they were declared with — which is what the screen then
shows.

`LoadAsync` runs **off the drawing thread**, so what it fetches reaches the atoms through
[`Post`](atoms.md#threads). Writing `Value` there throws and says so.

### Waiting for it

The first frame is drawn without waiting. A terminal that hangs black on a slow disk is worse than a
screen that says it is loading, so the store reports where it got to and the application keeps
running:

| Member | Meaning |
|---|---|
| `Status` | `Idle`, `Loading`, `Loaded` or `Failed`, as an atom — a view that reads it redraws by itself |
| `IsLoading`, `IsLoaded`, `Failed` | The same answer as a `bool`, for an `if` in `Draw` |
| `Error` | What the load threw, or `null` |
| `Ready` | A `Task` that completes when the load does |

A view reads the status, because it draws every frame anyway:

```csharp
public void Draw()
{
    if (_settings.IsLoading)
    {
        _surface.AppendLine("loading settings…", Theme.Secondary, Align.Center);
        return;
    }

    _form.Draw(_surface.Content);
}
```

Code that is not a view — a worker, a command that must not run early — awaits instead:

```csharp
await _settings.Ready;
```

`Ready` faults with whatever `LoadAsync` threw and is canceled when the application stopped before
the load finished, so awaiting it tells you what happened rather than hanging. A store that throws
turns its status to failed, is logged, and leaves the application running on whatever its atoms
already hold — which is why atoms are declared with sensible defaults.

Often no check is needed at all: `Server` starts at `127.0.0.1` and simply changes when the load
lands. Check where an empty value would lie — a list that is empty until it is loaded, a screen that
would otherwise say "0 records".

A store that loads itself is registered like any other, by `AddGeneratedStores()` or `AddStore<T>()`.
A [scoped](#two-lifetimes) one is not started by the host: it belongs to a screen rather than to the
application, so it loads when that screen builds it.

## Persisting a store

Nothing about a store is written to disk for you. Subscribe to what should be saved, or save on the
way out:

```csharp
public sealed class SettingsStore : IArlecchinoStore, IDisposable
{
    private readonly IDisposable _watch;

    public SettingsStore(SettingsFile file)
    {
        Profile = new TrackedAtom<string>(file.Read().Profile);
        _watch = Profile.Subscribe(() => file.Write(Profile.Value));
    }

    public Atom<string> Profile { get; }

    public void Dispose() => _watch.Dispose();
}
```

Writing a file on every keystroke is rarely what you want; [`Ticker.After`](frame-loop.md#work-on-a-clock)
is the usual way to debounce it.

## Reading a store from a widget

Widgets take their rows as data rather than reaching for services, so a view reads the store and hands
the widget what it needs. That keeps a widget usable on a screen whose data comes from somewhere else
entirely — see [Widgets](widgets.md).
