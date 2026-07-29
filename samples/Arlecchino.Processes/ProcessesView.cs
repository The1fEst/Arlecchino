using System.Globalization;
using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Processes.Views;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Widgets;

namespace Arlecchino.Processes;

public sealed class ProcessesView : IArlecchinoView
{
    private const int OutputLineRows = 2;

    private readonly Surface _surface;
    private readonly ProcessTable _processes;
    private readonly ArlecchinoState _state;
    private readonly Table<ProcessRow> _table;
    private readonly StatusBar _status;
    private readonly Spinner _spinner = new();

    public ProcessesView(
        Surface surface,
        ProcessTable processes,
        ArlecchinoState state,
        ArlecchinoOptions options,
        ViewLifetime lifetime)
    {
        _surface = surface;
        _processes = processes;
        _state = state;

        _table = new(options.Keymap)
        {
            Columns =
            [
                new()
                {
                    Header = static () => "Process",
                    Cell = static row => row.Name,
                    Sort = static (first, second) => string.CompareOrdinal(first.Name, second.Name),
                },
                new()
                {
                    Header = static () => "PID",
                    Cell = static row => row.Id.ToString(CultureInfo.InvariantCulture),
                    Width = 8,
                    AlignRight = true,
                    Sort = static (first, second) => first.Id.CompareTo(second.Id),
                },
                new()
                {
                    Header = static () => "Memory",
                    Cell = static row => Megabytes(row.Memory),
                    Width = 12,
                    AlignRight = true,
                    Sort = static (first, second) => first.Memory.CompareTo(second.Memory),
                },
                new()
                {
                    Header = static () => "Threads",
                    Cell = static row => row.Threads.ToString(CultureInfo.InvariantCulture),
                    Width = 9,
                    AlignRight = true,
                    Sort = static (first, second) => first.Threads.CompareTo(second.Threads),
                },
            ],
            ItemStyle = static row => row.Memory == 0 ? Theme.Muted : Theme.Default,
            OnActivate = Open,
        };

        _status = new()
        {
            Left = [Count, Loaded, () => _processes.Filter.Value.Length == 0 ? "" : $"filter: {_processes.Filter.Value}"],
        };

        lifetime.Track(_processes.Rows.SubscribeToStatus(() => _state.Invalidate()));
        _processes.Refresh();
    }

    public void Draw()
    {
        var content = _surface.Content.Inset(new Margin(0, 0, 0, OutputLineRows));
        var (header, rest) = content.SplitTop(2);
        var (rows, status) = rest.SplitTop(rest.Height - 1);

        header.WriteLine(0, "Processes", Theme.Header);
        header.WriteLine(1, Headline(), Theme.Muted);

        _table.Rows = _processes.Visible();
        _table.Draw(rows);
        _status.Draw(status);

        if (!_processes.Rows.IsLoading)
        {
            return;
        }

        _spinner.Advance();
        _spinner.Draw(header.SplitLeft(header.Width - 1).Right);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => _table.Handle(key).Route;

    public ViewRoute HandleMouse(MouseEvent mouse) => _table.HandleMouse(mouse).Route;

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.For(ConsoleKey.R, static () => "refresh", _processes.Refresh),
        ViewCommand.For(ConsoleKey.F, static () => "filter", Filter),
        ViewCommand.For(ConsoleKey.M, static () => "sort by memory", () => _table.SortBy(2)),
        ViewCommand.For(ConsoleKey.N, static () => "sort by name", () => _table.SortBy(0)),
        ViewCommand.Navigating(ConsoleKey.Enter, static () => "details", () => Open(_table.SelectedRow)),
    ];

    private ViewRoute Open(ProcessRow? row)
    {
        if (row is null)
        {
            return ViewRoute.None;
        }

        _processes.Selected.Value = row;
        return ViewKind.Details;
    }

    private void Filter() =>
        _state.RequestText("Filter", _processes.Filter.Value, null, typed =>
        {
            _processes.Filter.Value = typed;
            _table.Selected = 0;
        });

    private string Headline() => _processes.Rows.Error.Value is { } failure
        ? $"could not read the process list — {failure.Message}"
        : "sorted by memory · press m or n to change, f to filter, Enter for details";

    private string Count() => $"{_processes.Visible().Count} shown";

    private string Loaded() => _processes.LoadedAt == default
        ? "loading…"
        : $"read at {_processes.LoadedAt:HH:mm:ss}";

    private static string Megabytes(long bytes) => bytes == 0 ? "—" : $"{bytes / (1024d * 1024d):0.0} MB";
}
