using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Focus;

/// <summary>
/// The cycle of focusable elements inside one view: <c>Tab</c> and <c>Shift+Tab</c> move between
/// them, everything else goes to the one that holds the focus.
///
/// A ring is itself focusable, so one goes inside another: add a ring to a ring and <c>Tab</c> walks
/// into it, through what it holds and out the far side, without the view saying anything about it. A
/// nested ring remembers where it was left, so coming back to it from either side lands where the
/// cursor was rather than at the top.
/// </summary>
public sealed class FocusRing : IArlecchinoFocusable
{
    private readonly List<IArlecchinoFocusable> _items = [];
    private readonly ArlecchinoKeymap _keymap;

    private int _index;
    private bool _isFocused = true;

    /// <summary>Creates an empty ring.</summary>
    /// <param name="keymap">Where the keys that move the focus come from.</param>
    public FocusRing(ArlecchinoKeymap keymap)
    {
        _keymap = keymap;
    }

    /// <summary>The elements, in the order they were added.</summary>
    public IReadOnlyList<IArlecchinoFocusable> Items => _items;

    /// <summary>Position of the focused element.</summary>
    public int Index => _index;

    /// <summary>The focused element, or <c>null</c> when the ring is empty.</summary>
    public IArlecchinoFocusable? Current => _items.Count == 0 ? null : _items[Math.Clamp(_index, 0, _items.Count - 1)];

    /// <summary>
    /// Whether the ring itself holds the focus. A ring a view owns outright is focused from the
    /// start; one nested in another ring is told when the cursor arrives. It passes that on to
    /// whichever element it left the focus with, so nothing inside an unfocused ring draws as active.
    /// </summary>
    public bool IsFocused
    {
        get => _isFocused;
        set
        {
            _isFocused = value;

            if (Current is { } current)
            {
                current.IsFocused = value;
            }
        }
    }

    /// <summary>Adds an element. The first one added starts focused.</summary>
    /// <param name="item">The element to add.</param>
    public void Add(IArlecchinoFocusable item)
    {
        _items.Add(item);
        item.IsFocused = _isFocused && _items.Count == 1;
    }

    /// <summary>Moves the focus to a particular element, if it belongs to this ring.</summary>
    /// <param name="item">The element to focus.</param>
    public void Focus(IArlecchinoFocusable item)
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
    public ViewRoute Handle(KeyPress key)
    {
        if (_keymap.PreviousField.Matches(key))
        {
            Walk(FocusDirection.Previous);
            return ViewRoute.None;
        }

        if (!_keymap.NextField.Matches(key))
        {
            return Current?.Handle(key).Route ?? ViewRoute.None;
        }

        Walk(FocusDirection.Next);
        return ViewRoute.None;
    }

    /// <summary>
    /// Offers the event to each element and moves the focus to whichever one claims it, so a click
    /// both selects a pane and acts inside it.
    /// </summary>
    /// <param name="mouse">The event, in frame coordinates.</param>
    /// <returns>The route the element asked for, or <see cref="ViewRoute.None"/>.</returns>
    public ViewRoute HandleMouse(MouseEvent mouse) => Claim(mouse).Route;

    /// <summary>
    /// Moves the focus one element along without wrapping, for a ring nested in another one. At the
    /// last element going forward, or the first going back, the step is left to the ring outside, and
    /// this one keeps the place it was left at.
    /// </summary>
    /// <param name="direction">Which way the focus is going.</param>
    /// <returns>Whether the focus moved inside this ring.</returns>
    public bool MoveFocus(FocusDirection direction)
    {
        if (Current?.MoveFocus(direction) == true)
        {
            return true;
        }

        if (direction == FocusDirection.Next && _index + 1 < _items.Count)
        {
            MoveTo(_index + 1);
            return true;
        }

        if (direction != FocusDirection.Previous || _index <= 0)
        {
            return false;
        }

        MoveTo(_index - 1);
        return true;
    }

    /// <summary>
    /// What the focused element wants the hints box to show, asked down the chain: a ring answers for
    /// the ring inside it, which answers for the widget inside that.
    /// </summary>
    /// <returns>The hints of the focused element, empty when there are none.</returns>
    public (string Key, string Description)[] Hints() => Current?.Hints() ?? [];

    FocusResult IArlecchinoFocusable.Handle(KeyPress key) => Current?.Handle(key) ?? FocusResult.Ignored;

    FocusResult IArlecchinoFocusable.HandleMouse(MouseEvent mouse) => Claim(mouse);

    private void Walk(FocusDirection direction)
    {
        if (MoveFocus(direction))
        {
            return;
        }

        if (direction == FocusDirection.Next)
        {
            FocusNext();
            return;
        }

        FocusPrevious();
    }

    private FocusResult Claim(MouseEvent mouse)
    {
        foreach (var item in _items)
        {
            var result = item.HandleMouse(mouse);
            if (!result.WasHandled)
            {
                continue;
            }

            Focus(item);
            return result;
        }

        return FocusResult.Ignored;
    }

    private void MoveTo(int index)
    {
        if (_items.Count == 0)
        {
            return;
        }

        _items[Math.Clamp(_index, 0, _items.Count - 1)].IsFocused = false;
        _index = Math.Clamp(index, 0, _items.Count - 1);
        _items[_index].IsFocused = _isFocused;
    }
}
