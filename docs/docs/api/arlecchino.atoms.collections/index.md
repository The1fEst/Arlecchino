---
title: Arlecchino.Atoms.Collections
sidebar_label: Arlecchino.Atoms.Collections
sidebar_position: 0
---

# Arlecchino.Atoms.Collections

## Classes

| Type | Summary |
|---|---|
| [`AtomsList<T>`](AtomsList-1.md) | A list held as one piece of application state, changed in place. Every change notifies what reads the list, marks the frame stale and records an undo step. |
| [`AtomsMap<TKey, TValue>`](AtomsMap-2.md) | A map held as one piece of application state, changed in place the way [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md) is. Whether changes can be undone is decided by creating a [`TrackedAtomsMap`](../arlecchino.atoms.tracked/TrackedAtomsMap-2.md) or a [`LocalAtomsMap`](../arlecchino.atoms.local/LocalAtomsMap-2.md). |
| [`AtomsQueue<T>`](AtomsQueue-1.md) | A queue held as one piece of application state, joined at the back and left from the front. It is touched only by the drawing thread, so background work hands its item over with `FrameThread.Post`. |
| [`AtomsSet<T>`](AtomsSet-1.md) | A set held as one piece of application state, behaving as a `HashSet<T>` does: adding what is already there changes nothing. A walk of it is in no order, so sort it where the reader sees the order. |
| [`AtomsStack<T>`](AtomsStack-1.md) | A stack held as one piece of application state, put on and taken off the top. [`AtomsStack.Value`](../arlecchino.atoms.collections/AtomsStack-1.md#value) reads from the top down, so `Value[0]` is what [`AtomsStack.Peek`](../arlecchino.atoms.collections/AtomsStack-1.md#peek) answers. |

