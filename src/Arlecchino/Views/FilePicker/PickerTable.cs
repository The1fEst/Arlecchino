using System;
using System.Collections.Generic;
using System.IO;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// The folder listed as a table: the name, when it was written to, the size and the kind. The columns are
/// dropped from the right as the terminal narrows, the name being the one that always stays.
/// </summary>
internal sealed class PickerTable
{
    private readonly ArlecchinoStrings.FilePickerStrings _strings;

    /// <summary>Draws the listing in the wording given.</summary>
    /// <param name="strings">The words the picker says things in.</param>
    public PickerTable(ArlecchinoStrings.FilePickerStrings strings)
    {
        _strings = strings;
    }

    /// <summary>Which row the cursor is on.</summary>
    public int SelectedIndex { get; set; }

    /// <summary>The first row drawn, since a long listing only shows a window of it.</summary>
    public int FirstVisible { get; private set; }

    /// <summary>Which row of the listing a row on screen holds.</summary>
    /// <param name="row">Row on screen, counted from the top of the pane.</param>
    /// <returns>The row of the listing.</returns>
    public int RowAt(int row) => FirstVisible + row;

    /// <summary>Draws the table, scrolled to keep the row under the cursor in view.</summary>
    /// <param name="list">The pane to draw in.</param>
    /// <param name="entries">The rows to draw.</param>
    /// <param name="error">Why the folder could not be read, drawn in place of the rows.</param>
    /// <param name="focused">Whether the pane has the keyboard.</param>
    public void Draw(SurfaceRegion list, IReadOnlyList<PickerEntry> entries, string error, bool focused)
    {
        if (list.IsEmpty)
        {
            return;
        }

        var widths = Columns(list.Width);

        list.Write(0, 0, Header(widths), Theme.TableHeader);

        SelectedIndex = Math.Clamp(SelectedIndex, 0, Math.Max(0, entries.Count - 1));

        if (error.Length > 0)
        {
            list.Write(1, 0, PickerText.Clip(error, list.Width), Theme.Error);

            return;
        }

        if (entries.Count == 0)
        {
            list.Write(1, 0, PickerText.Clip(_strings.ItemCount(0), list.Width), Theme.Secondary);

            return;
        }

        var rows = list.Height - 1;
        var start = Math.Clamp(SelectedIndex - rows / 2, 0, Math.Max(0, entries.Count - rows));

        FirstVisible = start;

        for (var offset = 0; offset < rows && start + offset < entries.Count; offset++)
        {
            var entry = entries[start + offset];

            var style = start + offset == SelectedIndex
                ? focused ? Theme.ActiveSelection : Theme.Selection
                : entry.IsDirectory
                    ? Theme.Info
                    : Theme.Default;

            list.Write(1 + offset, 0, PickerText.Pad(Row(entry, widths), list.Width), style);
        }
    }

    private string Header(Widths widths)
    {
        var line = "  " + PickerText.Pad(_strings.ColumnName(), widths.Name);

        if (widths.Date > 0)
        {
            line += PickerText.Pad(_strings.ColumnDateModified(), widths.Date);
        }

        if (widths.Size > 0)
        {
            line += PickerText.PadLeft(_strings.ColumnSize(), widths.Size);
        }

        if (widths.Kind > 0)
        {
            line += "  " + PickerText.Pad(_strings.ColumnKind(), widths.Kind);
        }

        return line;
    }

    private string Row(PickerEntry entry, Widths widths)
    {
        var line = (entry.IsDirectory ? "▸ " : "  ") + PickerText.Pad(entry.Name, widths.Name);

        if (widths.Date > 0)
        {
            line += PickerText.Pad(entry.Modified == default ? "" : _strings.DateModified(entry.Modified), widths.Date);
        }

        if (widths.Size > 0)
        {
            line += PickerText.PadLeft(entry.IsDirectory ? "--" : _strings.Size(entry.Length), widths.Size);
        }

        if (widths.Kind <= 0)
        {
            return line;
        }

        var kind = entry.IsVolume
            ? _strings.KindVolume()
            : entry.IsDirectory
                ? _strings.KindFolder()
                : _strings.KindOf(Path.GetExtension(entry.Name));

        return line + "  " + PickerText.Pad(kind, widths.Kind);
    }

    private static Widths Columns(int width)
    {
        const int date = 22;
        const int size = 10;
        const int kind = 16;

        if (width >= 2 + 24 + date + size + kind + 2)
        {
            return new(width - 2 - date - size - kind - 2, date, size, kind);
        }

        if (width >= 2 + 20 + date + size)
        {
            return new(width - 2 - date - size, date, size, 0);
        }

        if (width >= 2 + 16 + size)
        {
            return new(width - 2 - size, 0, size, 0);
        }

        return new(Math.Max(1, width - 2), 0, 0, 0);
    }

    private readonly record struct Widths(int Name, int Date, int Size, int Kind);
}
