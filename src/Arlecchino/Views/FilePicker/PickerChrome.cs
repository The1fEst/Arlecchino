using System;
using System.IO;
using Arlecchino.Editing;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.State;
using Arlecchino.Widgets.Text;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// What is drawn around the two panes: the row above with the folder and what is typed, the row below with
/// the count and the keys, and the line between the panes.
/// </summary>
internal sealed class PickerChrome
{
    private readonly ArlecchinoStrings.FilePickerStrings _strings;
    private readonly ArlecchinoKeymap _keymap;
    private readonly FilePickerRequest _request;

    /// <summary>Draws the chrome of one picker.</summary>
    /// <param name="strings">The words the picker says things in.</param>
    /// <param name="keymap">The keys, which the row of hints names.</param>
    /// <param name="request">What was asked for, which is what the title says.</param>
    public PickerChrome(ArlecchinoStrings.FilePickerStrings strings, ArlecchinoKeymap keymap, FilePickerRequest request)
    {
        _strings = strings;
        _keymap = keymap;
        _request = request;
    }

    /// <summary>Draws the row above the panes.</summary>
    /// <param name="toolbar">The row to draw on.</param>
    /// <param name="path">The folder being listed.</param>
    /// <param name="filter">Whatever has been typed to narrow it, with its caret and selection.</param>
    public void Toolbar(SurfaceRegion toolbar, string path, ITextEntry filter)
    {
        var folder = path.Length == 0
            ? _strings.Drives()
            : Path.GetFileName(path) is { Length: > 0 } name
                ? name
                : path;

        toolbar.Write(0, 0, folder, Theme.Header);

        var mode = _request.PickFolder ? _strings.FolderMode() : _strings.FileMode();
        var title = $"{_request.Title} ({mode})";
        var titleColumn = 5 + TextWidth.Of(folder) + 3;

        if (titleColumn + TextWidth.Of(title) < toolbar.Width)
        {
            toolbar.Write(0, titleColumn, title, Theme.Secondary);
        }

        var label = $"{_strings.Search()}:";
        var searchWidth = TextWidth.Of(label) + TextWidth.Of(filter.Text) + 2;
        var searchColumn = Math.Max(titleColumn + TextWidth.Of(title) + 2, toolbar.Width - searchWidth);
        var style = filter.Text.Length > 0 ? Theme.Info : Theme.Secondary;

        toolbar.Write(0, searchColumn, label, style);
        EntryRow.Draw(
            toolbar,
            0,
            searchColumn + TextWidth.Of(label) + 1,
            Math.Max(0, toolbar.Width - searchColumn - TextWidth.Of(label) - 1),
            filter,
            new(style, Theme.Selection, Theme.Caret));
    }

    /// <summary>Draws the row below the panes.</summary>
    /// <param name="status">The row to draw on.</param>
    /// <param name="count">How many rows the listing has.</param>
    public void Status(SurfaceRegion status, int count)
    {
        var caption = _strings.ItemCount(count);

        status.Write(0, 1, caption, Theme.Secondary);

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

        if (column > TextWidth.Of(caption) + 3)
        {
            status.Write(0, column, legend, Theme.Secondary);
        }
    }

    /// <summary>Draws the line between the shortcuts and the listing.</summary>
    /// <param name="browser">The bordered region holding both panes.</param>
    /// <param name="sidebarWidth">How wide the shortcuts are.</param>
    public static void SplitBorder(SurfaceRegion browser, int sidebarWidth)
    {
        var column = sidebarWidth + 1;

        browser.Write(0, column, "┬", Theme.Secondary);
        browser.Write(browser.Height - 1, column, "┴", Theme.Secondary);

        for (var row = 1; row < browser.Height - 1; row++)
        {
            browser.Write(row, column, "│", Theme.Secondary);
        }
    }
}
