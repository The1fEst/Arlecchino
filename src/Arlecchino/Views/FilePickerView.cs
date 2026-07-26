using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;

namespace Arlecchino.Views;

/// <summary>
/// Browses the file system: shortcuts on the left, the current folder on the right. It is a view
/// rather than a dialog because it needs the whole screen, which is also why the request that opened
/// it has to say where to return to. It is registered automatically, so an application only has to
/// fill in <see cref="ArlecchinoState.FilePicker"/> and navigate here.
/// </summary>
internal class FilePickerView : IArlecchinoView
{
    /// <summary>The route it answers to.</summary>
    public const string Route = "FilePicker";

    private const int SidebarWidth = 22;
    private const int PageRows = 10;


    private sealed record Entry(string Name, string FullPath, bool IsDirectory, DateTime Modified, long Length, bool IsVolume);

    private sealed record SidebarRow(string Label, string? Path, string Icon);

    private readonly ArlecchinoKeymap _keymap;
    private readonly ArlecchinoState _state;
    private readonly Surface _surface;
    private readonly KeyText _keyText;
    private readonly ArlecchinoStrings.FilePickerStrings _strings;
    private readonly FilePickerRequest _request;
    private readonly List<SidebarRow> _sidebar;

    private readonly Stack<string> _back = new();
    private readonly Stack<string> _forward = new();

    private readonly FocusRing _panes;
    private readonly FocusablePane _sidebarPane;
    private readonly FocusablePane _list;
    private SurfaceRegion _sidebarRows;
    private SurfaceRegion _listRows;
    private int _sidebarFirstVisible;
    private int _listFirstVisible;
    private string _path;
    private string _filter = "";
    private string _error = "";
    private List<Entry> _entries = [];
    private int _selected;
    private int _sidebarSelected;

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
        _keyText = keyText;
        _keymap = options.Keymap;
        _strings = options.Strings.FilePicker;
        _request = state.FilePicker ??
                   new FilePickerRequest(_strings.Title(), PickFolder: true, "", ViewRoute.None, static _ => { });

        _sidebarPane = new(HandleSidebar);
        _list = new(HandleList);
        _panes = new(_keymap);
        _panes.Add(_list);
        _panes.Add(_sidebarPane);

        _sidebar = BuildSidebar();
        _path = Directory.Exists(_request.InitialPath) ? _request.InitialPath : "";
        LoadEntries();
        SyncSidebarSelection();
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
        DrawToolbar(toolbar);

        var (browser, status) = rest.SplitTop(rest.Height - 2);
        var inside = browser.Border(Theme.Muted);
        var (sidebar, list) = inside.SplitLeft(SidebarWidth);

        _sidebarRows = sidebar;
        _listRows = list.Inset(new Margin(1, 1, 0, 0));

