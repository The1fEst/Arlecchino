---
title: "IArlecchinoInteractiveWidget"
sidebar_label: "IArlecchinoInteractiveWidget"
---

# IArlecchinoInteractiveWidget interface

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

A widget that answers keys and the mouse as well as drawing: a list, a table, a set of tabs, a form. Adding one to a [`FocusRing`](../arlecchino.focus/FocusRing.md) is the whole integration — the ring cycles the focus with `Tab`, hands keys to whichever widget holds it, and moves the focus to the widget that claims a click. The members come from [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md): `IsFocused` for drawing the difference, and `Handle` / `HandleMouse` returning a [`FocusResult`](../arlecchino.focus/FocusResult.md) that says whether the event was claimed and whether it navigates.

```csharp
public interface IArlecchinoInteractiveWidget : IArlecchinoWidget, IArlecchinoFocusable
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)  
**Implemented by** [`Form`](../arlecchino.forms/Form.md), [`ListBox`](../arlecchino.widgets.lists/ListBox-1.md), [`ScrollPane`](../arlecchino.widgets.lists/ScrollPane.md), [`Table`](../arlecchino.widgets.lists/Table-1.md), [`Tabs`](../arlecchino.widgets.lists/Tabs.md), [`Tree`](../arlecchino.widgets.lists/Tree-1.md), [`TextView`](../arlecchino.widgets.readouts/TextView.md)

