---
title: Arlecchino.Forms
sidebar_label: Arlecchino.Forms
sidebar_position: 0
---

# Arlecchino.Forms

## Classes

| Type | Summary |
|---|---|
| [`ActionField`](ActionField.md) | A button rather than a value, for the things a form does once it has been filled in. It holds no atom, which is what the form draws it as a button by. |
| [`Field<T>`](Field-1.md) | One row of a form over a value of a known kind. It holds no value of its own, so a row shows what the state holds now; build one through [`Fields`](../arlecchino.forms/Fields.md) and add the rest with the `With` members. |
| [`Fields`](Fields.md) | The rows a form is built out of. Each one gives the row the shape [`Form`](../arlecchino.forms/Form.md) reads it by, so an application says what a row is over and never says which dialog to open. |
| [`Form`](Form.md) | A column of fields with their values lined up, and a help line under the selected one. It holds no values of its own, and the atom behind each field decides whether an edit can be undone. |
| [`PathField`](PathField.md) | A path on disk. It leaves the form rather than opening a dialog over it, since the picker is a view of its own, and so it has to be told where to come back to. |
| [`SecretField`](SecretField.md) | A line of text that is never shown or typed in the clear. Nothing about a string says it is a secret, so the row says it instead; the atom still holds the text as it is. |
| [`SliderField`](SliderField.md) | A number moved along a track rather than typed. Both kinds of number row are over the same `decimal`, so the row says which it is. |

## Interfaces

| Type | Summary |
|---|---|
| [`IField`](IField.md) | One row of a form, as the form sees it. Rows over values of different kinds stand in one list through this, and the form asks the atom rather than the row for what is shown. |

