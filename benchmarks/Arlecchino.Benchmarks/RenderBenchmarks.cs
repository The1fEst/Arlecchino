using System;
using BenchmarkDotNet.Attributes;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Widgets;

namespace Arlecchino.Benchmarks;

[MemoryDiagnoser]
public class RenderBenchmarks
{
    private const int Width = 120;
    private const int Height = 40;

    private readonly SinkTerminal _terminal = new(Width, Height);
    private readonly string[] _rows = new string[Height];
    private readonly string[] _items = new string[2000];

    private Surface _surface = null!;
    private ListBox<string> _list = null!;

    [GlobalSetup]
    public void Setup()
    {
        for (var row = 0; row < _rows.Length; row++)
        {
            _rows[row] = new((char)('a' + row % 26), Width - 4);
        }

        for (var item = 0; item < _items.Length; item++)
        {
            _items[item] = $"item {item} — a row of the kind an application actually lists";
        }

        _surface = new(_terminal);
        _list = new(new()) { Render = static item => item, Items = _items };
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
        _surface.WriteAt(Height / 2, Width / 2, _spinner[_frame++ % _spinner.Length], Theme.Accent);
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

        for (var row = 0; row < Height; row++)
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

        public MouseEvent ReadMouse() => default;

        public void EnterFullScreen()
        {
        }

        public void LeaveFullScreen()
        {
        }

        public void EnableMouse()
        {
        }

        public void DisableMouse()
        {
        }

        public void EnablePaste()
        {
        }

        public void DisablePaste()
        {
        }

        public void CopyToClipboard(string text)
        {
        }
    }
}
