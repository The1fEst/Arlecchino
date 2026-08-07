using System;
using Arlecchino.Navigation;
using Arlecchino.State;

namespace Arlecchino.Sample.Frames;

internal abstract class FrameShot
{
    public abstract void Arrange(ArlecchinoState state, Navigator navigator);
}

internal sealed class RouteShot : FrameShot
{
    private readonly ViewRoute[] _routes;

    public RouteShot(params ViewRoute[] routes)
    {
        _routes = routes;
    }

    public override void Arrange(ArlecchinoState state, Navigator navigator)
    {
        foreach (var route in _routes)
        {
            navigator.Apply(route);
        }
    }
}

internal sealed class ModalShot : FrameShot
{
    private readonly Action<ArlecchinoState> _open;
    private readonly ViewRoute _route;

    public ModalShot(ViewRoute route, Action<ArlecchinoState> open)
    {
        _route = route;
        _open = open;
    }

    public override void Arrange(ArlecchinoState state, Navigator navigator)
    {
        _open(state);
        navigator.Apply(_route);
    }
}
