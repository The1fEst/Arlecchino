using Arlecchino.Hosting;

namespace Arlecchino.Commander;

public static class CommanderOptions
{
    public static void Apply(ArlecchinoOptions options)
    {
        options.MinimumWidth = 80;
        options.MinimumHeight = 18;
        options.HorizontalPadding = 1;
        options.VerticalPadding = 0;
        options.ShowHints = false;
        options.ShowOutputLine = false;
    }
}
