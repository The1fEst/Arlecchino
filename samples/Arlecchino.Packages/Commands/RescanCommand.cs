using System;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Packages.Stores;

namespace Arlecchino.Packages.Commands;

public sealed class RescanCommand : IArlecchinoCommand
{
    private readonly Inventory _inventory;

    public RescanCommand(Inventory inventory) => _inventory = inventory;

    public KeyBinding Binding => new(ConsoleKey.R, ConsoleModifiers.Control);

    public string Icon => "↻";

    public string Label => "Rescan";

    public ViewRoute Execute()
    {
        _inventory.Rescan();
        return ViewRoute.None;
    }
}
