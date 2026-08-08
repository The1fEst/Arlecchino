---
title: Arlecchino.Modals
sidebar_label: Arlecchino.Modals
sidebar_position: 0
---

# Arlecchino.Modals

## Classes

| Type | Summary |
|---|---|
| [`Modal`](Modal.md) | A dialog waiting for an answer. Assign one to `ArlecchinoState.Modal` — while it is open it takes every key, draws over the view and suppresses the hints box. A dialog draws itself and reads its own keys. The framework used to do both for it, from a switch over every kind it knew, which meant an application could not add a kind at all. A kind it wrote would match no branch, never be drawn, and swallow every key. Now the kinds the framework brings are nothing more than the first few subclasses, and one an application writes is the next. |
| [`ModalFrame`](ModalFrame.md) | Everything a dialog needs from the application for as long as the screen shows it: where to draw, the words to draw in, the keys to obey, and how to close. A dialog is a value — an application writes `new TextModal { … }` and hands it over — so it cannot be given services when it is built. It is given them when it is asked to do something, which is what lets [`Modal.Draw`](../arlecchino.modals/Modal.md#draw-modalframe) and [`Modal.Handle`](../arlecchino.modals/Modal.md#handle-modalframe-keypress) live on the dialog itself rather than in a switch somewhere that has to know every kind there will ever be. |

## Structs

| Type | Summary |
|---|---|
| [`Piece`](Piece.md) | One run of a line inside a dialog, with the color it is written in. |

