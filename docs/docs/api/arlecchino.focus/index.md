---
title: Arlecchino.Focus
sidebar_label: Arlecchino.Focus
sidebar_position: 0
---

# Arlecchino.Focus

## Classes

| Type | Summary |
|---|---|
| [`FocusRing`](FocusRing.md) | The cycle of focusable elements inside one view: `Tab` and `Shift+Tab` move between them, everything else goes to the one that holds the focus. A ring is itself focusable, so one goes inside another: add a ring to a ring and `Tab` walks into it, through what it holds and out the far side, without the view saying anything about it. A nested ring remembers where it was left, so coming back to it from either side lands where the cursor was rather than at the top. |
| [`FocusablePane`](FocusablePane.md) | Wraps delegates as a focusable element, for a view that keeps its logic in methods rather than in objects. That is how the file picker holds its list and the sidebar of places. |

## Structs

| Type | Summary |
|---|---|
| [`FocusResult`](FocusResult.md) | What a focusable element did with an event: whether it claimed it, and whether that means going somewhere. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoFocusable`](IArlecchinoFocusable.md) | Something inside a view that can hold the cursor: a pane, a list, a form. Put them in a [`FocusRing`](../arlecchino.focus/FocusRing.md) and the cycling, the routing and the mouse are handled for you. |

## Enums

| Type | Summary |
|---|---|
| [`FocusDirection`](FocusDirection.md) | Which way the focus is being asked to move, on the key that walks the fields of a screen. |

