using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Focus;

/// <summary>
/// The cycle of focusable elements inside one view: <c>Tab</c> and <c>Shift+Tab</c> move between
/// them, everything else goes to the one that holds the focus.
/// </summary>
public sealed class FocusRing
{
    private readonly List<IFocusable> _items = [];
    private readonly ArlecchinoKeymap _keymap;

    private int _index;

    /// <summary>Creates an empty ring.</summary>
    /// <param name="keymap">Where the keys that move the focus come from.</param>
    public FocusRing(ArlecchinoKeymap keymap)
    {
        _keymap = keymap;
    }

    /// <summary>The elements, in the order they were added.</summary>
    public IReadOnlyList<IFocusable> Items => _items;

    /// <summary>Position of the focused element.</summary>
    public int Index => _index;

    /// <summary>The focused element, or <c>null</c> when the ring is empty.</summary>
    public IFocusable? Current => _items.Count == 0 ? null : _items[Math.Clamp(_index, 0, _items.Count - 1)];

    /// <summary>Adds an element. The first one added starts focused.</summary>
    /// <param name="item">The element to add.</param>
    public void Add(IFocusable item)
    {
        _items.Add(item);
        item.IsFocused = _items.Count == 1;
    }

    /// <summary>Moves the focus to a particular element, if it belongs to this ring.</summary>
    /// <param name="item">The element to focus.</param>
    public void Focus(IFocusable item)
    {
        var index = _items.IndexOf(item);
        if (index >= 0)
        {
            MoveTo(index);
        }
    }

    /// <summary>Moves the focus to the next element, wrapping around at the end.</summary>
    public void FocusNext() => MoveTo(_items.Count == 0 ? 0 : (_index + 1) % _items.Count);

    /// <summary>Moves the focus to the previous element, wrapping around at the start.</summary>
    public void FocusPrevious() => MoveTo(_items.Count == 0 ? 0 : (_index - 1 + _items.Count) % _items.Count);

    /// <summary>
    /// Moves the focus on the field keys, and otherwise hands the key to the focused element.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The route the element asked for, or <see cref="ViewRoute.None"/>.</returns>
    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        if (_keymap.PreviousField.Matches(key))
        {
            FocusPrevious();
            return ViewRoute.None;
        }

        if (_keymap.NextField.Matches(key))
        {
            FocusNext();
            return ViewRoute.None;
        }

        return Current?.Handle(key).Route ?? ViewRoute.None;
    }

    /// <summary>
    /// Offers the event to each element and moves the focus to whichever one claims it, so a click
    /// both selects a pane and acts inside it.
    /// </summary>
    /// <param name="mouse">The event, in frame coordinates.</param>
    /// <returns>The route the element asked for, or <see cref="ViewRoute.None"/>.</returns>
    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        foreach (var item in _items)
        {
            var result = item.HandleMouse(mouse);
            if (!result.WasHandled)
            {
                continue;
            }

            Focus(item);
            return result.Route;
        }

        return ViewRoute.None;
    }

    private void MoveTo(int index)
    {
        if (_items.Count == 0)
        {
            return;
        }

        _items[Math.Clamp(_index, 0, _items.Count - 1)].IsFocused = false;
        _index = Math.Clamp(index, 0, _items.Count - 1);
        _items[_index].IsFocused = true;
    }
}
