using System;
using Arlecchino.Modals.Setting;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

public sealed class MessageModalTests
{
    [Fact]
    public void AMessageIsDrawnWithItsTitleAndText()
    {
        using var app = new TestApplication();

        app.State.RequestMessage("Saved", "The profile was written to disk.");

        var frame = app.Frame();

        Assert.Contains("Saved", frame, StringComparison.Ordinal);
        Assert.Contains("The profile was written to disk.", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void LongTextWrapsInsteadOfRunningOffTheBox()
    {
        using var app = new TestApplication(60, 20);

        app.State.RequestMessage(
            "Failed",
            "The connection to the update server was refused, so nothing was downloaded and the " +
            "installed version is untouched.");

        var lines = app.FrameLines();
        var body = Array.FindAll(lines, line => line.Contains("connection", StringComparison.Ordinal));

        Assert.Single(body);
        Assert.DoesNotContain("untouched", body[0], StringComparison.Ordinal);
        Assert.Contains(lines, line => line.Contains("untouched", StringComparison.Ordinal));
    }

    [Fact]
    public void ConfirmAndCancelBothCloseIt()
    {
        using var app = new TestApplication();
        var closed = 0;

        app.State.RequestMessage("Note", "Nothing to do.", () => closed++);
        app.Press(ConsoleKey.Enter);

        Assert.Null(app.State.Modal);
        Assert.Equal(1, closed);

        app.State.RequestMessage("Note", "Nothing to do.", () => closed++);
        app.Press(ConsoleKey.Escape);

        Assert.Null(app.State.Modal);
        Assert.Equal(2, closed);
    }

    [Fact]
    public void ConfirmationOnlyRunsOnYes()
    {
        using var app = new TestApplication();
        var deleted = false;

        app.State.RequestConfirmation("Delete the profile?", () => deleted = true);

        Assert.IsType<ToggleModal>(app.State.Modal);

        app.Press(ConsoleKey.Enter);

        Assert.False(deleted);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void ConfirmationRunsWhenTheAnswerIsSwitchedToYes()
    {
        using var app = new TestApplication();
        var deleted = false;

        app.State.RequestConfirmation("Delete the profile?", () => deleted = true);

        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.Enter);

        Assert.True(deleted);
    }
}
