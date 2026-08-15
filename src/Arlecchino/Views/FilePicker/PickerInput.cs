using System;
using System.Collections.Generic;
using Arlecchino.Editing;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// What each key and click does in the two panes. Typing narrows the listing rather than jumping to a
/// letter, so the keys that would type are read last.
/// </summary>
internal sealed class PickerInput
{
    private const int PageRows = 10;

    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;
    private readonly PickerListing _listing;
    private readonly PickerPlaces _places;
    private readonly PickerTable _table;
    private readonly Func<PickerEntry, ViewRoute> _open;
    private readonly Action<bool> _focus;

    /// <summary>Reads the picker's input.</summary>
    /// <param name="keymap">Keys to obey.</param>
    /// <param name="keyText">Turns a key press into the character it stands for, for what is typed.</param>
    /// <param name="folder">The folder being listed.</param>
    /// <param name="places">The shortcuts down the left.</param>
    /// <param name="table">The listing on the right.</param>
    /// <param name="open">What opening a row comes to: browsing into it, or picking it.</param>
    /// <param name="focus">Moves the keyboard to the listing when <c>true</c>, to the shortcuts otherwise.</param>
    public PickerInput(
        ArlecchinoKeymap keymap,
        KeyText keyText,
        PickerListing folder,
        PickerPlaces places,
        PickerTable table,
        Func<PickerEntry, ViewRoute> open,
        Action<bool> focus)
    {
        _keymap = keymap;
        _keyText = keyText;
        _listing = folder;
        _places = places;
        _table = table;
        _open = open;
        _focus = focus;
    }

    /// <summary>Reads a key while the shortcuts have the keyboard.</summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns>Whether it was taken.</returns>
    public FocusResult Places(KeyPress key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            _places.Move(-1);
        }
        else if (_keymap.MoveDown.Matches(key))
        {
            _places.Move(1);
        }
        else if (_keymap.MoveRight.Matches(key))
        {
            _focus(true);
        }
        else if (_keymap.Confirm.Matches(key) && _places.Current is { } target)
        {
            GoTo(target);
        }
        else
        {
            return FocusResult.Ignored;
        }

        return FocusResult.Handled;
    }

    /// <summary>Reads a key while the listing has the keyboard.</summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns>Whether it was taken, and where it wants to go.</returns>
    public FocusResult List(KeyPress key)
    {
        var entries = _listing.Matching();

        if (Stepped(key, entries.Count))
        {
            return FocusResult.Handled;
        }

        if (_keymap.MoveLeft.Matches(key))
        {
            Leave();
        }
        else if (_keymap.MoveRight.Matches(key) && entries.Count > 0 && entries[_table.Selected].IsDirectory)
        {
            GoTo(entries[_table.Selected].FullPath);
        }
        else if (_keymap.Erase.Matches(key) && _listing.Filter.Text.Length == 0)
        {
            _listing.Up();
        }
        else if (EraseKeys.Erased(_listing.Filter, _keymap, key))
        {
            _table.Selected = 0;
        }
        else if (_keymap.Confirm.Matches(key) && entries.Count > 0)
        {
            return FocusResult.Navigate(_open(entries[_table.Selected]));
        }
        else if (_keyText.Resolve(key) is { } typed)
        {
            TextEditing.Insert(_listing.Filter, typed);
            _table.Selected = 0;
        }
        else
        {
            return FocusResult.Ignored;
        }

        return FocusResult.Handled;
    }

    /// <summary>Follows the shortcut that was clicked.</summary>
    /// <param name="row">Row on screen, counted from the top of the pane.</param>
    /// <returns>Where to go, which is nowhere.</returns>
    public ViewRoute ClickedPlaces(int row)
    {
        if (_places.ClickedAt(row) is { } target)
        {
            GoTo(target);
        }

        return ViewRoute.None;
    }

    /// <summary>
    /// Acts on a click or a turn of the wheel in the listing. A row is opened only when it was already the
    /// one under the cursor, so the first click picks it out and the second acts on it.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <param name="row">Row on screen, counted from the top of the pane.</param>
    /// <returns>Where to go.</returns>
    public ViewRoute ClickedList(MouseEvent mouse, int row)
    {
        var entries = _listing.Matching();

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                _table.Selected = Math.Max(0, _table.Selected - 1);

                return ViewRoute.None;
            case MouseAction.ScrolledDown:
                _table.Selected = Math.Min(Math.Max(0, entries.Count - 1), _table.Selected + 1);

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

        var wasSelected = index == _table.Selected;

        _table.Selected = index;

        return wasSelected ? _open(entries[index]) : ViewRoute.None;
    }

    private bool Stepped(KeyPress key, int count)
    {
        var last = Math.Max(0, count - 1);

        if (_keymap.MoveUp.Matches(key))
        {
            _table.Selected = Math.Max(0, _table.Selected - 1);
        }
        else if (_keymap.MoveDown.Matches(key))
        {
            _table.Selected = Math.Min(last, _table.Selected + 1);
        }
        else if (_keymap.JumpUp.Matches(key))
        {
            _table.Selected = Math.Max(0, _table.Selected - PageRows);
        }
        else if (_keymap.JumpDown.Matches(key))
        {
            _table.Selected = Math.Min(last, _table.Selected + PageRows);
        }
        else if (_keymap.First.Matches(key))
        {
            _table.Selected = 0;
        }
        else if (_keymap.Last.Matches(key))
        {
            _table.Selected = last;
        }
        else
        {
            return false;
        }

        return true;
    }

    private void Leave()
    {
        if (_listing.Folder.Length == 0)
        {
            _focus(false);

            return;
        }

        _listing.Up();
        _places.SyncTo(_listing.Folder);
        _table.Selected = 0;
    }

    private void GoTo(string path)
    {
        _listing.GoTo(path);
        _places.SyncTo(path);
        _table.Selected = 0;
        _focus(true);
    }
}
