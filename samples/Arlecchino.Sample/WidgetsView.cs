using System.Globalization;
using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample.Views;
using Arlecchino.Widgets;

namespace Arlecchino.Sample;

public sealed class WidgetsView : IArlecchinoView
{
    private sealed record Mod(string Name, string Author, int Files, bool Enabled);

    private static readonly Mod[] Catalog =
    [
        new("Widebody kit", "fEst", 42, true),
        new("Night traffic", "anon", 7, false),
        new("Engine sounds", "carfan", 118, true),
        new("HD wheels", "rimshop", 23, true),
        new("Rain shaders", "weatherman", 5, false),
    ];

    private readonly Surface _surface;
    private readonly FocusRing _focus;
    private readonly Tabs _tabs;
    private readonly Table<Mod> _table;
    private readonly ListBox<string> _authors;
    private readonly ProgressBar _progress = new() { Value = 68, Caption = static value => $"{value:0}%" };
    private readonly Spinner _spinner = new();
    private readonly StatusBar _status;

    public WidgetsView(Surface surface, ArlecchinoOptions options)
    {
        _surface = surface;

        _tabs = new(options.Keymap)
        {
            Titles = [static () => "Installed", static () => "Available", static () => "Updates"],
        };

        _table = new(options.Keymap)
        {
            Columns =
            [
                new() { Header = static () => "Name", Cell = static mod => mod.Name, Sort = static (first, second) => string.CompareOrdinal(first.Name, second.Name) },
                new() { Header = static () => "Author", Cell = static mod => mod.Author, Width = 12 },
                new() { Header = static () => "Files", Cell = static mod => mod.Files.ToString(CultureInfo.InvariantCulture), Width = 6, AlignRight = true, Sort = static (first, second) => first.Files.CompareTo(second.Files) },
            ],
            ItemStyle = static mod => mod.Enabled ? Theme.Default : Theme.Muted,
            Rows = Catalog,
        };

        _authors = new(options.Keymap)
        {
            Render = static author => $" {author}",
            Items = Authors(),
        };

        _status = new()
        {
            Left = [() => $"{Catalog.Length} mods", () => _spinner.Current + " syncing"],
            Right = [static () => "Tab panes", static () => "s sort", static () => "Esc back"],
        };

        _focus = new(options.Keymap);
        _focus.Add(_tabs);
        _focus.Add(_table);
        _focus.Add(_authors);
    }

    public void Draw()
    {
        var under = _tabs.Draw(_surface.Content);
        var (_, rest) = under.SplitTop(1);

        var (body, footer) = rest.SplitTop(rest.Height - 3);
        var (table, authors) = body.SplitLeft(body.Width - 20);

        _table.Draw(table.Inset(new Margin(0, 0, 2, 0)));
        _authors.Draw(authors);

        _progress.Draw(footer.Rows(0, 1));
        _status.Draw(footer.Rows(2, 1));
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Escape:
                return ViewKind.Default;
            case ConsoleKey.S:
                _table.SortBy(_table.SortedBy == 0 ? 2 : 0);
                return ViewRoute.None;
            case ConsoleKey.Spacebar:
                _spinner.Advance();
                return ViewRoute.None;
            default:
                return _focus.Handle(key);
        }
    }

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public (string Key, string Description)[] Hints() =>
    [
        ("Tab", "panes"),
        ("↑↓", "move"),
        ("s", "sort"),
        ("Space", "spin"),
        ("Esc", "back"),
    ];

    private static List<string> Authors()
    {
        var authors = new List<string>();
        foreach (var mod in Catalog)
        {
            if (!authors.Contains(mod.Author))
            {
                authors.Add(mod.Author);
            }
        }

        return authors;
    }
}
