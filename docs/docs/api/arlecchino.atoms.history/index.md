---
title: Arlecchino.Atoms.History
sidebar_label: Arlecchino.Atoms.History
sidebar_position: 0
---

# Arlecchino.Atoms.History

## Classes

| Type | Summary |
|---|---|
| [`AtomHistory`](AtomHistory.md) | Undo and redo over every [`HistoryAtom`](../arlecchino.atoms.history/HistoryAtom-1.md) and history collection. Recent steps are held in memory, and older ones are read back from the [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md) a step at a time. |
| [`HistoryAtom<T>`](HistoryAtom-1.md) | An atom whose writes go on the undo stack: a draft, a setting, the selected item. A plain [`Atom`](../arlecchino.atoms/Atom-1.md) is the one whose writes do not. |
| [`HistoryFile`](HistoryFile.md) | Where [`AtomHistory`](../arlecchino.atoms.history/AtomHistory.md) writes its steps, as one JSON array of edits per line in a file beside the program. A run that is killed leaves every complete line readable. |

## Interfaces

| Type | Summary |
|---|---|
| [`IAtomEdit`](IAtomEdit.md) | One recorded change of one atom, as kept by [`AtomHistory`](../arlecchino.atoms.history/AtomHistory.md). Replaying it does not record a new step. |
| [`IRestorableAtom`](IRestorableAtom.md) | An atom a step can be handed back to, which is how a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md) written by an earlier run reaches the atoms of this one. It answers to the name it was given. |
| [`IValueReader`](IValueReader.md) | Where an atom takes back the two values a step stood between. The atom calls this, since it is the one place the kind of value is known without reflection. |
| [`IValueWriter`](IValueWriter.md) | Where an edit hands over the two values it stands between. The edit calls this, since it is the one place the kind of value is known without reflection. |
| [`IWritableAtomEdit`](IWritableAtomEdit.md) | An edit a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md) can keep for longer than the run that made it. An edit of an atom that was never named cannot be written, and says so from [`IWritableAtomEdit.WriteTo`](../arlecchino.atoms.history/IWritableAtomEdit.md#writeto-ivaluewriter). |

