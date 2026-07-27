---
title: Arlecchino.Forms
sidebar_label: Arlecchino.Forms
sidebar_position: 0
---

# Arlecchino.Forms

## Classes

| Type | Summary |
|---|---|
| [`Field`](Field.md) | One row of a form: a label, the value beside it, and what happens when it is confirmed. Everything is read through delegates rather than stored, so a field always shows what the state holds now and follows the language the application is running in. The factories bind a field to an atom and pick the dialog that suits its type, which is the usual way to build one. The atom they take is a [`Atom`](../arlecchino.atoms/Atom-1.md), which is either a [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md) — so that editing the field can be undone — or a [`LocalAtom`](../arlecchino.atoms/LocalAtom-1.md) when it should not be. |
| [`Form`](Form.md) | A column of fields with their values lined up, and a help line under the selected one. The form holds no values of its own: the fields read and write atoms, so it draws whatever the state says without any copying back and forth. Whether an edit made here can be undone is decided by the atom behind the field — [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md) or [`LocalAtom`](../arlecchino.atoms/LocalAtom-1.md) — not by the form. |

