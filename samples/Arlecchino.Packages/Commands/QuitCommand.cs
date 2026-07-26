using System;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Microsoft.Extensions.Hosting;

namespace Arlecchino.Packages.Commands;

public sealed class QuitCommand : IArlecchinoCommand
{
    private readonly IHostApplicationLifetime _lifetime;

    public QuitCommand(IHostApplicationLifetime lifetime) => _lifetime = lifetime;

    public KeyBinding Binding => new(ConsoleKey.Q);

    public string Icon => "×";

    public string Label => "Quit";

    public ViewRoute Execute()
    {
        _lifetime.StopApplication();
        return ViewRoute.None;
    }
}
