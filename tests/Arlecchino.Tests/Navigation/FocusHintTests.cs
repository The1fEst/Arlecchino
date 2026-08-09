using System;
using Xunit;
using Arlecchino.Tests.Support;
using Arlecchino.Tests.Views;

namespace Arlecchino.Tests.Navigation;

public sealed class FocusHintTests
{
    [Fact]
    public void TheHintsBoxShowsTheKeysOfThePaneThatHasTheFocus()
    {
        using var app = Open();

        Assert.Contains("f → the first pane", app.Frame(), StringComparison.Ordinal);
        Assert.DoesNotContain("the second pane", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheHintsFollowTheFocusFromPaneToPane()
    {
        using var app = Open();

        app.Press(ConsoleKey.Tab);

        Assert.Contains("s → the second pane", app.Frame(), StringComparison.Ordinal);
        Assert.DoesNotContain("the first pane", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheKeysOfTheScreenAreStillListedUnderThem()
    {
        using var app = Open();

        var frame = app.Frame();
        var pane = frame.IndexOf("the first pane", StringComparison.Ordinal);
        var screen = frame.IndexOf("leave", StringComparison.Ordinal);

        Assert.True(pane >= 0, "the focused pane states its keys");
        Assert.True(screen > pane, "the keys of the screen come after them");
    }

    private static TestApplication Open()
    {
        var app = new TestApplication(120, 40, static builder => builder.Options.ShowHints = true);

        app.Navigator.Apply(ViewKind.FocusHints);

        return app;
    }
}
