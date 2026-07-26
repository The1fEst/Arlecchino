using System;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class MouseTests
{
    [Fact]
    public void PressAndReleaseAreParsedWithZeroBasedCells()
    {
        Assert.True(EscapeSequenceParser.TryParseMouse("<0;10;5M", out var pressed));
        Assert.Equal(MouseAction.Pressed, pressed.Action);
        Assert.Equal(MouseButton.Left, pressed.Button);
        Assert.Equal(4, pressed.Row);
        Assert.Equal(9, pressed.Column);

        Assert.True(EscapeSequenceParser.TryParseMouse("<2;1;1m", out var released));
        Assert.Equal(MouseAction.Released, released.Action);
        Assert.Equal(MouseButton.Right, released.Button);
        Assert.Equal(0, released.Row);
        Assert.Equal(0, released.Column);
    }

    [Fact]
    public void WheelAndModifiersAreParsed()
    {
        Assert.True(EscapeSequenceParser.TryParseMouse("<64;3;3M", out var up));
        Assert.Equal(MouseAction.ScrolledUp, up.Action);
        Assert.True(up.IsScroll);

        Assert.True(EscapeSequenceParser.TryParseMouse("<65;3;3M", out var down));
        Assert.Equal(MouseAction.ScrolledDown, down.Action);

        Assert.True(EscapeSequenceParser.TryParseMouse("<16;3;3M", out var withControl));
        Assert.Equal(ConsoleModifiers.Control, withControl.Modifiers);

        Assert.True(EscapeSequenceParser.TryParseMouse("<32;3;3M", out var dragged));
        Assert.Equal(MouseAction.Moved, dragged.Action);
    }

    [Fact]
    public void GarbageIsRejected()
    {
        Assert.False(EscapeSequenceParser.TryParseMouse("<0;10M", out _));
        Assert.False(EscapeSequenceParser.TryParseMouse("0;10;5M", out _));
        Assert.False(EscapeSequenceParser.TryParseMouse("<a;b;cM", out _));
    }

    [Fact]
    public void CursorAndFunctionKeysAreParsed()
    {
        Assert.True(EscapeSequenceParser.TryParseKey("A", out var up));
        Assert.Equal(ConsoleKey.UpArrow, up.Key);

        Assert.True(EscapeSequenceParser.TryParseKey("5~", out var pageUp));
        Assert.Equal(ConsoleKey.PageUp, pageUp.Key);

        Assert.True(EscapeSequenceParser.TryParseKey("15~", out var f5));
        Assert.Equal(ConsoleKey.F5, f5.Key);

        Assert.True(EscapeSequenceParser.TryParseKey("1;5C", out var controlRight));
        Assert.Equal(ConsoleKey.RightArrow, controlRight.Key);
        Assert.Equal(ConsoleModifiers.Control, controlRight.Modifiers);

        Assert.True(EscapeSequenceParser.TryParseKey("Z", out var shiftTab));
        Assert.Equal(ConsoleKey.Tab, shiftTab.Key);
        Assert.Equal(ConsoleModifiers.Shift, shiftTab.Modifiers);
    }

    [Fact]
    public void LoneEscapeStillReachesTheApplication()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "x", null, static _ => { });
        app.ReadFromTerminal("\e");

        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void EscapeSequenceBecomesTheKeyItEncodes()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["a", "b", "c"], static _ => { });
        app.ReadFromTerminal("\e[B");

        Assert.Equal(1, ((OptionListModal)app.State.Modal!).Index);
    }

    [Fact]
    public void MouseSequenceReachesTheView()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Mouse);
        app.ReadFromTerminal("\e[<0;7;3M");

        Assert.Equal(new(MouseAction.Pressed, MouseButton.Left, 2, 6, default), MouseView.LastEvent);
    }

    [Fact]
    public void ViewCanNavigateFromAClick()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Mouse);
        app.ReadFromTerminal("\e[<2;1;1M");

        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void WheelScrollsAnOpenList()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["a", "b", "c"], static _ => { });

        app.ReadFromTerminal("\e[<65;3;3M");
        Assert.Equal(1, ((OptionListModal)app.State.Modal!).Index);

        app.ReadFromTerminal("\e[<64;3;3M");
        Assert.Equal(0, ((OptionListModal)app.State.Modal!).Index);
    }

    [Fact]
    public void MouseIsOnlyTurnedOnWhenAsked()
    {
        using var quiet = new TestApplication();
        Assert.False(quiet.Terminal.IsMouseEnabled);

        using var app = new TestApplication(configure: static builder => builder.UseMouse());
        Assert.True(app.Options.MouseInput);
    }

    [Fact]
    public void EventsReportedOutsideTheKeyStreamStillReachTheView()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Mouse);
        app.Terminal.EnqueueMouse(new(MouseAction.Pressed, MouseButton.Left, 3, 7, default));
        app.Terminal.EnqueueMouse(new(MouseAction.ScrolledDown, MouseButton.None, 3, 7, default));
        app.ReadFromTerminal("");

        Assert.Equal(MouseAction.ScrolledDown, MouseView.LastEvent.Action);
        Assert.Equal(3, MouseView.LastEvent.Row);
    }
}

public sealed class MouseView : IArlecchinoView
{
    public static MouseEvent LastEvent { get; private set; }

    private readonly Surface _surface;

    public MouseView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw() => _surface.AppendLine("mouse", Theme.Default);

    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;

    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        LastEvent = mouse;
        return mouse.Button == MouseButton.Right ? ViewKind.Other : ViewRoute.None;
    }
}
