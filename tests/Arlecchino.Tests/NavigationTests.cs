using System;
using System.Threading.Tasks;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class NavigationTests
{
    [Fact]
    public void StartRouteIsShownOnTheFirstFrame()
    {
        using var app = new TestApplication();

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
        Assert.Contains("probe", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void RouteReturnedFromHandleNavigates()
    {
        using var app = new TestApplication();

        app.Press(ConsoleKey.O);

        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);
        Assert.Contains("other", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void NoneKeepsTheCurrentView()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewRoute.None);

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
        Assert.False(app.Navigator.CanGoBack);
    }

    [Fact]
    public void NavigatingToTheCurrentRouteDoesNotGrowHistory()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Probe);

        Assert.False(app.Navigator.CanGoBack);
    }

    [Fact]
    public void AltArrowsWalkTheHistory()
    {
        using var app = new TestApplication();

        app.Press(ConsoleKey.O);
        Assert.True(app.Navigator.CanGoBack);

        app.Press(ConsoleKey.LeftArrow, alt: true);
        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
        Assert.True(app.Navigator.CanGoForward);

        app.Press(ConsoleKey.RightArrow, alt: true);
        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void NavigatingForwardClearsTheForwardStack()
    {
        using var app = new TestApplication();

        app.Press(ConsoleKey.O);
        app.Navigator.Back();
        app.Navigator.Apply(ViewKind.Other);

        Assert.False(app.Navigator.CanGoForward);
    }

    [Fact]
    public void ReloadKeepsTheRoute()
    {
        using var app = new TestApplication();

        app.Navigator.Reload();

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
        Assert.Contains("probe", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownRouteReportsBothWaysToRegisterIt()
    {
        using var app = new TestApplication();

        var error = Assert.Throws<InvalidOperationException>(() => app.Navigator.Apply(new("Missing")));

        Assert.Contains("Missing", error.Message, StringComparison.Ordinal);
        Assert.Contains("AddView", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HintsOfTheCurrentViewAreDrawn()
    {
        using var app = new TestApplication();

        Assert.Contains("other", app.FrameLineContaining("o →"), StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedRoutesAreNamedAfterViewsWithoutTheSuffix()
    {
        Assert.Equal("Probe", ViewKind.Probe.Name);
        Assert.Equal("Other", ViewKind.Other.Name);
        Assert.True(ViewKind.None.IsNone);
    }

    [Fact]
    public void TheStartViewIsNotBuiltBeforeItIsNeeded()
    {
        CountedView.Built = 0;

        using var app = new TestApplication(configure: static builder =>
            builder.AddView<CountedView>("Counted").StartAt("Counted"));

        Assert.Equal(0, CountedView.Built);

        app.Frame();

        Assert.Equal(1, CountedView.Built);
    }

    [Fact]
    public async Task AStartViewMayAskForTheNavigatorItself()
    {
        var built = Task.Run(static () =>
        {
            using var app = new TestApplication(configure: static builder =>
                builder.AddView<NavigatingView>("Navigating").StartAt("Navigating"));

            return app.Frame();
        });

        var finished = await Task.WhenAny(built, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.True(ReferenceEquals(finished, built), "building a view that asks for the navigator hung");
        Assert.Contains("navigating", await built, StringComparison.Ordinal);
    }

    [Fact]
    public void AViewThatNavigatesWhileItIsBuiltIsReported()
    {
        using var app = new TestApplication(configure: static builder => builder
            .AddView<EagerView>("Eager")
            .StartAt("Eager"));

        var error = Assert.Throws<InvalidOperationException>(() => app.Frame());

        Assert.Contains("still being built", error.Message, StringComparison.Ordinal);
    }

    public sealed class CountedView : IArlecchinoView
    {
        public CountedView() => Built++;

        public static int Built { get; set; }

        public void Draw()
        {
        }

        public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
    }

    public sealed class NavigatingView : IArlecchinoView
    {
        private readonly Surface _surface;
        private readonly Navigator _navigator;

        public NavigatingView(Surface surface, Navigator navigator)
        {
            _surface = surface;
            _navigator = navigator;
        }

        public void Draw() => _surface.AppendLine($"navigating {_navigator.CurrentRoute.Name}", Theme.Default);

        public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
    }

    public sealed class EagerView : IArlecchinoView
    {
        public EagerView(Navigator navigator) => navigator.Apply(ViewKind.Other);

        public void Draw()
        {
        }

        public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
    }
}
