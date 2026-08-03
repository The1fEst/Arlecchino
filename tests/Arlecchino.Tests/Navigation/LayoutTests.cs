using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Support;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests.Navigation;

/// <summary>
///     The frame drawn around every view. It is one object for the whole application, so what it holds
///     outlives the screen inside it — which is the point of having one at all rather than a header every
///     view draws again.
/// </summary>
public sealed class LayoutTests
{
    [Fact]
    public void TheLayoutIsDrawnAroundTheView()
    {
        using var app = Framed();

        app.Navigator.Apply(ViewKind.Framed);

        var lines = app.FrameLines();
        var band = Row(lines, "the band");
        var view = Row(lines, "framed view");
        var bar = Row(lines, "the bar");

        Assert.True(band < view, "the band is above the view");
        Assert.True(view < bar, "the bar is below it");
    }

    /// <summary>
    ///     The view is given the room the layout left it and asks the surface for it as usual, so nothing
    ///     in the view knows it is inside one.
    /// </summary>
    [Fact]
    public void TheViewIsGivenWhatTheLayoutLeftIt()
    {
        using var app = Framed();

        app.Navigator.Apply(ViewKind.Framed);
        app.Frame();

        var whole = app.Surface.Content;

        Assert.Equal(whole.Top + 1, FramedView.Seen.Top);
        Assert.Equal(whole.Height - 2, FramedView.Seen.Height);
    }

    /// <summary>A screen that wants the whole terminal says so and is drawn without the frame.</summary>
    [Fact]
    public void AViewThatWantsTheWholeTerminalIsDrawnWithoutTheLayout()
    {
        using var app = Framed();

        app.Navigator.Apply(ViewKind.Whole);

        var frame = app.Frame();

        Assert.DoesNotContain("the band", frame, StringComparison.Ordinal);
        Assert.Contains("whole view", frame, StringComparison.Ordinal);
    }

    /// <summary>
    ///     The layout sees a click before the view does, because what it draws is around the view rather
    ///     than under it. What it does not take carries on to the view as before.
    /// </summary>
    [Fact]
    public void TheLayoutTakesAClickOnItsOwnBandBeforeTheView()
    {
        using var app = Framed();

        app.Navigator.Apply(ViewKind.Framed);

        var lines = app.FrameLines();

        FramedLayout.Clicked = 0;
        FramedView.Clicked = 0;

        app.Click(Row(lines, "the band"), 3);
        app.Click(Row(lines, "framed view"), 3);

        Assert.Equal(1, FramedLayout.Clicked);
        Assert.Equal(1, FramedView.Clicked);
    }

    /// <summary>
    ///     One layout serves the application, so what it holds survives leaving a screen and coming back —
    ///     a header rebuilt per view could not keep anything.
    /// </summary>
    [Fact]
    public void TheLayoutOutlivesTheViewsDrawnInsideIt()
    {
        using var app = Framed();

        app.Navigator.Apply(ViewKind.Framed);
        app.Frame();

        var layout = FramedLayout.Instance!;
        var drawn = layout.Drawn;

        app.Navigator.Apply(ViewKind.Whole);
        app.Frame();
        app.Navigator.Apply(ViewKind.Framed);
        app.Frame();

        Assert.Same(layout, FramedLayout.Instance);
        Assert.True(layout.Drawn > drawn, "the same layout kept drawing across the views");
    }

    private static int Row(string[] lines, string text)
    {
        var row = Array.FindIndex(lines, line => line.Contains(text, StringComparison.Ordinal));

        Assert.True(row >= 0, $"'{text}' is not on the screen");

        return row;
    }

    /// <summary>
    ///     An application whose frame belongs to the layout. The hints box and the output row are the
    ///     framework's own chrome and are drawn over whatever is under them, which is why an application
    ///     with a bar of its own turns them off — as this one does.
    /// </summary>
    /// <returns>The application.</returns>
    private static TestApplication Framed()
    {
        return new(configure: static builder =>
        {
            builder.Options.ShowHints = false;
            builder.Options.ShowOutputLine = false;

            builder.UseMouse().UseLayout<FramedLayout>();
        });
    }
}

/// <summary>A band above and a bar below, with the view in what is left between them.</summary>
public sealed class FramedLayout : IArlecchinoLayout
{
    private SurfaceRegion _band;

    public FramedLayout()
    {
        Instance = this;
    }

    public static FramedLayout? Instance { get; private set; }

    public static int Clicked { get; set; }

    public int Drawn { get; private set; }

    public void Draw(SurfaceRegion frame, Action<SurfaceRegion> body)
    {
        ArgumentNullException.ThrowIfNull(body);

        Drawn++;
        _band = frame.Rows(0, 1);

        frame.WriteLine(0, "the band", Theme.Header);
        frame.WriteLine(frame.Height - 1, "the bar", Theme.Muted);

        body(frame.Rows(1, frame.Height - 2));
    }

    public bool HandleMouse(MouseEvent mouse)
    {
        if (!_band.Contains(mouse.Row, mouse.Column))
        {
            return false;
        }

        Clicked++;

        return true;
    }
}

/// <summary>A view that draws where it is put and counts what reaches it.</summary>
public sealed class FramedView : IArlecchinoView
{
    private readonly Surface _surface;

    public FramedView(Surface surface)
    {
        _surface = surface;
    }

    public static SurfaceRegion Seen { get; private set; }

    public static int Clicked { get; set; }

    public void Draw()
    {
        Seen = _surface.Content;
        _surface.Content.WriteLine(0, "framed view", Theme.Default);
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        return ViewRoute.None;
    }

    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        Clicked++;

        return ViewRoute.None;
    }

    public IReadOnlyList<ViewCommand> Commands()
    {
        return [];
    }
}

/// <summary>A view that wants the terminal to itself.</summary>
public sealed class WholeView : IArlecchinoView
{
    private readonly Surface _surface;

    public WholeView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw()
    {
        _surface.Content.WriteLine(0, "whole view", Theme.Default);
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        return ViewRoute.None;
    }

    public bool UsesLayout => false;
}
