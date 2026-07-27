---
title: Arlecchino.Atoms
sidebar_label: Arlecchino.Atoms
sidebar_position: 0
---

# Arlecchino.Atoms

## Classes

| Type | Summary |
|---|---|
| [`AsyncAtom<T>`](AsyncAtom-1.md) | A value produced by background work, with its progress exposed as state so the view can draw a spinner or an error without knowing anything about the task. Results are handed back on the UI thread, and a new load cancels the one before it, so a slow reply can never overwrite a newer one. Nothing here is recorded in history, because loading is not something the user undoes. |
| [`AtomHistory`](AtomHistory.md) | Undo and redo over every [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md). It collects from the moment it exists, so the hosted service resolves it at startup; a headless run has to create it before the edits it wants to undo. The undo stack is bounded: a long-running application would otherwise hold on to every edit it has ever made, and each of those keeps the old value alive too. Steps past [`AtomHistory.Capacity`](../arlecchino.atoms/AtomHistory.md#capacity) fall off the far end, which is the end nobody is going to reach. |
| [`Atom<T>`](Atom-1.md) | An atom: one piece of application state that notifies what reads it and marks the frame stale by itself, so a screen driven by atoms never needs a manual repaint request. Whether an edit can be undone is decided by the type that is created — [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md) or [`LocalAtom`](../arlecchino.atoms/LocalAtom-1.md) — rather than by a flag set afterwards, so the declaration says which kind of state it is. Everything that takes an atom takes this base type, so the two are interchangeable at the call site. |
| [`Computed<T>`](Computed-1.md) | A value derived from other atoms. It re-evaluates lazily and tracks whatever it read while doing so, including other computed values and branches taken only sometimes — reading `other.Value` inside the lambda is the subscription. |
| [`LocalAtom<T>`](LocalAtom-1.md) | An atom the undo stack never sees: a filter, a cursor, a load in progress, a selection — state the user did not author and would not expect to travel back through. It notifies and repaints exactly as a [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md) does. |
| [`TrackedAtom<T>`](TrackedAtom-1.md) | An atom whose edits go on the undo stack: the draft being edited, a setting, the selected item — anything a user changed and may want back. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoScopedStore`](IArlecchinoScopedStore.md) | A store that lives as long as the screen that asked for it: navigating away disposes the scope and with it the store, and navigating back builds a fresh one. Registered by `AddGeneratedStores()` exactly as an [`IArlecchinoStore`](../arlecchino.atoms/IArlecchinoStore.md) is, only scoped. |
| [`IArlecchinoStore`](IArlecchinoStore.md) | A holder of application state — a class of atoms that outlive the screens reading them. Marking it is all the registration there is: the generator finds every store in the project and `AddGeneratedStores()` puts them in the container as singletons, built from their public constructor with the most parameters. Implement [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md) instead for state that belongs to one screen. |
| [`IAtomEdit`](IAtomEdit.md) | One recorded change of one atom, as kept by [`AtomHistory`](../arlecchino.atoms/AtomHistory.md). Replaying it does not record a new step. |
| [`IReadableAtom<T>`](IReadableAtom-1.md) | Something that holds a value and tells interested parties when it changes. Implemented by [`Atom`](../arlecchino.atoms/Atom-1.md) and [`Computed`](../arlecchino.atoms/Computed-1.md). |

## Enums

| Type | Summary |
|---|---|
| [`LoadStatus`](LoadStatus.md) | Where a background load has got to. |

