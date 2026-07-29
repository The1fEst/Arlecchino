---
title: Arlecchino.Atoms
sidebar_label: Arlecchino.Atoms
sidebar_position: 0
---

# Arlecchino.Atoms

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoAsyncStore`](ArlecchinoAsyncStore.md) | A store that has to fetch something before it holds the truth — settings read from disk, a session restored from a server, a catalogue that lives in a file. Derive from it, override [`ArlecchinoAsyncStore.LoadAsync`](../arlecchino.atoms/ArlecchinoAsyncStore.md#loadasync-cancellationtoken), and the framework starts the load as the application starts and keeps the bookkeeping: no worker of its own, and no `TaskCompletionSource` written by hand.  ```csharp public sealed class SettingsStore : ArlecchinoAsyncStore { public TrackedAtom<string> Server { get; } = new("127.0.0.1");  protected override async Task LoadAsync(CancellationToken token) { await using var fs = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read); var saved = await JsonSerializer.DeserializeAsync<Saved>(fs, cancellationToken: token);  Server.Post(saved.Server); } }  ```  Reading the file is the application's own code — the framework has nothing to do with disks, formats or paths. The first frame is drawn without waiting: a terminal that hangs black on a slow disk is worse than a screen that says it is loading. A view draws from [`ArlecchinoAsyncStore.Status`](../arlecchino.atoms/ArlecchinoAsyncStore.md#status), which is an atom and so redraws by itself; code that is not a view — a worker, a command that must not run early — awaits [`ArlecchinoAsyncStore.Ready`](../arlecchino.atoms/ArlecchinoAsyncStore.md#ready). |
| [`AsyncAtom<T>`](AsyncAtom-1.md) | A value produced by background work, with its progress exposed as state so the view can draw a spinner or an error without knowing anything about the task. Results are handed back on the UI thread, and a new load cancels the one before it, so a slow reply can never overwrite a newer one. Nothing here is recorded in history, because loading is not something the user undoes. |
| [`AtomHistory`](AtomHistory.md) | Undo and redo over every [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md). It collects from the moment it exists, so the hosted service resolves it at startup; a headless run has to create it before the edits it wants to undo. The undo stack is bounded: a long-running application would otherwise hold on to every edit it has ever made, and each of those keeps the old value alive too. Steps past [`AtomHistory.Capacity`](../arlecchino.atoms/AtomHistory.md#capacity) fall off the far end, which is the end nobody is going to reach. |
| [`Atom<T>`](Atom-1.md) | An atom: one piece of application state that notifies what reads it and marks the frame stale by itself, so a screen driven by atoms never needs a manual repaint request. Whether an edit can be undone is decided by the type that is created — [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md) or [`LocalAtom`](../arlecchino.atoms/LocalAtom-1.md) — rather than by a flag set afterwards, so the declaration says which kind of state it is. Everything that takes an atom takes this base type, so the two are interchangeable at the call site. |
| [`AtomsList<T>`](AtomsList-1.md) | A list held as one piece of application state. Every change goes through the same path a plain atom's write does — it is checked against the drawing thread, it notifies what reads the list, it marks the frame stale, and it records an undo step when the list is undoable. This is what an `Atom<List<T>>` cannot be. Adding to a list held in an ordinary atom never reaches `Atom.Value`, so nothing is notified and no frame is asked for; writing the same instance back does not help either, because an atom compares by the default comparer and a list is compared by reference, so the write is taken for a change of nothing and dropped. Hold an `Atom<IReadOnlyList<T>>` and replace it wholesale, or hold this and change it in place. Which of the two to reach for is a question of size and rate: replacing a list of a few settings on a keystroke costs nothing, while a log appended to line by line copies the whole of itself on every line. Whether edits can be undone is decided by the type created — [`TrackedAtomsList`](../arlecchino.atoms/TrackedAtomsList-1.md) or [`LocalAtomsList`](../arlecchino.atoms/LocalAtomsList-1.md) — exactly as it is for atoms. |
| [`Computed<T>`](Computed-1.md) | A value derived from other atoms. It re-evaluates lazily and tracks whatever it read while doing so, including other computed values and branches taken only sometimes — reading `other.Value` inside the lambda is the subscription. |
| [`LocalAtom<T>`](LocalAtom-1.md) | An atom the undo stack never sees: a filter, a cursor, a load in progress, a selection — state the user did not author and would not expect to travel back through. It notifies and repaints exactly as a [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md) does. |
| [`LocalAtomsList<T>`](LocalAtomsList-1.md) | A list the undo stack never sees: a log, search results, the rows a background scan found, the notifications on screen — contents the user did not author and would not expect to travel back through. It notifies and asks for a frame exactly as a [`TrackedAtomsList`](../arlecchino.atoms/TrackedAtomsList-1.md) does. |
| [`TrackedAtom<T>`](TrackedAtom-1.md) | An atom whose edits go on the undo stack: the draft being edited, a setting, the selected item — anything a user changed and may want back. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register. |
| [`TrackedAtomsList<T>`](TrackedAtomsList-1.md) | A list whose changes go on the undo stack: the rows of the document being edited, the tasks of a plan, the marked files. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step — a page added with [`AtomsList.Add`](../arlecchino.atoms/AtomsList-1.md#add-ireadonlylist-t) comes back as a page rather than row by row. |

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

