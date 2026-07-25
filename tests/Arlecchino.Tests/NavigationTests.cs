using System;
using Arlecchino.Navigation;
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
}
