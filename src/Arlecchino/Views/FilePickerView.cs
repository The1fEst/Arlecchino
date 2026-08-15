using System.IO;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.State;
using Arlecchino.Views.FilePicker;

namespace Arlecchino.Views;

/// <summary>
/// Browses the file system: shortcuts on the left, the current folder on the right. It is registered
/// already, so an application fills in <see cref="ArlecchinoState.FilePicker"/> and navigates here.
/// </summary>
/// <seealso cref="PickerListing"/>
/// <seealso cref="PickerPlaces"/>
/// <seealso cref="PickerInput"/>
internal sealed class FilePickerView : IArlecchinoView
{
    /// <summary>The route it answers to.</summary>
    public const string Route = "FilePicker";

    private const int SidebarWidth = 22;

    private readonly ArlecchinoKeymap _keymap;
    private readonly ArlecchinoState _state;
    private readonly Surface _surface;
    private readonly FilePickerRequest _request;

    private readonly PickerListing _listing;
    private readonly PickerPlaces _places;
    private readonly PickerTable _table;
    private readonly PickerChrome _chrome;
    private readonly PickerInput _input;

    private readonly FocusRing _panes;
    private readonly FocusablePane _placesPane;
    private readonly FocusablePane _listPane;

    private SurfaceRegion _placesRows;
    private SurfaceRegion _listRows;

    /// <summary>
    /// Creates the view and reads the folder it opens on. Without a request in the state it falls back
    /// to a harmless folder picker that discards what is chosen, so navigating here by mistake cannot
    /// bring the application down.
    /// </summary>
    /// <param name="state">Holds the request that opened the picker.</param>
    /// <param name="surface">The cell grid to draw into.</param>
    /// <param name="keyText">Turns a key press into the character it stands for, for the filter.</param>
    /// <param name="options">Supplies the keymap and the wording.</param>
    public FilePickerView(ArlecchinoState state, Surface surface, KeyText keyText, ArlecchinoOptions options)
    {
        _state = state;
        _surface = surface;
        _keymap = options.Keymap;

        var strings = options.Strings.FilePicker;

        _request = state.FilePicker ??
                   new FilePickerRequest(strings.Title(), PickFolder: true, "", ViewRoute.None, static _ => { });

        _listing = new(_request, _request.InitialPath);
        _places = new(_request, strings);
        _table = new(strings);
        _chrome = new(strings, _keymap, _request);
        _input = new(_keymap, keyText, _listing, _places, _table, Open, Focus);

        _placesPane = new(_input.Places);
        _listPane = new(_input.List);
        _panes = new(_keymap);
        _panes.Add(_listPane);
        _panes.Add(_placesPane);

        _table.Selected = _listing.IndexOf(_request.InitialPath) is var row and >= 0 ? row : 0;
        _places.SyncTo(_listing.Folder);
    }

    /// <summary>Draws the two panes, or nothing at all when the terminal is too small for them.</summary>
    public void Draw()
    {
        var frame = _surface.Frame.Inset(new Margin(2, 1, 3, 2));

        if (frame.Width < 24 || frame.Height < 6)
        {
            return;
        }

        var (toolbar, rest) = frame.SplitTop(2);

        _chrome.Toolbar(toolbar, _listing.Folder, _listing.Filter.Text);

        var (browser, status) = rest.SplitTop(rest.Height - 2);
        var inside = browser.Border(Theme.Muted);
        var (sidebar, list) = inside.SplitLeft(SidebarWidth);

        _placesRows = sidebar;
        _listRows = list.Inset(new Margin(1, 1, 0, 0));

        PickerChrome.SplitBorder(browser, SidebarWidth);

        _places.Draw(sidebar, _placesPane.IsFocused);
        _table.Draw(list.Inset(new Margin(1, 0, 0, 0)), _listing.Matching(), _listing.Error, _listPane.IsFocused);
        _chrome.Status(status, _listing.Matching().Count);
    }

    /// <summary>
    /// Moves focus to whichever pane was clicked and handles the click there, so pointing at a pane is
    /// enough to start working in it.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>Where to go, or <see cref="ViewRoute.None"/> to stay put.</returns>
    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        if (_placesRows.Contains(mouse.Row, mouse.Column))
        {
            _panes.Focus(_placesPane);

            var (row, _) = _placesRows.ToLocal(mouse.Row, mouse.Column);

            return mouse is { Action: MouseAction.Pressed, Button: MouseButton.Left }
                ? _input.ClickedPlaces(row)
                : ViewRoute.None;
        }

        if (!_listRows.Contains(mouse.Row, mouse.Column))
        {
            return ViewRoute.None;
        }

        _panes.Focus(_listPane);

        var (listRow, _) = _listRows.ToLocal(mouse.Row, mouse.Column);

        return _input.ClickedList(mouse, listRow);
    }

    /// <summary>
    /// Cancels, picks the folder being browsed, or hands the key to whichever pane has focus.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>Where to go, or <see cref="ViewRoute.None"/> to stay put.</returns>
    public ViewRoute Handle(KeyPress key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            return Cancel();
        }

        if (_keymap.PickCurrentFolder.Matches(key) && _request.PickFolder && _listing.Folder.Length > 0)
        {
            return Pick(_listing.Folder);
        }

        return _panes.Handle(key);
    }

    /// <summary>No hint box: the picker draws its own key line as part of its layout.</summary>
    /// <returns>An empty list.</returns>
    public (string Key, string Description)[] Hints() => [];

    /// <summary>Browses into a folder, or picks a file when files are what was asked for.</summary>
    /// <param name="entry">The row that was opened.</param>
    /// <returns>Where to go.</returns>
    private ViewRoute Open(PickerEntry entry)
    {
        if (!entry.IsDirectory)
        {
            return _request.PickFolder ? ViewRoute.None : Pick(entry.FullPath);
        }

        _listing.GoTo(entry.FullPath);
        _places.SyncTo(_listing.Folder);
        _table.Selected = 0;

        return ViewRoute.None;
    }

    /// <summary>Moves the keyboard between the two panes.</summary>
    /// <param name="toList"><c>true</c> for the listing, <c>false</c> for the shortcuts.</param>
    private void Focus(bool toList) => _panes.Focus(toList ? _listPane : _placesPane);

    private ViewRoute Pick(string path)
    {
        _state.FilePicker = null;
        _state.PickerLastFolder = Directory.Exists(path) ? path : Path.GetDirectoryName(path) ?? "";

        _request.OnPicked(path);

        return _request.ReturnView;
    }

    private ViewRoute Cancel()
    {
        _state.FilePicker = null;

        return _request.ReturnView;
    }
}
