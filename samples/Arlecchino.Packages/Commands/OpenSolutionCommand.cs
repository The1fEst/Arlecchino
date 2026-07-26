using System;
using System.IO;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Packages.Stores;
using Arlecchino.Packages.Views;
using Arlecchino.State;

namespace Arlecchino.Packages.Commands;

public sealed class OpenSolutionCommand : IArlecchinoCommand
{
    private static readonly string[] Openable = [".slnx", ".sln", ".csproj"];

    private readonly ArlecchinoState _state;
    private readonly Inventory _inventory;

    public OpenSolutionCommand(ArlecchinoState state, Inventory inventory)
    {
        _state = state;
        _inventory = inventory;
    }

    public KeyBinding Binding => new(ConsoleKey.O, ConsoleModifiers.Control);

    public string Icon => "▤";

    public string Label => "Open solution";

    public ViewRoute Execute()
    {
        _state.FilePicker = new(
            "Pick a solution or a project",
            false,
            Start(),
            ViewKind.Inventory,
            path =>
            {
                _inventory.Solution.Value = path;
                _inventory.Rescan();
            })
        {
            FileFilter = static path => Array.IndexOf(Openable, Path.GetExtension(path).ToLowerInvariant()) >= 0,
        };

        return Routes.FilePicker;
    }

    private string Start()
    {
        var solution = _inventory.Solution.Value;
        return solution.Length == 0
            ? Directory.GetCurrentDirectory()
            : Path.GetDirectoryName(solution) ?? Directory.GetCurrentDirectory();
    }
}
