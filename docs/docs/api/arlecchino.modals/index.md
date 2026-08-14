---
title: Arlecchino.Modals
sidebar_label: Arlecchino.Modals
sidebar_position: 0
---

# Arlecchino.Modals

## Classes

| Type | Summary |
|---|---|
| [`Modal`](Modal.md) | A dialog waiting for an answer, assigned to `ArlecchinoState.Modal`. It draws itself and reads its own keys, so a kind an application writes is another subclass and nothing more. |
| [`ModalFrame`](ModalFrame.md) | Everything a dialog needs from the application: where to draw, the words to draw in, the keys to obey, and how to close. A dialog is a value, so it is given these when asked to act. |

## Structs

| Type | Summary |
|---|---|
| [`Piece`](Piece.md) | One run of a line inside a dialog, with the color it is written in. |

