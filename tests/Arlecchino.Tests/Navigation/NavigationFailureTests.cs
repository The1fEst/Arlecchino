using System;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Navigation;

public sealed class NavigationFailureTests
{
    [Fact]
    public void AScreenThatCannotBeBuiltLeavesTheApplicationWhereItWas()
    {
        using var app = Application();
        var before = app.Navigator.CurrentRoute;

        Assert.Throws<InvalidOperationException>(() => app.Navigator.Apply(new("Broken")));

        Assert.Equal(before, app.Navigator.CurrentRoute);
        Assert.Contains("probe", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void AFailedNavigationLeavesNothingInTheHistory()
    {
        using var app = Application();

        Assert.False(app.Navigator.CanGoBack);
        Assert.Throws<InvalidOperationException>(() => app.Navigator.Apply(new("Broken")));

        Assert.False(app.Navigator.CanGoBack);
        Assert.False(app.Navigator.CanGoForward);
    }

    [Fact]
    public void TheScreenStillWorksAfterAFailedNavigation()
    {
        using var app = Application();

        Assert.Throws<InvalidOperationException>(() => app.Navigator.Apply(new("Broken")));

        app.Navigator.Apply(ViewKind.Other);

        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);
        Assert.True(app.Navigator.CanGoBack);

        app.Navigator.Back();

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void AKeyThatNavigatesIntoABrokenScreenIsReportedRatherThanFatal()
    {
        using var app = Application();

        app.Navigator.Apply(new("Gateway"));
        app.Press(ConsoleKey.B);

        Assert.Equal("Gateway", app.Navigator.CurrentRoute.Name);
        Assert.Contains(app.Options.Strings.ViewFailed("the store is missing"),
            app.Frame(),
            StringComparison.Ordinal);
    }

    private static TestApplication Application() => new(80,
        24,
        static builder => builder
            .AddView<BrokenView>("Broken")
            .AddView<GatewayView>("Gateway"));

    public sealed class BrokenView : IArlecchinoView
    {
        public BrokenView() => throw new InvalidOperationException("the store is missing");

        public void Draw() { }

        public ViewRoute Handle(KeyPress key) => ViewRoute.None;
    }

    public sealed class GatewayView : IArlecchinoView
    {
        private readonly Surface _surface;

        public GatewayView(Surface surface) => _surface = surface;

        public void Draw() => _surface.AppendLine("gateway", Theme.Default);

        public ViewRoute Handle(KeyPress key) =>
            key.Key == ConsoleKey.B ? new("Broken") : ViewRoute.None;
    }
}
