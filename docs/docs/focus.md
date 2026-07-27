---
title: Focus
sidebar_label: Focus
description: FocusRing, IArlecchinoFocusable and FocusResult — cycling the panes of a view with Tab, and letting a click do the same.
---

# Focus

A view with one list handles keys itself. A view with a sidebar, a table and a filter has to decide
which of them a key belongs to, and that decision is the same every time: `Tab` moves on, everything
else goes to whichever pane has the cursor. `FocusRing` is that decision, written once.

## The ring

```csharp
public sealed class PackagesView : IArlecchinoView
{
    private readonly FocusRing _focus;
    private readonly Tree<Project> _tree;
    private readonly Table<Package> _table;

    public PackagesView(ArlecchinoKeymap keymap)
    {
        _tree = new Tree<Project>(keymap);
        _table = new Table<Package>(keymap);

        _focus = new FocusRing(keymap);
        _focus.Add(_tree);
        _focus.Add(_table);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key).Route;

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse).Route;
}
```

| Member | Meaning |
|---|---|
| `Add(element)` | Adds an element; the first one added starts focused |
| `Current` | The focused element, or `null` while the ring is empty |
| `Index` | Position of the focused element |
| `Items` | The elements, in the order they were added |
| `Focus(element)` | Moves the focus to a particular element of this ring |
| `FocusNext()` / `FocusPrevious()` | Moves on, wrapping around |
| `Handle(key)` | Moves the focus on `NextField` / `PreviousField`, and otherwise hands the key to the focused element |
| `HandleMouse(mouse)` | Offers the event to every element and moves the focus to whichever claims it |

The ring takes the [keymap](keyboard.md#the-keymap), so the keys that move the focus are the
application's `NextField` and `PreviousField` — `Tab` and `Shift+Tab` unless they were rebound.

## IArlecchinoFocusable

```csharp
public interface IArlecchinoFocusable
{
    bool IsFocused { get; set; }
    FocusResult Handle(ConsoleKeyInfo key);
    FocusResult HandleMouse(MouseEvent mouse);
}
```

`IsFocused` is set by the ring. Draw the element differently while it is `false` — that is what the
`Selected` and `ActiveSelected` [roles](theming.md#roles) are for: the cursor row of the pane with the
focus and the cursor row of the pane without it.

Every interactive widget already implements it — [`ListBox`](lists.md), [`Table`](table.md),
[`Tree`](tree.md), [`Tabs`](tabs.md), [`TextView`](text-view.md), [`ScrollPane`](scrolling.md) and
[`Form`](forms.md) — so a ring of widgets needs no adapter.

## FocusResult

An element says what it did with an event:

| Value | Meaning |
|---|---|
| `FocusResult.Ignored` | Not mine; whoever asked should keep looking |
| `FocusResult.Handled` | Mine, and nothing else should see it |
| `FocusResult.Navigate(route)` | Mine, and the screen should go somewhere |

`WasHandled` and `Route` are the two things a caller reads. Returning `Ignored` from the focused
element is what lets a key fall through to the view — a screen-wide `r` for reload keeps working while
a list has the cursor.

## Panes that are not objects

A view that keeps its logic in methods rather than in widgets wraps them:

```csharp
private readonly FocusRing _focus;
private readonly FocusablePane _places;

_places = new FocusablePane(HandlePlacesKey, HandlePlacesClick);
_focus.Add(_places);
```

That is how the [file picker](file-picker.md) holds its list and its places sidebar.

## Focus and the mouse

`HandleMouse` on the ring asks **every** element, not just the focused one, and moves the focus to the
one that claims the event. So a click on an unfocused pane selects it and acts inside it in one go,
which is what a click is expected to do. An element that does not care where the pointer was should
return `Ignored` for events outside its own [region](layout.md#regions).
