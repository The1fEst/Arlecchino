using System;
using Arlecchino.Editing;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// What each key does in the two panes. Typing narrows the listing rather than jumping to a
/// letter, so the keys that would type are read last.
/// </summary>
internal sealed class PickerInput
{
    private const int PageRows = 10;

    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;
    private readonly IArlecchinoTerminal _terminal;
    private readonly PickerListing _listing;
    private readonly PickerPlaces _places;
    private readonly PickerTable _table;
    private readonly Func<PickerEntry, ViewRoute> _open;
    private readonly Action<bool> _focus;

    /// <summary>Reads the picker's input.</summary>
    /// <param name="keymap">Keys to obey.</param>
    /// <param name="keyText">Turns a key press into the character it stands for, for what is typed.</param>
    /// <param name="terminal">Reached for the clipboard when the filter is copied or cut.</param>
    /// <param name="folder">The folder being listed.</param>
    /// <param name="places">The shortcuts down the left.</param>
    /// <param name="table">The listing on the right.</param>
    /// <param name="open">What opening a row comes to: browsing into it, or picking it.</param>
    /// <param name="focus">Moves the keyboard to the listing when <c>true</c>, to the shortcuts otherwise.</param>
    public PickerInput(
        ArlecchinoKeymap keymap,
        KeyText keyText,
        IArlecchinoTerminal terminal,
        PickerListing folder,
        PickerPlaces places,
        PickerTable table,
        Func<PickerEntry, ViewRoute> open,
        Action<bool> focus)
    {
        _keymap = keymap;
        _keyText = keyText;
        _terminal = terminal;
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

        if (_keymap.Confirm.Matches(key) && entries.Count > 0)
        {
            return FocusResult.Navigate(_open(entries[_table.SelectedIndex]));
        }

        if (_keymap.Erase.Matches(key) && _listing.Filter.Text.Length == 0)
        {
            _listing.Up();

            return FocusResult.Handled;
        }

        if (Filtering(key))
        {
            _table.SelectedIndex = 0;

            return FocusResult.Handled;
        }

        if (Ended(key, entries.Count))
        {
            return FocusResult.Handled;
        }

        if (_keymap.MoveLeft.Matches(key))
        {
            Leave();
        }
        else if (_keymap.MoveRight.Matches(key) && entries.Count > 0 && entries[_table.SelectedIndex].IsDirectory)
        {
            GoTo(entries[_table.SelectedIndex].FullPath);
        }
        else if (_keyText.Resolve(key) is { } typed)
        {
            TextEditing.Insert(_listing.Filter, typed);
            _table.SelectedIndex = 0;
        }
        else
        {
            return FocusResult.Ignored;
        }

        return FocusResult.Handled;
    }

    /// <summary>
    /// Puts pasted text into the filter, which is the one thing in the picker that is typed into.
    /// </summary>
    /// <param name="text">What was pasted.</param>
    public void Paste(string text)
    {
        TextEditing.InsertText(_listing.Filter, PastedText.FirstLine(text));
        _table.SelectedIndex = 0;
    }

    /// <summary>
    /// The keys that edit what is being filtered by rather than walk the listing. They are read only once
    /// something is being filtered by: with nothing typed the rows are what the keys are for.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the filter took the key.</returns>
    private bool Filtering(KeyPress key) =>
        _listing.Filter.Text.Length > 0 &&
        EntryKeys.Handled(_listing.Filter, _keymap, _terminal.CopyToClipboard, key);

    /// <summary>Browses to a folder, taking the shortcuts and the cursor with it.</summary>
    /// <param name="path">The folder to look at.</param>
    public void GoTo(string path)
    {
        _listing.GoTo(path);
        _places.SyncTo(path);
        _table.SelectedIndex = 0;
        _focus(true);
    }

    private bool Stepped(KeyPress key, int count)
    {
        var last = Math.Max(0, count - 1);

        if (_keymap.MoveUp.Matches(key))
        {
            _table.SelectedIndex = Math.Max(0, _table.SelectedIndex - 1);
        }
        else if (_keymap.MoveDown.Matches(key))
        {
            _table.SelectedIndex = Math.Min(last, _table.SelectedIndex + 1);
        }
        else if (_keymap.JumpUp.Matches(key))
        {
            _table.SelectedIndex = Math.Max(0, _table.SelectedIndex - PageRows);
        }
        else if (_keymap.JumpDown.Matches(key))
        {
            _table.SelectedIndex = Math.Min(last, _table.SelectedIndex + PageRows);
        }
        else
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// The keys that go to either end of the listing. They belong to the caret instead while something is
    /// being filtered by, so they are read after the filter has had the key.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    /// <param name="count">How many rows there are.</param>
    /// <returns><c>true</c> when the key was one of these.</returns>
    private bool Ended(KeyPress key, int count)
    {
        if (_keymap.First.Matches(key))
        {
            _table.SelectedIndex = 0;
        }
        else if (_keymap.Last.Matches(key))
        {
            _table.SelectedIndex = Math.Max(0, count - 1);
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
        _table.SelectedIndex = 0;
    }
}
