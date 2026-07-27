---
title: Arlecchino.State
sidebar_label: Arlecchino.State
sidebar_position: 0
---

# Arlecchino.State

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoState`](ArlecchinoState.md) | State that outlives a single screen: the output line, the dialog that is open, and a pending file picker request. Derive from it to hang application state that every screen reads. |
| [`FilePickerPlace`](FilePickerPlace.md) | A shortcut in the file picker's sidebar, for somewhere the user goes often. |
| [`FilePickerRequest`](FilePickerRequest.md) | Everything the file picker needs for one round of picking. Unlike the modals, the picker is a view of its own, so the request also carries where to go once it is done. |

