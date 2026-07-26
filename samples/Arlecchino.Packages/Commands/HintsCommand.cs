using System;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Packages.Commands;

public sealed class HintsCommand : IArlecchinoCommand
{
    private readonly ArlecchinoOptions _options;

    public HintsCommand(ArlecchinoOptions options) => _options = options;

    public KeyBinding Binding => new(ConsoleKey.H);

    public string Icon => "?";

    public string Label => "Hints";

    public ViewRoute Execute()
    {
        _options.ShowHints = !_options.ShowHints;
        return ViewRoute.None;
    }
}
