using System;
using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;

namespace Arlecchino.Tests;

public sealed class FormHostView : IArlecchinoView
{
    private readonly Surface _surface;

    public FormHostView(Surface surface)
    {
        _surface = surface;
    }

    public static Form? Hosted { get; set; }

    public static SurfaceRegion Rows { get; private set; }

    public void Draw()
    {
        Rows = _surface.Content;
        Hosted?.Place(Rows);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => Hosted?.Handle(key).Route ?? ViewRoute.None;

    public ViewRoute HandleMouse(MouseEvent mouse) => Hosted?.HandleMouse(mouse).Route ?? ViewRoute.None;
}
