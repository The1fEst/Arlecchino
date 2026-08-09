using Arlecchino.Focus;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Rendering;

public sealed class PaneTreeMouseTests
{
    [Fact]
    public void AClickGoesToThePaneItLandedIn()
    {
        var left = new Greedy();
        var right = new Greedy();
        var layout = Branch(Columns, 0.5, Leaf(left), Leaf(right));

        layout.Draw(Frame(80, 24));
        layout.HandleMouse(Click(row: 5, column: 60));

        Assert.Equal(0, left.Clicks);
        Assert.Equal(1, right.Clicks);
    }

    [Fact]
    public void ThePaneThatWasClickedTakesTheFocus()
    {
        using var app = new TestApplication();

        var top = new Greedy();
        var bottom = new Greedy();
        var layout = Branch(Rows, 0.5, Leaf(top), Leaf(bottom));
        var ring = layout.AsFocusRing(app.Options.Keymap);

        layout.Draw(Frame(80, 24));

        Assert.True(top.IsFocused);

        layout.HandleMouse(Click(row: 20, column: 10));

        Assert.True(bottom.IsFocused);
        Assert.False(top.IsFocused);
        Assert.Same(bottom, ring.Current);
    }

    [Fact]
    public void ThePaneAnswersWhereItWasPutRatherThanWhereItWasFirstDrawn()
    {
        var left = new Greedy();
        var right = new Greedy();
        var layout = Branch(Columns, 0.25, Leaf(left), Leaf(right));

        layout.Draw(Frame(80, 24));
        layout.Draw(Frame(40, 24));

        layout.HandleMouse(Click(row: 3, column: 15));

        Assert.Equal(0, left.Clicks);
        Assert.Equal(1, right.Clicks);
    }

    [Fact]
    public void AClickInTheGapBetweenPanesBelongsToNeither()
    {
        var left = new Greedy();
        var right = new Greedy();
        var layout = Branch(Columns, 0.5, Leaf(left), Leaf(right)).Gaps(inner: 2);

        layout.Draw(Frame(80, 24));

        Assert.Equal(ViewRoute.None, layout.HandleMouse(Click(row: 5, column: 40)));

        Assert.Equal(0, left.Clicks);
        Assert.Equal(0, right.Clicks);
    }

    [Fact]
    public void NoPaneAnswersBeforeTheFirstFrameIsDrawn()
    {
        var only = new Greedy();

        Leaf(only).HandleMouse(Click(row: 1, column: 1));

        Assert.Equal(0, only.Clicks);
    }

    [Fact]
    public void ThePaneKeepsTheRouteItAnsweredWith()
    {
        var layout = Branch(Columns, 0.5, Leaf(new Greedy()), Leaf(new Greedy { Route = new("Somewhere") }));

        layout.Draw(Frame(80, 24));

        Assert.Equal(new("Somewhere"), layout.HandleMouse(Click(row: 5, column: 60)));
    }

    private static MouseEvent Click(int row, int column) =>
        new(MouseAction.Pressed, MouseButton.Left, row, column, default);

    private static SurfaceRegion Frame(int width, int height)
    {
        var surface = new Surface(new FakeTerminal(width, height))
        {
            HorizontalPadding = 0,
            VerticalPadding = 0,
        };

        surface.StartFrame();

        return surface.Frame;
    }

    /// <summary>
    /// A widget that claims every event it is handed, wherever it landed. Which pane sees a click is
    /// then the tree's answer alone, rather than the widget's own guess about where it was drawn.
    /// </summary>
    private sealed class Greedy : IArlecchinoInteractiveWidget
    {
        public int Clicks { get; private set; }

        public ViewRoute Route { get; init; } = ViewRoute.None;

        public bool IsFocused { get; set; }

        public SurfaceRegion Draw(SurfaceRegion region) => region;

        public FocusResult Handle(KeyPress key) => FocusResult.Ignored;

        public FocusResult HandleMouse(MouseEvent mouse)
        {
            Clicks++;

            return Route == ViewRoute.None ? FocusResult.Handled : FocusResult.Navigate(Route);
        }
    }
}
