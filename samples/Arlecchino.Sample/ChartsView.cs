using System;
using System.Globalization;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample.Views;
using Arlecchino.Widgets;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Sample;

public sealed class ChartsView : IArlecchinoView
{
    private sealed record Mirror(string Name, decimal Megabytes);

    private static readonly Mirror[] Mirrors =
    [
        new("europe-west", 812m),
        new("us-east", 640m),
        new("asia-south", 227m),
        new("cdn-fallback", 58m),
    ];

    private static readonly decimal[] Downloads =
    [
        12m, 14m, 19m, 17m, 23m, 31m, 44m, 39m, 35m, 28m,
        26m, 30m, 41m, 57m, 68m, 74m, 66m, 51m, 43m, 38m,
    ];

    private static readonly decimal[] Failures =
    [
        0m, 0m, 1m, 0m, 0m, 0m, 2m, 5m, 9m, 6m,
        3m, 1m, 0m, 0m, 1m, 0m, 0m, 0m, 0m, 0m,
    ];

    private static readonly decimal[] Latency =
    [
        41m, 44m, 43m, 48m, 52m, 61m, 88m, 120m, 97m, 74m,
        63m, 58m, 55m, 51m, 49m, 47m, 46m, 45m, 44m, 44m,
    ];

    private readonly Surface _surface;
    private readonly PaneTree _layout;

    private readonly BarChart<Mirror> _mirrors = new()
    {
        Render = static mirror => mirror.Name,
        Value = static mirror => mirror.Megabytes,
        Items = Mirrors,
        Caption = static value => value.ToString("0", CultureInfo.InvariantCulture),
        ItemStyle = static mirror => mirror.Megabytes < 100m ? Theme.Muted : Theme.Active,
    };

    private readonly Sparkline _downloads = new()
    {
        Values = Downloads,
        Caption = static value => $"{value:0}/s",
    };

    private readonly Sparkline _failures = new()
    {
        Values = Failures,
        Minimum = 0,
        Caption = static value => $"{value:0}/s",
    };

    private readonly Sparkline _latency = new()
    {
        Values = Latency,
        Caption = static value => $"{value:0}ms",
    };

    private readonly Gauge _disk = new()
    {
        Value = 91,
        Caption = static value => $"{value:0}%",
        Bands = [new(0m, Theme.Active), new(70m, Theme.Warning), new(90m, Theme.Error)],
    };

    private readonly Gauge _memory = new()
    {
        Value = 63,
        Caption = static value => $"{value:0}%",
        Bands = [new(0m, Theme.Active), new(70m, Theme.Warning), new(90m, Theme.Error)],
    };

    private readonly Gauge _queue = new()
    {
        Minimum = 0,
        Maximum = 500,
        Value = 128,
        Caption = static value => $"{value:0}",
    };

    public ChartsView(Surface surface)
    {
        _surface = surface;

        var panes = Branch(
            Rows,
            0.5,
            Leaf(_mirrors, static () => "downloads by mirror, MB"),
            Branch(
                Columns,
                0.5,
                Leaf(DrawTrends, static () => "last 20 minutes"),
                Leaf(DrawHost, static () => "host")));

        _layout = panes.Gaps(inner: 1, outer: 1);
    }

    public void Draw() => _layout.Draw(_surface.Content);

    public ViewRoute Handle(ConsoleKeyInfo key) =>
        key.Key == ConsoleKey.Escape ? ViewKind.Default : ViewRoute.None;

    public ViewRoute HandleMouse(MouseEvent mouse) => ViewRoute.None;

    public (string Key, string Description)[] Hints() => [("Esc", "back")];

    private static void Labelled(SurfaceRegion region, int row, string label, IArlecchinoWidget widget)
    {
        var (name, rest) = region.Rows(row, 1).SplitLeft(9);

        name.WriteLine(0, label, Theme.Muted);
        widget.Draw(rest);
    }

    private void DrawTrends(SurfaceRegion region)
    {
        Labelled(region, 0, "downloads", _downloads);
        Labelled(region, 2, "failures", _failures);
        Labelled(region, 4, "latency", _latency);
    }

    private void DrawHost(SurfaceRegion region)
    {
        Labelled(region, 0, "disk", _disk);
        Labelled(region, 2, "memory", _memory);
        Labelled(region, 4, "queue", _queue);
    }
}
