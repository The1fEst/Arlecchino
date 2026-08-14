---
title: Arlecchino.Atoms
sidebar_label: Arlecchino.Atoms
sidebar_position: 0
---

# Arlecchino.Atoms

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoAsyncStore`](ArlecchinoAsyncStore.md) | A store that has to fetch something before it holds the truth. Override [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken), and the load is started as the application starts and its bookkeeping kept.  ```csharp public sealed class SettingsStore : ArlecchinoAsyncStore { public TrackedAtom<string> Server { get; } = new("127.0.0.1");  protected override async Task LoadAsync(CancellationToken token) { await using var fs = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read); var saved = await JsonSerializer.DeserializeAsync<Saved>(fs, cancellationToken: token);  Server.Post(saved.Server); } }  ``` |
| [`AsyncAtom<T>`](AsyncAtom-1.md) | A value produced by background work, with its progress exposed as state for a view to draw. Results are handed back on the drawing thread, and a new load cancels the one before it. |
| [`AtomHistory`](AtomHistory.md) | Undo and redo over every [`TrackedAtom`](../arlecchino.atoms.tracked/TrackedAtom-1.md), collecting from the moment it exists. The stack is bounded, and steps past [`AtomHistory.Capacity`](../arlecchino.atoms/AtomHistory.md#capacity) fall off the far end. |
| [`Atom<T>`](Atom-1.md) | One piece of application state that notifies what reads it and marks the frame stale by itself. Whether an edit can be undone is decided by creating a [`TrackedAtom`](../arlecchino.atoms.tracked/TrackedAtom-1.md) or a [`LocalAtom`](../arlecchino.atoms.local/LocalAtom-1.md). |
| [`Computed<T>`](Computed-1.md) | A value derived from other atoms. It re-evaluates lazily and tracks whatever it read while doing so, including other computed values and branches taken only sometimes — reading `other.Value` inside the lambda is the subscription. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoScopedStore`](IArlecchinoScopedStore.md) | A store that lives as long as the screen that asked for it: navigating away disposes the scope and with it the store, and navigating back builds a fresh one. Registered by `AddGeneratedStores()` exactly as an [`IArlecchinoStore`](../arlecchino.atoms/IArlecchinoStore.md) is, only scoped. |
| [`IArlecchinoStore`](IArlecchinoStore.md) | A holder of application state: a class of atoms that outlive the screens reading them. Marking it is all the registration there is, and [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md) is the one for a single screen. |
| [`IAtomEdit`](IAtomEdit.md) | One recorded change of one atom, as kept by [`AtomHistory`](../arlecchino.atoms/AtomHistory.md). Replaying it does not record a new step. |
| [`IReadableAtom<T>`](IReadableAtom-1.md) | Something that holds a value and tells interested parties when it changes. Implemented by [`Atom`](../arlecchino.atoms/Atom-1.md) and [`Computed`](../arlecchino.atoms/Computed-1.md). |

## Enums

| Type | Summary |
|---|---|
| [`LoadStatus`](LoadStatus.md) | Where a background load has got to. |

