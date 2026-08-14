---
title: Arlecchino.Atoms.Local
sidebar_label: Arlecchino.Atoms.Local
sidebar_position: 0
---

# Arlecchino.Atoms.Local

## Classes

| Type | Summary |
|---|---|
| [`LocalAtom<T>`](LocalAtom-1.md) | An atom the undo stack never sees: a filter, a cursor, a load in progress, a selection — state the user did not author and would not expect to travel back through. It notifies and repaints exactly as a [`TrackedAtom`](../arlecchino.atoms.tracked/TrackedAtom-1.md) does. |
| [`LocalAtomsList<T>`](LocalAtomsList-1.md) | A list the undo stack never sees, for contents the user did not author: a log, search results, the rows of a scan. It notifies and asks for a frame as a [`TrackedAtomsList`](../arlecchino.atoms.tracked/TrackedAtomsList-1.md) does. |
| [`LocalAtomsMap<TKey, TValue>`](LocalAtomsMap-2.md) | A map the undo stack never sees: what a scan found per folder, the sizes worked out so far, the state of each connection. It notifies and asks for a frame exactly as a [`TrackedAtomsMap`](../arlecchino.atoms.tracked/TrackedAtomsMap-2.md) does. |
| [`LocalAtomsQueue<T>`](LocalAtomsQueue-1.md) | A queue the undo stack never sees: files still to copy, requests waiting for an answer, work a background task will pick up. It notifies and asks for a frame exactly as a [`TrackedAtomsQueue`](../arlecchino.atoms.tracked/TrackedAtomsQueue-1.md) does. |
| [`LocalAtomsSet<T>`](LocalAtomsSet-1.md) | A set the undo history never sees: the rows expanded, the folders already walked, the hosts that answered. It notifies and asks for a frame exactly as a [`TrackedAtomsSet`](../arlecchino.atoms.tracked/TrackedAtomsSet-1.md) does. |
| [`LocalAtomsStack<T>`](LocalAtomsStack-1.md) | A stack the undo history never sees: where the user has been, the folders walked into, the screens over one another. It notifies and asks for a frame exactly as a [`TrackedAtomsStack`](../arlecchino.atoms.tracked/TrackedAtomsStack-1.md) does. |