        DrawSplitBorder(browser);
        DrawSidebar(sidebar);
        DrawList(list.Inset(new Margin(1, 0, 0, 0)));
        DrawStatus(status);
    }

    /// <summary>
    /// Moves focus to whichever pane was clicked and handles the click there, so pointing at a pane is
    /// enough to start working in it.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>Where to go, or <see cref="ViewRoute.None"/> to stay put.</returns>
    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        if (_sidebarRows.Contains(mouse.Row, mouse.Column))
        {
            _panes.Focus(_sidebarPane);
            return ClickSidebar(mouse);
        }

        if (_listRows.Contains(mouse.Row, mouse.Column))
        {
            _panes.Focus(_list);
            return ClickList(mouse);
        }

        return ViewRoute.None;
    }

    private ViewRoute ClickSidebar(MouseEvent mouse)
    {
        if (mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left)
        {
            return ViewRoute.None;
        }

        var (row, _) = _sidebarRows.ToLocal(mouse.Row, mouse.Column);
        var index = _sidebarFirstVisible + row;

        if (index < 0 || index >= _sidebar.Count || _sidebar[index].Path is not { } target)
        {
            return ViewRoute.None;
        }

        _sidebarSelected = index;
        NavigateTo(target);
        _panes.Focus(_list);
        return ViewRoute.None;
    }

    private ViewRoute ClickList(MouseEvent mouse)
    {
        var entries = GetMatchingEntries();

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                _selected = Math.Max(0, _selected - 1);
                return ViewRoute.None;
            case MouseAction.ScrolledDown:
                _selected = Math.Min(Math.Max(0, entries.Count - 1), _selected + 1);
                return ViewRoute.None;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left:
                var (row, _) = _listRows.ToLocal(mouse.Row, mouse.Column);
                var index = _listFirstVisible + row;

                if (index < 0 || index >= entries.Count)
                {
                    return ViewRoute.None;
                }

                var wasSelected = index == _selected;
                _selected = index;
                return wasSelected ? Open(entries[index]) : ViewRoute.None;
            default:
                return ViewRoute.None;
        }
    }

    private static void DrawSplitBorder(SurfaceRegion browser)
    {
        var column = SidebarWidth + 1;

        browser.Write(0, column, "┬", Theme.Muted);
        browser.Write(browser.Height - 1, column, "┴", Theme.Muted);

        for (var row = 1; row < browser.Height - 1; row++)
        {
            browser.Write(row, column, "│", Theme.Muted);
        }
    }

    /// <summary>
    /// Cancels, picks the folder being browsed, or hands the key to whichever pane has focus.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>Where to go, or <see cref="ViewRoute.None"/> to stay put.</returns>
    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            return Cancel();
        }

        if (_keymap.PickCurrentFolder.Matches(key) && _request.PickFolder && _path.Length > 0)
        {
            return Pick(_path);
        }

        return _panes.Handle(key);
    }

    /// <summary>No hint box: the picker draws its own key line as part of its layout.</summary>
    /// <returns>An empty list.</returns>
    public (string Key, string Description)[] Hints() => [];

    private FocusResult HandleSidebar(ConsoleKeyInfo key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            MoveSidebar(-1);
        }
        else if (_keymap.MoveDown.Matches(key))
        {
            MoveSidebar(1);
        }
        else if (_keymap.MoveRight.Matches(key))
        {
            _panes.Focus(_list);
        }
        else if (_keymap.Confirm.Matches(key) && _sidebar[_sidebarSelected].Path is { } target)
        {
            NavigateTo(target);
            _panes.Focus(_list);
        }
        else
        {
            return FocusResult.Ignored;
        }

        return FocusResult.Handled;
    }

    private FocusResult HandleList(ConsoleKeyInfo key)
    {
        var entries = GetMatchingEntries();

        if (_keymap.MoveUp.Matches(key))
        {
            _selected = Math.Max(0, _selected - 1);
        }
        else if (_keymap.MoveDown.Matches(key))
        {
            _selected = Math.Min(Math.Max(0, entries.Count - 1), _selected + 1);
        }
        else if (_keymap.JumpUp.Matches(key))
        {
            _selected = Math.Max(0, _selected - PageRows);
        }
        else if (_keymap.JumpDown.Matches(key))
        {
            _selected = Math.Min(Math.Max(0, entries.Count - 1), _selected + PageRows);
        }
        else if (_keymap.First.Matches(key))
        {
            _selected = 0;
        }
        else if (_keymap.Last.Matches(key))
        {
            _selected = Math.Max(0, entries.Count - 1);
        }
        else if (_keymap.MoveLeft.Matches(key))
        {
            if (_path.Length == 0)
            {
                _panes.Focus(_sidebarPane);
            }
            else
            {
                GoUp();
            }
        }
        else if (_keymap.MoveRight.Matches(key) && entries.Count > 0 && entries[_selected].IsDirectory)
        {
            NavigateTo(entries[_selected].FullPath);
        }
        else if (_keymap.Erase.Matches(key))
        {
            if (_filter.Length > 0)
            {
                _filter = _filter[..^1];
                _selected = 0;
            }
            else
            {
                GoUp();
            }
        }
        else if (_keymap.Confirm.Matches(key) && entries.Count > 0)
        {
            return FocusResult.Navigate(Open(entries[_selected]));
        }
        else if (_keyText.Resolve(key) is { } typed)
        {
            _filter += typed;
            _selected = 0;
        }
        else
        {
            return FocusResult.Ignored;
        }

        return FocusResult.Handled;
    }

    private void DrawToolbar(SurfaceRegion toolbar)
    {
        toolbar.Write(0, 0, "◀", _back.Count > 0 ? Theme.Accent : Theme.Muted);
        toolbar.Write(0, 2, "▶", _forward.Count > 0 ? Theme.Accent : Theme.Muted);

        var folder = _path.Length == 0 ? _strings.Drives() : (Path.GetFileName(_path) is { Length: > 0 } name ? name : _path);
        toolbar.Write(0, 5, folder, Theme.Header);

        var mode = _request.PickFolder ? _strings.FolderMode() : _strings.FileMode();
        var title = $"{_request.Title} ({mode})";
        var titleColumn = 5 + TextWidth.Of(folder) + 3;
        if (titleColumn + TextWidth.Of(title) < toolbar.Width)
        {
            toolbar.Write(0, titleColumn, title, Theme.Muted);
        }

        var search = $"{_strings.Search()}: {_filter}▏";
        var searchColumn = Math.Max(titleColumn + TextWidth.Of(title) + 2, toolbar.Width - TextWidth.Of(search));
        toolbar.Write(0, searchColumn, search, _filter.Length > 0 ? Theme.Info : Theme.Muted);
    }

    private void DrawSidebar(SurfaceRegion sidebar)
    {
        var start = Math.Clamp(_sidebarSelected - sidebar.Height / 2, 0, Math.Max(0, _sidebar.Count - sidebar.Height));
        _sidebarFirstVisible = start;

        for (var i = 0; i < sidebar.Height && start + i < _sidebar.Count; i++)
        {
            var item = _sidebar[start + i];

            if (item.Path is null)
            {
                sidebar.Write(i, 0, Pad(item.Label, sidebar.Width), Theme.Muted);
                continue;
            }

            var style = start + i == _sidebarSelected
                ? _sidebarPane.IsFocused ? Theme.ActiveSelected : Theme.Selected
                : Theme.Default;

            sidebar.Write(i, 0, Pad($" {item.Icon} {item.Label}", sidebar.Width), style);
        }
    }

    private void DrawList(SurfaceRegion list)
    {
        if (list.IsEmpty)
        {
            return;
        }

        var (nameWidth, dateWidth, sizeWidth, kindWidth) = Columns(list.Width);

        list.Write(0, 0, Header(nameWidth, dateWidth, sizeWidth, kindWidth), Theme.TableHeader);

        var entries = GetMatchingEntries();
        _selected = Math.Clamp(_selected, 0, Math.Max(0, entries.Count - 1));

        var rows = list.Height - 1;

        if (_error.Length > 0)
        {
            list.Write(1, 0, Clip(_error, list.Width), Theme.Error);
            return;
        }

        if (entries.Count == 0)
        {
            list.Write(1, 0, Clip(_strings.ItemCount(0), list.Width), Theme.Muted);
            return;
        }

        var start = Math.Clamp(_selected - rows / 2, 0, Math.Max(0, entries.Count - rows));
        _listFirstVisible = start;

        for (var i = 0; i < rows && start + i < entries.Count; i++)
        {
            var entry = entries[start + i];

            var style = start + i == _selected
                ? _list.IsFocused ? Theme.ActiveSelected : Theme.Selected
                : entry.IsDirectory
                    ? Theme.Info
                    : Theme.Default;

            list.Write(1 + i, 0, Pad(Row(entry, nameWidth, dateWidth, sizeWidth, kindWidth), list.Width), style);
        }
    }

    private void DrawStatus(SurfaceRegion status)
    {
        var entries = GetMatchingEntries();
        var count = _strings.ItemCount(entries.Count);
        status.Write(0, 1, count, Theme.Muted);

        var legend = string.Join("   ",
            $"{_keymap.MoveUp}{_keymap.MoveDown} {_strings.HintMove()}",
            $"{_keymap.MoveRight} {_strings.HintOpen()}",
            $"{_keymap.MoveLeft} {_strings.HintUp()}",
            $"{_keymap.NextField} {_strings.HintPlaces()}",
            _request.PickFolder
                ? $"{_keymap.PickCurrentFolder} {_strings.HintPickCurrentFolder()}"
                : $"{_keymap.Confirm} {_strings.HintOpenFolderOrPickFile()}",
            $"{_keymap.Cancel} {_strings.HintCancel()}");

        var column = status.Width - TextWidth.Of(legend) - 1;
        if (column > TextWidth.Of(count) + 3)
        {
            status.Write(0, column, legend, Theme.Muted);
        }
    }

    private string Header(int nameWidth, int dateWidth, int sizeWidth, int kindWidth)
    {
        var line = "  " + Pad(_strings.ColumnName(), nameWidth);

        if (dateWidth > 0)
        {
            line += Pad(_strings.ColumnDateModified(), dateWidth);
        }

        if (sizeWidth > 0)
        {
            line += PadLeft(_strings.ColumnSize(), sizeWidth);
        }

        if (kindWidth > 0)
        {
            line += "  " + Pad(_strings.ColumnKind(), kindWidth);
        }

        return line;
    }

    private string Row(Entry entry, int nameWidth, int dateWidth, int sizeWidth, int kindWidth)
    {
        var line = (entry.IsDirectory ? "▸ " : "  ") + Pad(entry.Name, nameWidth);

        if (dateWidth > 0)
        {
            line += Pad(entry.Modified == default ? "" : _strings.DateModified(entry.Modified), dateWidth);
        }

        if (sizeWidth > 0)
        {
            line += PadLeft(entry.IsDirectory ? "--" : _strings.Size(entry.Length), sizeWidth);
        }

        if (kindWidth > 0)
        {
            var kind = entry.IsVolume
                ? _strings.KindVolume()
                : entry.IsDirectory
                    ? _strings.KindFolder()
                    : _strings.KindOf(Path.GetExtension(entry.Name));

            line += "  " + Pad(kind, kindWidth);
        }

        return line;
    }

    private static (int Name, int Date, int Size, int Kind) Columns(int width)
    {
        const int date = 22;
        const int size = 10;
        const int kind = 16;

        if (width >= 2 + 24 + date + size + kind + 2)
        {
            return (width - 2 - date - size - kind - 2, date, size, kind);
        }

        if (width >= 2 + 20 + date + size)
        {
            return (width - 2 - date - size, date, size, 0);
        }

        if (width >= 2 + 16 + size)
        {
            return (width - 2 - size, 0, size, 0);
        }

        return (Math.Max(1, width - 2), 0, 0, 0);
    }

    private static string Pad(string text, int width) => TextWidth.PadRight(Clip(text, width), width);

    private static string PadLeft(string text, int width) => TextWidth.PadLeft(Clip(text, width), width);

    private static string Clip(string text, int width) =>
        width <= 0 ? "" :
        TextWidth.Of(text) > width ? TextWidth.Truncate(text, Math.Max(0, width - 1)) + "…" : text;

    private ViewRoute Open(Entry entry)
    {
        if (!entry.IsDirectory)
        {
            return _request.PickFolder ? ViewRoute.None : Pick(entry.FullPath);
        }

        NavigateTo(entry.FullPath);
        return ViewRoute.None;
    }

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

    private void MoveSidebar(int delta)
    {
        var next = _sidebarSelected;
        for (var i = 0; i < _sidebar.Count; i++)
        {
            next += delta;
            if (next < 0 || next >= _sidebar.Count)
            {
                return;
            }

            if (_sidebar[next].Path is null)
            {
                continue;
            }

            _sidebarSelected = next;
            return;
        }
    }

    private void NavigateTo(string path)
    {
        _back.Push(_path);
        _forward.Clear();
        SetPath(path);
    }

    private void GoUp()
    {
        if (_path.Length == 0)
        {
            return;
        }

        var parent = Path.GetDirectoryName(_path);
        NavigateTo(parent ?? "");
    }

    private void SetPath(string path)
    {
        _path = path;
        _filter = "";
        _selected = 0;
        LoadEntries();
        SyncSidebarSelection();
    }

    private void SyncSidebarSelection()
    {
        for (var i = 0; i < _sidebar.Count; i++)
        {
            if (_sidebar[i].Path is { } path && string.Equals(path, _path, StringComparison.OrdinalIgnoreCase))
            {
                _sidebarSelected = i;
                return;
            }
        }
    }

    private List<SidebarRow> BuildSidebar()
    {
        var rows = new List<SidebarRow> { new(_strings.Favorites(), null, "") };

        foreach (var place in _request.Places)
        {
            rows.Add(new(place.Name, place.Path, place.Icon.Length > 0 ? place.Icon : "▪"));
        }

        AddSpecial(rows, Environment.SpecialFolder.Desktop, "▪");
        AddSpecial(rows, Environment.SpecialFolder.MyDocuments, "▪");
        AddSpecial(rows, Environment.SpecialFolder.UserProfile, "▪", "Downloads");
        AddSpecial(rows, Environment.SpecialFolder.MyPictures, "▪");
        AddSpecial(rows, Environment.SpecialFolder.MyMusic, "▪");
        AddSpecial(rows, Environment.SpecialFolder.MyVideos, "▪");

        rows.Add(new(_strings.Locations(), null, ""));

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (Directory.Exists(home))
        {
            rows.Add(new(Environment.UserName, home, "⌂"));
        }

        rows.Add(new(_strings.Drives(), "", "▣"));

        foreach (var drive in DriveInfo.GetDrives().Where(static drive => drive.IsReady))
        {
            rows.Add(new(drive.Name.TrimEnd(Path.DirectorySeparatorChar), drive.Name, "▤"));
        }

        _sidebarSelected = rows.FindIndex(static row => row.Path is not null);
        return rows;
    }

    private static void AddSpecial(List<SidebarRow> rows, Environment.SpecialFolder folder, string icon, string? childName = null)
    {
        var path = Environment.GetFolderPath(folder);
        if (path.Length == 0)
        {
            return;
        }

        if (childName is not null)
        {
            path = Path.Combine(path, childName);
        }

        if (!Directory.Exists(path))
        {
            return;
        }

        var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
        if (name.Length == 0 || rows.Any(row => string.Equals(row.Path, path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        rows.Add(new(name, path, icon));
    }

    private void LoadEntries()
    {
        _error = "";

        try
        {
            if (_path.Length == 0)
            {
                _entries = DriveInfo.GetDrives()
                    .Where(static drive => drive.IsReady)
                    .Select(static drive => new Entry(
                        drive.VolumeLabel.Length > 0
                            ? $"{drive.VolumeLabel} ({drive.Name.TrimEnd(Path.DirectorySeparatorChar)})"
                            : drive.Name,
                        drive.Name,
                        IsDirectory: true,
                        default,
                        0,
                        IsVolume: true))
                    .ToList();
                return;
            }

            var directories = new DirectoryInfo(_path).EnumerateDirectories()
                .Select(static dir => new Entry(dir.Name, dir.FullName, true, SafeTime(dir), 0, false))
                .OrderBy(static entry => entry.Name, StringComparer.OrdinalIgnoreCase);

            var files = new DirectoryInfo(_path).EnumerateFiles()
                .Where(file => _request.FileFilter is null || _request.FileFilter(file.FullName))
                .Select(static file => new Entry(file.Name, file.FullName, false, SafeTime(file), SafeLength(file), false))
                .OrderBy(static entry => entry.Name, StringComparer.OrdinalIgnoreCase);

            _entries = directories.Concat(files).ToList();
        }
        catch (Exception e)
        {
            _entries = [];
            _error = e.Message;
        }
    }

    private static DateTime SafeTime(FileSystemInfo info)
    {
        try
        {
            return info.LastWriteTime;
        }
        catch (Exception)
        {
            return default;
        }
    }

    private static long SafeLength(FileInfo info)
    {
        try
        {
            return info.Length;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    private List<Entry> GetMatchingEntries()
    {
        if (_filter.Length == 0)
        {
            return _entries;
        }

        return _entries
            .Where(entry => entry.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
