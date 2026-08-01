---
title: Arlecchino.State
sidebar_label: Arlecchino.State
sidebar_position: 0
---

# Arlecchino.State

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoState`](ArlecchinoState.md) | State that outlives a single screen: the output line, the dialog that is open, and a pending file picker request. Derive from it to hang application state that every screen reads. A frame reads all of it, so all of it is written on the drawing thread — the `Request…` methods included, since each of them opens a dialog. Anything arriving on a timer, a task or a socket hands the change over with [`FrameThread.Post`](../arlecchino/FrameThread.md#post-action), which runs it just before the next frame; only [`ArlecchinoState.Invalidate`](../arlecchino.state/ArlecchinoState.md#invalidate) may be called from anywhere. The stack of dialogs is a [`LocalAtomsList`](../arlecchino.atoms/LocalAtomsList-1.md), so opening or closing one asks for a frame by itself. It is outside the undo history: stepping back through what was typed should not reopen a dialog that was answered. |
| [`FilePickerPlace`](FilePickerPlace.md) | A shortcut in the file picker's sidebar, for somewhere the user goes often. |
| [`FilePickerRequest`](FilePickerRequest.md) | Everything the file picker needs for one round of picking. Unlike the modals, the picker is a view of its own, so the request also carries where to go once it is done. |

