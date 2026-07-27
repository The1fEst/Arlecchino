using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Testing;
using Arlecchino.Tests.Views;
using Arlecchino.Atoms;

namespace Arlecchino.Tests;

public sealed class TestApplication : IDisposable
{
    private readonly ArlecchinoTestHost _host;

    public TestApplication(int width = 80, int height = 24, Action<ArlecchinoBuilder>? configure = null)
    {
        _host = new(width, height, builder =>
        {
            builder.AddGeneratedViews().StartAt(ViewKind.Probe);
            configure?.Invoke(builder);
        });
    }

    public FakeTerminal Terminal => _host.Terminal;

    public IServiceProvider Services => _host.Services;

    public ArlecchinoState State => _host.State;

    public Navigator Navigator => _host.Navigator;

    public Surface Surface => _host.Surface;

    public ArlecchinoOptions Options => _host.Options;

    public Repaint Repaint => _host.Repaint;

    public UiDispatcher Dispatcher => _host.Dispatcher;

    public AtomHistory History => _host.History;

    public void Press(ConsoleKey key, bool shift = false, bool alt = false, bool control = false) =>
        _host.Press(key, shift, alt, control);

    public void Type(string text) => _host.Type(text);

    public void ReadFromTerminal(string sequence) => _host.ReadFromTerminal(sequence);

    public void DrainInput() => _host.DrainInput();

    public void Advance(TimeSpan amount) => _host.Advance(amount);

    public string Frame() => _host.Frame();

    public string[] FrameLines() => _host.FrameLines();

    public string FrameLineContaining(string text) => _host.FrameLineContaining(text);

    public IReadOnlyList<string> RawStyles() => _host.Styles();

    public IReadOnlyList<string> Styles() => _host.Styles();

    public void Click(int row, int column) => _host.Click(row, column);

    public void Scroll(int row, int column, bool down) => _host.Scroll(row, column, down);

    public void Dispose() => _host.Dispose();
}
