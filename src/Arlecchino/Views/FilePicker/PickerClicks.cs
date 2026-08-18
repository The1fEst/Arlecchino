using System;
using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// What the mouse does in the two panes. A row is acted on only when it was already the one under the
/// cursor, so the first click picks it out and the second opens it.
/// </summary>
internal sealed class PickerClicks
{
    private readonly PickerListing _listing;
    private readonly PickerPlaces _places;
    private readonly PickerTable _table;
    private readonly Func<PickerEntry, ViewRoute> _open;
    private readonly Action<string> _goTo;

    /// <summary>Reads the picker's clicks.</summary>
    /// <param name="listing">The folder being listed.</param>
    /// <param name="places">The shortcuts down the left.</param>
    /// <param name="table">The listing on the right.</param>
    /// <param name="open">What opening a row comes to: browsing into it, or picking it.</param>
    /// <param name="goTo">Browses to a folder, moving the keyboard to the listing.</param>
    public PickerClicks(
        PickerListing listing,
        PickerPlaces places,
        PickerTable table,
        Func<PickerEntry, ViewRoute> open,
        Action<string> goTo)
    {
        _listing = listing;
        _places = places;
        _table = table;
        _open = open;
        _goTo = goTo;
    }

    /// <summary>Follows the shortcut that was clicked.</summary>
    /// <param name="row">Row on screen, counted from the top of the pane.</param>
    /// <returns>Where to go, which is nowhere.</returns>
    public ViewRoute Places(int row)
    {
        if (_places.ClickedAt(row) is { } target)
        {
            _goTo(target);
        }

        return ViewRoute.None;
    }

    /// <summary>Acts on a click or a turn of the wheel in the listing.</summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <param name="row">Row on screen, counted from the top of the pane.</param>
    /// <returns>Where to go.</returns>
    public ViewRoute List(MouseEvent mouse, int row)
    {
        var entries = _listing.Matching();

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                _table.SelectedIndex = Math.Max(0, _table.SelectedIndex - 1);

                return ViewRoute.None;
            case MouseAction.ScrolledDown:
                _table.SelectedIndex = Math.Min(Math.Max(0, entries.Count - 1), _table.SelectedIndex + 1);

                return ViewRoute.None;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left:
                return Clicked(_table.RowAt(row), entries);
            default:
                return ViewRoute.None;
        }
    }

    private ViewRoute Clicked(int index, List<PickerEntry> entries)
    {
        if (index < 0 || index >= entries.Count)
        {
            return ViewRoute.None;
        }

        var wasSelected = index == _table.SelectedIndex;

        _table.SelectedIndex = index;

        return wasSelected ? _open(entries[index]) : ViewRoute.None;
    }
}
