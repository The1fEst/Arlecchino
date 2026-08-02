using System;
using BenchmarkDotNet.Attributes;
using Arlecchino.Input;
using Arlecchino.Rendering;

using Arlecchino.Widgets.Lists;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Benchmarks;

[MemoryDiagnoser]
public class RenderBenchmarks
{
    private const int FrameWidth = 120;
    private const int FrameHeight = 40;

    private readonly SinkTerminal _terminal = new(FrameWidth, FrameHeight);
    private readonly string[] _rows = new string[FrameHeight];
    private readonly string[] _items = new string[2000];

    private readonly Surface _surface;
    private readonly ListBox<string> _list;

    public RenderBenchmarks()
    {
        _surface = new(_terminal);
        _list = new(new()) { Render = static item => item, Items = _items };
    }

    [GlobalSetup]
    public void Setup()
    {
        for (var row = 0; row < _rows.Length; row++)
        {
            _rows[row] = new((char)('a' + row % 26), FrameWidth - 4);
        }

        for (var item = 0; item < _items.Length; item++)
        {
            _items[item] = $"item {item} — a row of the kind an application actually lists";
        }
    }

    [Benchmark(Description = "Full frame, every cell changed")]
    public void FullFrame()
    {
        _surface.ForgetPreviousFrame();
        DrawRows();
        _surface.Build();
    }

    [Benchmark(Description = "Repeat frame, nothing changed")]
    public void UnchangedFrame()
    {
        DrawRows();
        _surface.Build();
    }

    [Benchmark(Description = "Frame with one cell changed")]
    public void OneCellChanged()
    {
        DrawRows();
        _surface.WriteAt(FrameHeight / 2, FrameWidth / 2, _spinner[_frame++ % _spinner.Length], Theme.Accent);
        _surface.Build();
    }

    [Benchmark(Description = "List of 2000 scrolled by one")]
    public void ScrollList()
    {
        _surface.StartFrame();
        _list.Selected = (_list.Selected + 1) % _items.Length;
        _list.Draw(_surface.Frame);
        _surface.Build();
    }

    private static readonly string[] _spinner = ["⠋", "⠙", "⠹", "⠸"];

    private int _frame;

    private void DrawRows()
    {
        _surface.StartFrame();

        for (var row = 0; row < FrameHeight; row++)
        {
            _surface.WriteAt(row, 2, _rows[row], row % 3 == 0 ? Theme.Accent : Theme.Default);
        }
    }

    private sealed class SinkTerminal : IArlecchinoTerminal
    {
        public SinkTerminal(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public bool KeyAvailable => false;

        public bool MouseAvailable => false;

        public int WrittenLength { get; private set; }

        public void Write(string text) => WrittenLength += text.Length;

        public ConsoleKeyInfo ReadKey() => default;

        public void Unread(ConsoleKeyInfo key) { }

        public MouseEvent ReadMouse() => default;

        public void EnterFullScreen() { }

        public void LeaveFullScreen() { }

        public void EnableMouse() { }

        public void DisableMouse() { }

        public void EnablePaste() { }

        public void DisablePaste() { }

        public void CopyToClipboard(string text) { }
    }
}
