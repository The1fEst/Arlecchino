using System;
using Arlecchino.Hosting;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample.Views;
using Arlecchino.Widgets;

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

        _layout = PaneTree.Rows(
            3,
            PaneTree.Pane(static region => Box(region, "toolbar", "three rows, whatever the terminal is")),
            PaneTree.Rows(
                PaneSize.CellsFromEnd(2),
                PaneTree.Columns(
                    0.25,
                    PaneTree.Pane(files),
                    PaneTree.Rows(
                        0.7,
                        PaneTree.Pane(static region => Box(region, "editor", "70% of what is left")),
                        PaneTree.Pane(static region => Box(region, "log", "the rest")))),
                PaneTree.Pane(status)));
    }

    public void Draw() => _layout.Draw(_surface.Content, gap: 1);

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
