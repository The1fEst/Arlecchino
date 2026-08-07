using System;
using Microsoft.Extensions.DependencyInjection;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Navigation;

public sealed class ViewScopeTests
{
    [Fact]
    public void AScopedServiceIsBuiltPerScreenAndReleasedWithIt()
    {
        ScopedProbe.Created = 0;
        ScopedProbe.Disposed = 0;

        using var app = new TestApplication(configure: static builder =>
        {
            builder.Services.AddScoped<ScopedProbe>();
            builder.AddView<ScopedView>("Scoped");
            builder.AddView<ScopedView>("ScopedAgain");
        });

        app.Navigator.Apply(new("Scoped"));
        Assert.Equal(1, ScopedProbe.Created);
        Assert.Equal(0, ScopedProbe.Disposed);

        app.Navigator.Apply(new("ScopedAgain"));
        Assert.Equal(2, ScopedProbe.Created);
        Assert.Equal(1, ScopedProbe.Disposed);
    }

    [Fact]
    public void TwoScreensDoNotShareAScopedService()
    {
        ScopedProbe.Created = 0;

        using var app = new TestApplication(configure: static builder =>
        {
            builder.Services.AddScoped<ScopedProbe>();
            builder.AddView<ScopedView>("Scoped");
        });

        app.Navigator.Apply(new("Scoped"));
        var first = ScopedView.Last!.Probe;

        app.Navigator.Reload();
        var second = ScopedView.Last.Probe;

        Assert.NotSame(first, second);
    }

    [Fact]
    public void TheScreenIsDisposedBeforeItsScope()
    {
        ScopedProbe.Disposed = 0;
        ScopedView.DisposedBeforeProbe = false;

        using var app = new TestApplication(configure: static builder =>
        {
            builder.Services.AddScoped<ScopedProbe>();
            builder.AddView<ScopedView>("Scoped");
        });

        app.Navigator.Apply(new("Scoped"));
        app.Navigator.Apply(ViewKind.Probe);

        Assert.True(ScopedView.DisposedBeforeProbe);
    }
}

public sealed class ScopedProbe : IDisposable
{
    public static int Created { get; set; }

    public static int Disposed { get; set; }

    public ScopedProbe() => Created++;

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        Disposed++;
    }
}

public sealed class ScopedView : IArlecchinoView, IDisposable
{
    public static ScopedView? Last { get; private set; }

    public static bool DisposedBeforeProbe { get; set; }

    private readonly Surface _surface;

    public ScopedView(Surface surface, ScopedProbe probe)
    {
        _surface = surface;
        Probe = probe;
        Last = this;
    }

    public ScopedProbe Probe { get; }

    public void Draw() => _surface.AppendLine("scoped", Theme.Default);

    public ViewRoute Handle(KeyPress key) => ViewRoute.None;

    public void Dispose() => DisposedBeforeProbe = !Probe.IsDisposed;
}
