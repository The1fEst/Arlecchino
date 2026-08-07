using System;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Sample.Views;
using Arlecchino.Widgets.Lists;
using Arlecchino.Widgets.Readouts;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Sample;

public sealed class PanesView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly PaneTree _layout;
    private readonly FocusRing _focus;

    public PanesView(Surface surface, ArlecchinoOptions options)
    {
        _surface = surface;

        var files = new ListBox<string>(options.Keymap)
        {
            Render = static file => $" {file}",
            Items = ["Program.cs", "PanesView.cs", "WidgetsView.cs", "SettingsView.cs"],
        };

        var authors = new ListBox<string>(options.Keymap)
        {
            Render = static author => $" {author}",
            Items = ["fEst", "anon", "carfan"],
        };

        var status = new StatusBar
        {
            Left = [static () => "Tab walks the panes in the order the tree lays them out"],
            Right = [static () => "Esc back"],
        };

        var panes = Branch(
            Rows,
            3,
            Leaf(
                static region => region.WriteLine(0, "three rows, whatever the terminal is", Theme.Muted),
                static () => "toolbar"),
            Branch(
                Rows,
                PaneSize.CellsFromEnd(2),
                Branch(
                    Columns,
                    0.25,
                    Leaf(files, static () => "files"),
                    Branch(0.7, Leaf(authors, static () => "authors"), Leaf(Log, static () => "log"))),
                Leaf(status)));

        _layout = panes.Gaps(inner: 0, outer: 1);
        _focus = _layout.AsFocusRing(options.Keymap);
    }

    public void Draw() => _layout.Draw(_surface.Content);

    public ViewRoute Handle(KeyPress key) =>
        key.Key == ConsoleKey.Escape ? ViewKind.Default : _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public (string Key, string Description)[] Hints() =>
    [
        ("Tab", "next pane"),
        ("↑↓", "move"),
        ("Esc", "back"),
    ];

    private static void Log(SurfaceRegion region)
    {
        region.WriteLine(0, "the rest of it", Theme.Muted);

        if (region.Height > 2)
        {
            region.WriteLine(2, $"{region.Width}×{region.Height}", Theme.Accent);
        }
    }
}
