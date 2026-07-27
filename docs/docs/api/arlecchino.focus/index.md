---
title: Arlecchino.Focus
sidebar_label: Arlecchino.Focus
sidebar_position: 0
---

# Arlecchino.Focus

## Classes

| Type | Summary |
|---|---|
| [`FocusRing`](FocusRing.md) | The cycle of focusable elements inside one view: `Tab` and `Shift+Tab` move between them, everything else goes to the one that holds the focus. |
| [`FocusablePane`](FocusablePane.md) | Wraps delegates as a focusable element, for a view that keeps its logic in methods rather than in objects — that is how the file picker holds its list and its places sidebar. |

## Structs

| Type | Summary |
|---|---|
| [`FocusResult`](FocusResult.md) | What a focusable element did with an event: whether it claimed it, and whether that means going somewhere. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoFocusable`](IArlecchinoFocusable.md) | Something inside a view that can hold the cursor: a pane, a list, a form. Put them in a [`FocusRing`](../arlecchino.focus/FocusRing.md) and the cycling, the routing and the mouse are handled for you. |

