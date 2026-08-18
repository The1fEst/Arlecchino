using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;

namespace Arlecchino.Tests.Support;

public sealed class FormHostView : IArlecchinoView
{
    private readonly Surface _surface;

    public FormHostView(Surface surface)
    {
        _surface = surface;
    }

    public static Form? Form { get; set; }

    public static SurfaceRegion Rows { get; private set; }

    public void Draw()
    {
        Rows = _surface.Content;
        Form?.Draw(Rows);
    }

    public ViewRoute Handle(KeyPress key) => Form?.Handle(key).Route ?? ViewRoute.None;

    public ViewRoute HandleMouse(MouseEvent mouse) => Form?.HandleMouse(mouse).Route ?? ViewRoute.None;
}
