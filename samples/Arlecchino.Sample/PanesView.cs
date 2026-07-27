using System;
using Arlecchino.Hosting;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample.Views;
using Arlecchino.Widgets;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Sample;

public sealed class PanesView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly PaneTree _layout;

    public PanesView(Surface surface, ArlecchinoOptions options)
    {
        _surface = surface;

        var files = new ListBox<string>(options.Keymap)
        {
            Render = static file => $" {file}",
            Items = ["Program.cs", "PanesView.cs", "WidgetsView.cs", "SettingsView.cs"],
            IsFocused = true,
        };

        var status = new StatusBar
        {
            Left = [static () => "a row measured from the bottom, above the output line"],
            Right = [static () => "Esc back"],
        };

        _layout = Branch(
            Rows,
            3,
            Leaf(static region => Box(region, "toolbar", "three rows, whatever the terminal is")),
            Branch(
                Rows,
                PaneSize.CellsFromEnd(2),
                Branch(
                    Columns,
                    0.25,
                    Leaf(files),
                    Branch(
                        0.7,
                        Leaf(static region => Box(region, "editor", "70%, along whichever side is longer")),
                        Leaf(static region => Box(region, "log", "the rest of it")))),
                Leaf(status))).Gaps(inner: 1, outer: 1);
    }

    public void Draw() => _layout.Draw(_surface.Content);

    public ViewRoute Handle(ConsoleKeyInfo key) =>
        key.Key == ConsoleKey.Escape ? ViewKind.Default : ViewRoute.None;

    public (string Key, string Description)[] Hints() => [("Esc", "back")];

    private static void Box(SurfaceRegion region, string title, string what)
    {
        var inside = region.Border(Theme.Info, title);

        if (inside.Height > 0)
        {
            inside.WriteLine(0, what, Theme.Muted);
        }

        if (inside.Height > 2)
        {
            inside.WriteLine(2, $"{inside.Width}×{inside.Height}", Theme.Accent);
        }
    }
}
