---
title: Arlecchino.Atoms.Tracked
sidebar_label: Arlecchino.Atoms.Tracked
sidebar_position: 0
---

# Arlecchino.Atoms.Tracked

## Classes

| Type | Summary |
|---|---|
| [`TrackedAtom<T>`](TrackedAtom-1.md) | An atom whose edits go on the undo stack: the draft being edited, a setting, the selected item — anything a user changed and may want back. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register. |
| [`TrackedAtomsList<T>`](TrackedAtomsList-1.md) | A list whose changes go on the undo stack: the rows of the document being edited, the tasks of a plan, the marked files. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step — a page added with [`AtomsList.Add`](../arlecchino.atoms.collections/AtomsList-1.md#add-ireadonlylist-t) comes back as a page rather than row by row. |
| [`TrackedAtomsMap<TKey, TValue>`](TrackedAtomsMap-2.md) | A map whose changes go on the undo stack: what the user set per profile, the notes kept against each entry, the overrides of a configuration. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step. |
| [`TrackedAtomsQueue<T>`](TrackedAtomsQueue-1.md) | A queue whose changes go on the undo stack: the steps of a plan the user arranged, the batch they lined up. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step — including one that puts several in at once. |
| [`TrackedAtomsSet<T>`](TrackedAtomsSet-1.md) | A set whose changes go on the undo stack: what the user marked, the columns they turned on, the tags they put on something. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step — including one that puts several in at once. |
| [`TrackedAtomsStack<T>`](TrackedAtomsStack-1.md) | A stack whose changes go on the undo stack of their own: the steps the user piled up, a draft being unwound. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step. |

