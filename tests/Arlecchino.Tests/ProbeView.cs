using System;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Tests.Views;

namespace Arlecchino.Tests;

public sealed class ProbeView : IArlecchinoView
{
    private readonly Surface _surface;

    public ProbeView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw() => _surface.AppendLine("probe", Theme.Default);

    public ViewRoute Handle(ConsoleKeyInfo key) =>
        key.Key == ConsoleKey.O ? ViewKind.Other : ViewRoute.None;

    public (string Key, string Description)[] Hints() => [("o", "other")];
}

public sealed class OtherView : IArlecchinoView
{
    private readonly Surface _surface;

    public OtherView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw() => _surface.AppendLine("other", Theme.Default);

    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
}
