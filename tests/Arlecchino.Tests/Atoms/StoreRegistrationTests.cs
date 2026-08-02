using Arlecchino.Tests.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Atoms;

public sealed class StoreRegistrationTests
{
    [Fact]
    public void StoresAreResolvableWithoutBeingRegisteredByHand()
    {
        using var app = new TestApplication(configure: static builder => builder.AddGeneratedStores());

        var store = app.Services.GetRequiredService<ProbeStore>();

        Assert.Equal("probe", store.Name.Value);
        Assert.Same(store, app.Services.GetRequiredService<ProbeStore>());
    }

    [Fact]
    public void AScopedStoreIsBuiltOncePerScope()
    {
        using var app = new TestApplication(configure: static builder => builder.AddGeneratedStores());

        using var first = app.Services.CreateScope();
        using var second = app.Services.CreateScope();

        var inFirst = first.ServiceProvider.GetRequiredService<ScopedProbeStore>();

        Assert.Same(inFirst, first.ServiceProvider.GetRequiredService<ScopedProbeStore>());
        Assert.NotSame(inFirst, second.ServiceProvider.GetRequiredService<ScopedProbeStore>());
        Assert.Same(app.State, inFirst.State);
    }

    [Fact]
    public void AStoreCanBeTakenByAView()
    {
        using var app = new TestApplication(configure: static builder => builder.AddGeneratedStores());

        var store = app.Services.GetRequiredService<ProbeStore>();
        store.Name.Value = "renamed";

        Assert.Equal("renamed", app.Services.GetRequiredService<ProbeStore>().Name.Value);
    }
}
