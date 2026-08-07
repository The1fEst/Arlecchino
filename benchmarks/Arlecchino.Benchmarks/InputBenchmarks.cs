using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using BenchmarkDotNet.Attributes;
using Arlecchino.Widgets.Lists;

namespace Arlecchino.Benchmarks;

[MemoryDiagnoser]
public class InputBenchmarks
{
    private const string Route = "list";
    private const int Items = 2000;

    private ArlecchinoTestHost _host = null!;

    [GlobalSetup]
    public void Setup()
    {
        _host = new(120, 40, builder => builder.AddView<ListView>(Route).StartAt(Route));
        _host.Navigator.Apply(new(Route));
        _ = _host.Frame();
    }

    [GlobalCleanup]
    public void Cleanup() => _host.Dispose();

    [Benchmark(Description = "A key through the router, no frame drawn")]
    public void PressKey() => _host.Press(ConsoleKey.DownArrow);

    [Benchmark(Description = "A key the view does not answer to")]
    public void PressUnhandledKey() => _host.Press(ConsoleKey.F12);

    [Benchmark(Description = "A click through the router")]
    public void Click() => _host.Click(4, 10);

    [Benchmark(Description = "A pasted block arriving as an escape sequence")]
    public void ReadPaste() => _host.ReadFromTerminal("\e[200~pasted into the list\e[201~");

    private sealed class ListView : IArlecchinoView
    {
        private readonly Surface _surface;
        private readonly ListBox<string> _list;

        public ListView(Surface surface, ArlecchinoKeymap keymap)
        {
            _surface = surface;

            var items = new string[Items];
            for (var item = 0; item < items.Length; item++)
            {
                items[item] = $"item {item} — a row of the kind an application actually lists";
            }

            _list = new(keymap) { Render = static item => item, Items = items, IsFocused = true };
        }

        public void Draw() => _list.Draw(_surface.Frame);

        public ViewRoute Handle(KeyPress key) => _list.Handle(key).Route;

        public IReadOnlyList<ViewCommand> Commands() => [];
    }
}
