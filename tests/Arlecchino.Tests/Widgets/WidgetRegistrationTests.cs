using Arlecchino.Tests.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

public sealed class WidgetRegistrationTests
{
    [Fact]
    public void WidgetsAreResolvableWithoutBeingRegisteredByHand()
    {
        using var app = new TestApplication(configure: static builder => builder.AddGeneratedWidgets());

        var widget = app.Services.GetRequiredService<ProbeWidget>();

        Assert.Equal("probe widget", widget.Text);
    }

    [Fact]
    public void AWidgetIsASingleton()
    {
        using var app = new TestApplication(configure: static builder => builder.AddGeneratedWidgets());

        var widget = app.Services.GetRequiredService<ProbeWidget>();
        widget.Text = "changed";

        using var scope = app.Services.CreateScope();

        Assert.Same(widget, app.Services.GetRequiredService<ProbeWidget>());
        Assert.Same(widget, scope.ServiceProvider.GetRequiredService<ProbeWidget>());
    }

    [Fact]
    public void AWidgetCanBeRegisteredOneAtATime()
    {
        using var app = new TestApplication(configure: static builder => builder.AddWidget<ProbeWidget>());

        var widget = app.Services.GetRequiredService<ProbeWidget>();

        Assert.Same(widget, app.Services.GetRequiredService<ProbeWidget>());
    }

    [Fact]
    public void AWidgetTakesItsDependenciesFromTheContainer()
    {
        using var app = new TestApplication(configure: static builder => builder
            .AddGeneratedStores()
            .AddGeneratedWidgets());

        Assert.NotNull(app.Services.GetRequiredService<ProbeReadoutWidget>());
    }
}
