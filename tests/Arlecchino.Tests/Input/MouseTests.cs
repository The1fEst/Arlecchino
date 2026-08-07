using System;
using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Modals.Choosing;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

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


    /// <summary>
    /// A stretch of what a real terminal really sent, caught once from kitty with a hand on a mouse:
    /// a click on a known cell, the wheel both ways, and a drag. Mouse reports are the one thing that
    /// cannot be asked for — no amount of driving a terminal produces them, only a person does — so the
    /// way to keep them honest is to hold on to a recording. Taken with
    /// <c>tools/Arlecchino.Tools -- keys --decode</c>.
    /// </summary>
    [Fact]
    public void WhatARealMouseSentIsReadBackAsWhereItWasPointed()
    {
        const string Caught =
            "\e[<0;17;6M\e[<0;17;6m" +
            "\e[<65;24;5M\e[<64;24;5M" +
            "\e[<0;12;3M\e[<32;13;3M\e[<32;14;4M\e[<0;14;4m";

        var events = new List<MouseEvent>();
        var index = 0;

        while (index < Caught.Length)
        {
            var end = Caught.IndexOf('M', index) is var press && press >= 0 ? press : Caught.Length;
            var release = Caught.IndexOf('m', index);
            var stop = release >= 0 && release < end ? release : end;

            Assert.True(EscapeSequenceParser.TryParseMouse(Caught[(index + 2)..(stop + 1)], out var mouse));
            events.Add(mouse);
            index = stop + 1;
        }

        Assert.Equal(MouseAction.Pressed, events[0].Action);
        Assert.Equal(MouseButton.Left, events[0].Button);
        Assert.Equal(5, events[0].Row);
        Assert.Equal(16, events[0].Column);

        Assert.Equal(MouseAction.Released, events[1].Action);
        Assert.Equal(5, events[1].Row);
        Assert.Equal(16, events[1].Column);

        Assert.Equal(MouseAction.ScrolledDown, events[2].Action);
        Assert.Equal(MouseAction.ScrolledUp, events[3].Action);

        Assert.Equal(MouseAction.Pressed, events[4].Action);
        Assert.Equal(MouseAction.Moved, events[5].Action);
        Assert.Equal(MouseButton.Left, events[5].Button);
        Assert.Equal(MouseAction.Moved, events[6].Action);
        Assert.Equal(3, events[6].Row);
        Assert.Equal(13, events[6].Column);
        Assert.Equal(MouseAction.Released, events[7].Action);
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
        Assert.Equal(KeyModifiers.Control, withControl.Modifiers);

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
        Assert.Equal(KeyModifiers.Control, controlRight.Modifiers);

        Assert.True(EscapeSequenceParser.TryParseKey("Z", out var shiftTab));
        Assert.Equal(ConsoleKey.Tab, shiftTab.Key);
        Assert.Equal(KeyModifiers.Shift, shiftTab.Modifiers);
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

    public ViewRoute Handle(KeyPress key) => ViewRoute.None;

    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        LastEvent = mouse;
        return mouse.Button == MouseButton.Right ? ViewKind.Other : ViewRoute.None;
    }
}
