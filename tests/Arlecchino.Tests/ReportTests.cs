using System;
using Arlecchino.Diagnostics;
using Arlecchino.Modals;
using Arlecchino.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arlecchino.Tests;

public sealed class ReportTests
{
    [Fact]
    public void TheReportSaysWhereTheApplicationIs()
    {
        using var app = new TestApplication(90, 30);

        var report = Describe(app);

        Assert.Contains("route: Probe", report, StringComparison.Ordinal);
        Assert.Contains("size: 90×30", report, StringComparison.Ordinal);
        Assert.Contains("modals: none", report, StringComparison.Ordinal);
        Assert.Contains("colour: TrueColor", report, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReportNamesTheModalsThatAreOpen()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.State.PushModal(new MessageModal { Title = "Careful", Text = "something happened" });

        var report = Describe(app);

        Assert.Contains($"modals: {nameof(TextModal)} over {nameof(MessageModal)}", report, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReportFollowsNavigation()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(Routes.Help);

        var report = Describe(app);

        Assert.Contains("route: Help", report, StringComparison.Ordinal);
        Assert.Contains("can go back: True", report, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReportCarriesTheVersionAndThePlatform()
    {
        using var app = new TestApplication();

        var report = Describe(app);

        Assert.Contains("[Arlecchino]", report, StringComparison.Ordinal);
        Assert.Contains("version: ", report, StringComparison.Ordinal);
        Assert.Contains("runtime: ", report, StringComparison.Ordinal);
        Assert.DoesNotContain("version: unknown", report, StringComparison.Ordinal);
    }

    private static string Describe(TestApplication app) =>
        app.Services.GetRequiredService<ArlecchinoReport>().Describe();
}
