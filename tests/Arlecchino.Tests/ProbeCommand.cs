using System;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.State;

namespace Arlecchino.Tests;

public sealed class ProbeCommand : IArlecchinoCommand
{
    private readonly ArlecchinoState _state;

    public ProbeCommand(ArlecchinoState state)
    {
        _state = state;
    }

    public KeyBinding Binding => new(ConsoleKey.P);

    public string Icon => "▸";

    public string Label => "Probe command";

    public ViewRoute Execute()
    {
        _state.Output = "probe command";
        return ViewRoute.None;
    }
}
