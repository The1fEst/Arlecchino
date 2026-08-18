using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Navigation;

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

        ring.Handle(new(ConsoleKey.A));
        ring.Handle(new(ConsoleKey.Tab));
        ring.Handle(new(ConsoleKey.B));

        Assert.Equal(["first:A", "second:B"], log);
    }

    [Fact]
    public void TabAndShiftTabWrapAround()
    {
        var ring = CreateRing([], out var first, out var second);

        ring.Handle(new(ConsoleKey.Tab));
        Assert.True(second.IsFocused);

        ring.Handle(new(ConsoleKey.Tab));
        Assert.True(first.IsFocused);

        ring.Handle(new(ConsoleKey.Tab, KeyModifiers.Shift));
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

        Assert.Equal(new("Somewhere"), ring.Handle(new(ConsoleKey.Enter)));
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
    public void TabWalksIntoANestedRingThroughItAndOutTheFarSide()
    {
        var outer = new FocusRing(new());
        var inner = new FocusRing(new());
        var first = new FocusablePane(static _ => FocusResult.Handled);
        var second = new FocusablePane(static _ => FocusResult.Handled);
        var third = new FocusablePane(static _ => FocusResult.Handled);

        inner.Add(second);
        inner.Add(third);
        outer.Add(first);
        outer.Add(inner);

        Assert.True(first.IsFocused);
        Assert.False(second.IsFocused);

        outer.Handle(new(ConsoleKey.Tab));
        Assert.True(second.IsFocused);

        outer.Handle(new(ConsoleKey.Tab));
        Assert.False(second.IsFocused);
        Assert.True(third.IsFocused);

        outer.Handle(new(ConsoleKey.Tab));
        Assert.True(first.IsFocused);
        Assert.False(third.IsFocused);
    }

    [Fact]
    public void ANestedRingIsComeBackToWhereItWasLeft()
    {
        var outer = new FocusRing(new());
        var inner = new FocusRing(new());
        var first = new FocusablePane(static _ => FocusResult.Handled);
        var second = new FocusablePane(static _ => FocusResult.Handled);
        var third = new FocusablePane(static _ => FocusResult.Handled);

        inner.Add(second);
        inner.Add(third);
        outer.Add(first);
        outer.Add(inner);

        outer.Handle(new(ConsoleKey.Tab));
        outer.Handle(new(ConsoleKey.Tab));
        outer.Handle(new(ConsoleKey.Tab));

        outer.Handle(new(ConsoleKey.Tab, KeyModifiers.Shift));

        Assert.True(third.IsFocused);
        Assert.False(second.IsFocused);
    }

    [Fact]
    public void KeysReachTheWidgetInsideANestedRing()
    {
        var log = new List<string>();
        var outer = new FocusRing(new());
        var inner = new FocusRing(new());

        outer.Add(new FocusablePane(key =>
        {
            log.Add($"outer:{key.Key}");
            return FocusResult.Handled;
        }));

        inner.Add(new FocusablePane(key =>
        {
            log.Add($"inner:{key.Key}");
            return FocusResult.Handled;
        }));

        outer.Add(inner);

        outer.Handle(new(ConsoleKey.A));
        outer.Handle(new(ConsoleKey.Tab));
        outer.Handle(new(ConsoleKey.B));

        Assert.Equal(["outer:A", "inner:B"], log);
    }

    [Fact]
    public void AClickInsideANestedRingMovesTheFocusThere()
    {
        var outer = new FocusRing(new());
        var inner = new FocusRing(new());
        var claiming = new FocusablePane(static _ => FocusResult.Handled, static _ => FocusResult.Handled);

        outer.Add(new FocusablePane(static _ => FocusResult.Handled, static _ => FocusResult.Ignored));
        inner.Add(new FocusablePane(static _ => FocusResult.Handled, static _ => FocusResult.Ignored));
        inner.Add(claiming);
        outer.Add(inner);

        outer.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 1, 1, default));

        Assert.True(inner.IsFocused);
        Assert.True(claiming.IsFocused);
    }

    [Fact]
    public void HintsAreAskedOfWhateverHoldsTheFocusDownTheChain()
    {
        var outer = new FocusRing(new());
        var inner = new FocusRing(new());

        outer.Add(new HintingPane([("a", "first")]));
        inner.Add(new HintingPane([("b", "second")]));
        outer.Add(inner);

        Assert.Equal([("a", "first")], outer.Hints());

        outer.Handle(new(ConsoleKey.Tab));

        Assert.Equal([("b", "second")], outer.Hints());
    }

    [Fact]
    public void FormIsAFocusableAndDimsWhenItIsNot()
    {
        using var app = new TestApplication();
        var form = new Form(app.State, app.Options)
        {
            Fields = [Field.Text(static () => "Name", new TrackedAtom<string>(""))],
        };

        FormHostView.Form = form;
        app.Navigator.Apply(ViewKind.FormHost);

        var withFocus = app.RawStyles();
        form.IsFocused = false;
        var withoutFocus = app.RawStyles();

        Assert.NotEqual(withFocus, withoutFocus);
    }

    [Fact]
    public void FilePickerTabSwitchesBetweenPanes()
    {
        using var app = new TestApplication(100, 26);

        app.State.FilePicker = new("Pick",
            PickFolder: true,
            Environment.CurrentDirectory,
            ViewRoute.None,
            static _ => { });
        app.Navigator.Apply(Routes.FilePicker);

        var listStyles = app.RawStyles();

        app.Press(ConsoleKey.Tab);
        var sidebarStyles = app.RawStyles();

        Assert.NotEqual(listStyles, sidebarStyles);

        app.Press(ConsoleKey.Tab);
        Assert.Equal(listStyles, app.RawStyles());
    }

    private sealed class HintingPane : IArlecchinoFocusable
    {
        private readonly (string Key, string Description)[] _hints;

        public HintingPane((string Key, string Description)[] hints)
        {
            _hints = hints;
        }

        public bool IsFocused { get; set; }

        public FocusResult Handle(KeyPress key) => FocusResult.Ignored;

        public (string Key, string Description)[] Hints() => _hints;
    }
}
