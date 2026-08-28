---
title: Arlecchino.Atoms
sidebar_label: Arlecchino.Atoms
sidebar_position: 0
---

# Arlecchino.Atoms

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoAsyncStore`](ArlecchinoAsyncStore.md) | A store that has to fetch something before it holds the truth. Override [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken), and the load is started as the application starts and its bookkeeping kept.  ```csharp public sealed class SettingsStore : ArlecchinoAsyncStore { public HistoryAtom<string> Server { get; } = new("127.0.0.1", "SettingsStore.Server");  protected override async Task LoadAsync(CancellationToken token) { await using var fs = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read); var saved = await JsonSerializer.DeserializeAsync<Saved>(fs, cancellationToken: token);  Server.Post(saved.Server); } }  ``` |
| [`AsyncAtom<T>`](AsyncAtom-1.md) | A value produced by background work, with its progress exposed as state for a view to draw. Results are handed back on the drawing thread, and a new load cancels the one before it. |
| [`Atom<T>`](Atom-1.md) | One piece of application state that notifies what reads it and marks the frame stale by itself. |
| [`Computed<T>`](Computed-1.md) | A value derived from other atoms. It re-evaluates lazily and tracks whatever it read while doing so, including other computed values and branches taken only sometimes — reading `other.Value` inside the lambda is the subscription. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoScopedStore`](IArlecchinoScopedStore.md) | A store that lives as long as the screen that asked for it: navigating away disposes the scope and with it the store, and navigating back builds a fresh one. Registered by `AddGeneratedStores()` exactly as an [`IArlecchinoStore`](../arlecchino.atoms/IArlecchinoStore.md) is, only scoped. |
| [`IArlecchinoStore`](IArlecchinoStore.md) | A holder of application state: a class of atoms that outlive the screens reading them. Marking it is all the registration there is, and [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md) is the one for a single screen. |
| [`IReadableAtom`](IReadableAtom.md) | What every atom has in common, whatever it holds: it can be listened to, and it can be written out. It is what a widget takes when it shows a value whose type it has no use for. |
| [`IReadableAtom<T>`](IReadableAtom-1.md) | Something that holds a value and tells interested parties when it changes. Implemented by [`Atom`](../arlecchino.atoms/Atom-1.md) and [`Computed`](../arlecchino.atoms/Computed-1.md). |
| [`IResettableAtom`](IResettableAtom.md) | An atom that knows where it began and can be put back there. One keeping no record of its own cannot tell the value it was built with from the one it holds. |

## Enums

| Type | Summary |
|---|---|
| [`LoadStatus`](LoadStatus.md) | Where a background load has got to. |

