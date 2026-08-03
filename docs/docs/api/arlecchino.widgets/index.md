---
title: Arlecchino.Widgets
sidebar_label: Arlecchino.Widgets
sidebar_position: 0
---

# Arlecchino.Widgets

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoInteractiveWidget`](IArlecchinoInteractiveWidget.md) | A widget that answers keys and the mouse as well as drawing: a list, a table, a set of tabs, a form. Adding one to a [`FocusRing`](../arlecchino.focus/FocusRing.md) is the whole integration — the ring cycles the focus with `Tab`, hands keys to whichever widget holds it, and moves the focus to the widget that claims a click. The members come from [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md): `IsFocused` for drawing the difference, and `Handle` / `HandleMouse` returning a [`FocusResult`](../arlecchino.focus/FocusResult.md) that says whether the event was claimed and whether it navigates. |
| [`IArlecchinoWidget`](IArlecchinoWidget.md) | A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of its own, so the same widget works in a pane, in a column or across the whole frame. This is the contract every built-in widget answers, and the one to implement for a widget of your own. A widget that also takes keys or the mouse implements [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md) instead. |

