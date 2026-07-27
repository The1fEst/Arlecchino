---
title: Stores
sidebar_label: Stores
description: IArlecchinoStore and IArlecchinoScopedStore — a class of atoms that registers itself, and the difference between state that outlives a screen and state that does not.
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
