using System;
using Microsoft.Extensions.Hosting;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Sample.Views;
using Arlecchino.State;

namespace Arlecchino.Sample;

public sealed class AboutCommand : IArlecchinoCommand
{
    public KeyBinding Binding => new(ConsoleKey.A);
    public string Icon => "?";
    public string Label => "About";
    public ViewRoute Execute() => ViewKind.About;
}

public sealed class PickFolderCommand : IArlecchinoCommand
{
    private readonly ArlecchinoState _state;

    public PickFolderCommand(ArlecchinoState state)
    {
        _state = state;
    }

    public KeyBinding Binding => new(ConsoleKey.F);
    public string Icon => "▸";
    public string Label => "Pick a folder";

    public ViewRoute Execute()
    {
        _state.FilePicker = new(
            "Pick a folder",
            PickFolder: true,
            Environment.CurrentDirectory,
            ViewKind.Default,
            path => _state.Output = $"picked: {path}");

        return Routes.FilePicker;
    }
}

public sealed class QuitCommand : IArlecchinoCommand
{
    private readonly IHostApplicationLifetime _lifetime;

    public QuitCommand(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public KeyBinding Binding => new(ConsoleKey.Q);
    public string Icon => "×";
    public string Label => "Quit";

    public ViewRoute Execute()
    {
        _lifetime.StopApplication();
        return ViewRoute.None;
    }
}
