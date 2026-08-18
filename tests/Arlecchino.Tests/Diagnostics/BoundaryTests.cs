using System;
using Arlecchino.Atoms.Local;
using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Modals.Telling;
using Arlecchino.Navigation;
using Arlecchino.Commands;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Testing;
using System.Collections.Generic;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Diagnostics;

public sealed class BoundaryTests
{
    [Fact]
    public void ClosingAModalWhenNoneIsOpen()
    {
        using var app = new TestApplication();

        app.State.CloseModal();
        app.State.CloseAllModals();

        Assert.Null(app.State.Modal);
        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void TheSameModalPushedTwice()
    {
        using var app = new TestApplication();
        var modal = new MessageModal { Title = "Careful", Text = "twice" };

        app.State.PushModal(modal);
        app.State.PushModal(modal);

        Assert.Equal(2, app.State.Modals.Count);

        app.Press(ConsoleKey.Escape);
        app.Press(ConsoleKey.Escape);

        Assert.Empty(app.State.Modals);
    }

    [Fact]
    public void WritingOutsideTheSurface()
    {
        var terminal = new FakeTerminal(10, 4);
        var surface = new Surface(terminal);

        surface.StartFrame();
        surface.WriteAt(-3, -3, "before the frame", Theme.Default);
        surface.WriteAt(100, 100, "after the frame", Theme.Default);
        surface.WriteAt(2, 8, "over the edge", Theme.Default);
        surface.Build();

        Assert.Equal(4, FrameText.Lines(terminal.WrittenText).Length);
    }

    [Fact]
    public void ClippingToNothing()
    {
        var terminal = new FakeTerminal(10, 4);
        var surface = new Surface(terminal);

        surface.StartFrame();

        using (surface.Clip(new(surface, 0, 0, 0, 0)))
        {
            surface.WriteAt(1, 1, "invisible", Theme.Default);
        }

        surface.Build();

        Assert.DoesNotContain("invisible", FrameText.WithoutStyles(terminal.WrittenText), StringComparison.Ordinal);
    }

    [Fact]
    public void AFormWithASingleDisabledField()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<string>("x");
        var form = new Form(app.State, app.Options)
        {
            Fields = [new() { Label = static () => "Name", Value = () => value.Value, IsEnabled = static () => false }],
        };

        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.Enter, default, '\r'));

        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void AFormWithNoFieldsAtAll()
    {
        using var app = new TestApplication();
        var form = new Form(app.State, app.Options) { Fields = [] };

        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.Enter, default, '\r'));

        var terminal = new FakeTerminal(40, 10);
        var surface = new Surface(terminal);
        surface.StartFrame();
        form.Draw(surface.Frame);
        surface.Build();

        Assert.NotNull(FrameText.Lines(terminal.WrittenText));
    }

    [Fact]
    public void ANotificationLongerThanTheScreen()
    {
        using var app = new TestApplication(40, 12);

        app.State.Output = new('x', 5_000);

        Assert.NotNull(app.Frame());

        app.Navigator.Apply(Routes.Notifications);

        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void ARouteWithAnEmptyName()
    {
        using var app = new TestApplication();
        var start = app.Navigator.CurrentRoute;

        app.Navigator.Apply(new(""));

        Assert.Equal(start, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void TwoCommandsClaimingTheSameKey()
    {
        using var app = new TestApplication(80,
            24,
            static builder => builder
                .AddView<ClashingView>("Clashing"));

        app.Navigator.Apply(new("Clashing"));
        app.Press(ConsoleKey.D);

        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void UndoWhenThereIsNothingToUndo()
    {
        using var app = new TestApplication();

        Assert.False(app.History.Undo());
        Assert.False(app.History.Redo());
    }

    public sealed class ClashingView : IArlecchinoView
    {
        private readonly Surface _surface;

        public ClashingView(Surface surface) => _surface = surface;

        public void Draw() => _surface.AppendLine("clashing", Theme.Default);

        public ViewRoute Handle(KeyPress key) => ViewRoute.None;

        public IReadOnlyList<ViewCommand> Commands() =>
        [
            ViewCommand.For(ConsoleKey.D, static () => "first", static () => { }),
            ViewCommand.For(ConsoleKey.D, static () => "second", static () => { }),
        ];
    }
}
