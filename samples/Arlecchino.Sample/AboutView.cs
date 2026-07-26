using System;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample.Views;

namespace Arlecchino.Sample;

public class AboutView : IArlecchinoView
{
    private readonly Surface _surface;

    public AboutView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw()
    {
        _surface.AppendLine("  ABOUT", Theme.TableHeader);
        _surface.FillLine();
        _surface.AppendLine("  Views are plain classes implementing IArlecchinoView.", Theme.Default);
        _surface.AppendLine("  The generator turns every *View class into a ViewKind route.", Theme.Default);
        _surface.AppendLine("  Navigation keeps a back/forward history on Alt+←/→.", Theme.Default);
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        return key.Key == ConsoleKey.Escape ? ViewKind.Default : ViewRoute.None;
    }

    public (string Key, string Description)[] Hints() => [("Esc", "back")];
}
