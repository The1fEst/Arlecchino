using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Sample.Views;

namespace Arlecchino.Sample;

public sealed class AboutView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly ArlecchinoKeymap _keymap;

    public AboutView(Surface surface, ArlecchinoKeymap keymap)
    {
        _surface = surface;
        _keymap = keymap;
    }

    public void Draw()
    {
        _surface.AppendLine("  ABOUT", Theme.TableHeader);
        _surface.FillLine();
        _surface.AppendLine("  Views are plain classes implementing IArlecchinoView.", Theme.Default);
        _surface.AppendLine("  The generator turns every *View class into a ViewKind route.", Theme.Default);
        _surface.AppendLine(
            $"  Navigation keeps a back/forward history on {_keymap.Back} and {_keymap.Forward}.",
            Theme.Default);
    }

    public ViewRoute Handle(KeyPress key)
    {
        return key.Key == ConsoleKey.Escape ? ViewKind.Default : ViewRoute.None;
    }

    public (string Key, string Description)[] Hints() => [("Esc", "back")];
}
