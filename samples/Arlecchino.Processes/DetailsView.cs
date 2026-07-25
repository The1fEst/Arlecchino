using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Navigation;
using Arlecchino.Processes.Views;
using Arlecchino.Rendering;

namespace Arlecchino.Processes;

public sealed class DetailsView : IView
{
    private readonly Surface _surface;
    private readonly ProcessTable _processes;

    public DetailsView(Surface surface, ProcessTable processes)
    {
        _surface = surface;
        _processes = processes;
    }

    public void Draw()
    {
        var content = _surface.Content;

        if (_processes.Selected.Value is not { } row)
        {
            content.WriteLine(0, "Nothing selected", Theme.Header);
            return;
        }

        content.WriteLine(0, row.Name, Theme.Header);
        content.WriteLine(1, $"pid {row.Id}", Theme.Muted);

        var labels = new (string Label, string Value)[]
        {
            ("Working set", $"{row.Memory / (1024d * 1024d):0.0} MB"),
            ("Threads", row.Threads.ToString()),
            ("Processor time", row.Cpu.ToString(@"hh\:mm\:ss")),
            ("Started", row.Started is { } started ? started.ToString("yyyy-MM-dd HH:mm:ss") : "not available"),
        };

        var width = 0;
        foreach (var (label, _) in labels)
        {
            width = Math.Max(width, TextWidth.Of(label));
        }

        for (var i = 0; i < labels.Length; i++)
        {
            var (label, value) = labels[i];
            content.Write(3 + i, 0, TextWidth.PadRight(label, width), Theme.Muted);
            content.Write(3 + i, width + 2, value, Theme.Default);
        }
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Escape, static () => "back", static () => ViewKind.Processes),
    ];
}
