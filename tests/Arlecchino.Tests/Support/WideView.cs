using System;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Tests.Support;

public sealed class WideView : IArlecchinoView
{
    private readonly Surface _surface;

    public WideView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw() => _surface.AppendLine("日本語のビュー", Theme.Default);

    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;

    public (string Key, string Description)[] Hints() =>
    [
        ("↑↓", "移動する"),
        ("Enter", "選ぶ"),
        ("q", "quit"),
    ];
}
