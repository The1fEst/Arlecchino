---
title: Arlecchino.Widgets
sidebar_label: Arlecchino.Widgets
sidebar_position: 0
---

# Arlecchino.Widgets

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoInteractiveWidget`](IArlecchinoInteractiveWidget.md) | A widget that answers keys and the mouse as well as drawing. Adding one to a [`FocusRing`](../arlecchino.focus/FocusRing.md) is the whole integration, and its members come from [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md). |
| [`IArlecchinoWidget`](IArlecchinoWidget.md) | A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of its own. A widget that also takes keys or the mouse implements [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md) instead. |

