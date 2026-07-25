using System;
using Arlecchino.State;
using Xunit;

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
    public void ASubmittedModalCanOpenTheNextOne()
    {
        using var app = new TestApplication();
        var confirmed = false;

        app.State.RequestText("Name", "abc", null, _ =>
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
}
