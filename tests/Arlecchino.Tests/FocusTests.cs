using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.State;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class FocusTests
{
    private static FocusRing CreateRing(List<string> log, out FocusablePane first, out FocusablePane second)
    {
        var ring = new FocusRing(new());

        first = new(key =>
        {
            log.Add($"first:{key.Key}");
            return FocusResult.Handled;
        });

        second = new(key =>
        {
            log.Add($"second:{key.Key}");
            return FocusResult.Handled;
        });

        ring.Add(first);
        ring.Add(second);
        return ring;
    }

    [Fact]
    public void TheFirstItemAddedTakesFocus()
    {
        var ring = CreateRing([], out var first, out var second);

        Assert.True(first.IsFocused);
        Assert.False(second.IsFocused);
        Assert.Same(first, ring.Current);
    }

    [Fact]
    public void OnlyTheFocusedItemSeesKeys()
    {
        var log = new List<string>();
        var ring = CreateRing(log, out _, out _);

        ring.Handle(new('\0', ConsoleKey.A, false, false, false));
        ring.Handle(new('\0', ConsoleKey.Tab, false, false, false));
        ring.Handle(new('\0', ConsoleKey.B, false, false, false));

        Assert.Equal(["first:A", "second:B"], log);
    }

    [Fact]
    public void TabAndShiftTabWrapAround()
    {
        var ring = CreateRing([], out var first, out var second);

        ring.Handle(new('\0', ConsoleKey.Tab, false, false, false));
        Assert.True(second.IsFocused);

        ring.Handle(new('\0', ConsoleKey.Tab, false, false, false));
        Assert.True(first.IsFocused);

        ring.Handle(new('\0', ConsoleKey.Tab, true, false, false));
        Assert.True(second.IsFocused);
    }

    [Fact]
    public void FocusingAnItemDirectlyMovesTheRing()
    {
        var ring = CreateRing([], out var first, out var second);

        ring.Focus(second);

        Assert.True(second.IsFocused);
        Assert.False(first.IsFocused);
        Assert.Equal(1, ring.Index);
    }

    [Fact]
    public void RouteFromAnItemIsPassedThrough()
    {
        var ring = new FocusRing(new());
        ring.Add(new FocusablePane(static _ => FocusResult.Navigate(new("Somewhere"))));

        Assert.Equal(new("Somewhere"), ring.Handle(new('\0', ConsoleKey.Enter, false, false, false)));
    }

    [Fact]
    public void MouseGoesToTheItemThatClaimsItAndMovesFocus()
    {
        var ring = new FocusRing(new());
        var ignoring = new FocusablePane(static _ => FocusResult.Handled, static _ => FocusResult.Ignored);
        var claiming = new FocusablePane(static _ => FocusResult.Handled, static _ => FocusResult.Handled);

        ring.Add(ignoring);
        ring.Add(claiming);

        ring.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 1, 1, default));

        Assert.True(claiming.IsFocused);
        Assert.False(ignoring.IsFocused);
    }

    [Fact]
    public void FormIsAFocusableAndDimsWhenItIsNot()
    {
        using var app = new TestApplication();
        var form = new Form(app.State, app.Options)
        {
            Fields = [Field.Text(static () => "Name", new TrackedState<string>(""))],
        };

        FormHostView.Hosted = form;
        app.Navigator.Apply(ViewKind.FormHost);

        var focused = app.RawStyles();
        form.IsFocused = false;
        var blurred = app.RawStyles();

        Assert.NotEqual(focused, blurred);
    }

    [Fact]
    public void FilePickerTabSwitchesBetweenPanes()
    {
        using var app = new TestApplication(100, 26);

        app.State.FilePicker = new("Pick", PickFolder: true, Environment.CurrentDirectory, ViewRoute.None,
            static _ => { });
        app.Navigator.Apply(Routes.FilePicker);

        var listFocused = app.RawStyles();

        app.Press(ConsoleKey.Tab);
        var sidebarFocused = app.RawStyles();

        Assert.NotEqual(listFocused, sidebarFocused);

        app.Press(ConsoleKey.Tab);
        Assert.Equal(listFocused, app.RawStyles());
    }
}
