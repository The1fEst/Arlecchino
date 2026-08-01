using System;
using Xunit;
using Arlecchino.Modals;

namespace Arlecchino.Tests;

public sealed class ModalStackTests
{
    [Fact]
    public void ClosingTheTopModalUncoversTheOneUnderneath()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.State.PushModal(new ToggleModal { Title = "Sure?", OnSubmit = static _ => { } });

        Assert.Equal(2, app.State.Modals.Count);
        Assert.IsType<ToggleModal>(app.State.Modal);

        app.Press(ConsoleKey.Escape);

        Assert.IsType<TextModal>(app.State.Modal);
        Assert.Equal("abc", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void KeysReachOnlyTheModalOnTop()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        var underneath = (TextModal)app.State.Modal!;
        app.State.PushModal(new TextModal { Title = "Note", OnSubmit = static _ => { } });

        app.Type("xyz");

        Assert.Equal("xyz", ((TextModal)app.State.Modal!).Text);
        Assert.Equal("", underneath.Text);
    }

    [Fact]
    public void BothModalsAreDrawnAndTheTopOneIsOffset()
    {
        using var app = new TestApplication();

        app.State.Modal = new TextModal { Title = "Below", OnSubmit = static _ => { } };
        app.State.PushModal(new TextModal { Title = "Above", OnSubmit = static _ => { } });

        var lines = app.FrameLines();
        var below = Array.FindIndex(lines, line => line.Contains("Below", StringComparison.Ordinal));
        var above = Array.FindIndex(lines, line => line.Contains("Above", StringComparison.Ordinal));

        Assert.True(below >= 0);
        Assert.True(above > below);
    }

    [Fact]
    public void AssigningAModalReplacesTheWholeStack()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.State.PushModal(new ToggleModal { Title = "Sure?", OnSubmit = static _ => { } });
        app.State.Modal = new TextModal { Title = "Fresh", OnSubmit = static _ => { } };

        Assert.Single(app.State.Modals);
    }

    [Fact]
    public void ClosingEverythingLeavesNoModal()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.State.PushModal(new ToggleModal { Title = "Sure?", OnSubmit = static _ => { } });
        app.State.CloseAllModals();

        Assert.Null(app.State.Modal);
        Assert.Empty(app.State.Modals);
    }

    [Fact]
    public void OpeningAndClosingADialogAsksForAFrameWithoutBeingTold()
    {
        using var app = new TestApplication();

        Assert.True(Changed(app, () => app.State.Modal = Dialog("first")));
        Assert.True(Changed(app, () => app.State.PushModal(Dialog("second"))));
        Assert.True(Changed(app, app.State.CloseModal));
        Assert.True(Changed(app, app.State.CloseAllModals));
    }

    [Fact]
    public void ClosingWhatIsNotOpenAsksForNothing()
    {
        using var app = new TestApplication();

        Assert.False(Changed(app, app.State.CloseModal));
        Assert.False(Changed(app, app.State.CloseAllModals));
    }

    [Fact]
    public void TheStackHandedOutIsTheOneThatKeepsChanging()
    {
        using var app = new TestApplication();
        var open = app.State.Modals;

        Assert.Empty(open);

        app.State.PushModal(Dialog("later"));

        Assert.Single(open);
    }

    [Fact]
    public void ADialogIsNotSomethingUndoStepsBackInto()
    {
        using var app = new TestApplication();

        app.State.Modal = Dialog("answered");
        app.State.PushModal(Dialog("over it"));
        app.State.CloseAllModals();

        Assert.False(app.History.CanUndo);
        Assert.Equal(0, app.History.Depth);
    }

    [Fact]
    public void ASubmittedModalCanOpenTheNextOne()
    {
        using var app = new TestApplication();
        var confirmed = false;

        app.State.RequestText("Name",
            "abc",
            null,
            _ =>
                app.State.PushModal(new ToggleModal
                {
                    Title = "Save?",
                    Value = true,
                    OnSubmit = value => confirmed = value,
                }));

        app.Press(ConsoleKey.Enter);
        Assert.IsType<ToggleModal>(app.State.Modal);

        app.Press(ConsoleKey.Enter);

        Assert.True(confirmed);
        Assert.Null(app.State.Modal);
    }

    private static bool Changed(TestApplication app, Action change)
    {
        app.Repaint.TakeRequested();
        change();

        return app.Repaint.IsRequested;
    }

    private static MessageModal Dialog(string title) => new() { Title = title, Text = title };
}
