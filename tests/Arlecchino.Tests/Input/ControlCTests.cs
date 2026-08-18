using System;
using System.Threading;
using Arlecchino.Input;
using Arlecchino.Modals.Asking;
using Arlecchino.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Arlecchino.Tests.Input;

/// <summary>
/// Telling <c>Ctrl+C</c> from <c>Ctrl+Shift+C</c>, which type the same character. The console used to
/// answer both of them itself, so copying never reached the application on Windows.
/// </summary>
public sealed class ControlCTests
{
    [Fact]
    public void ControlCStopsTheApplication()
    {
        var lifetime = new CountingLifetime();
        using var app = Application(lifetime);

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.C, KeyModifiers.Control);

        Assert.Equal(1, lifetime.Stops);
    }

    [Fact]
    public void ControlShiftCCopiesRatherThanStopping()
    {
        var lifetime = new CountingLifetime();
        using var app = Application(lifetime);

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);

        Assert.Equal(0, lifetime.Stops);
        Assert.Equal("c", app.Terminal.CopiedText);
        Assert.Equal("abc", ((TextModal)app.State.Modal!).Text);
    }

    private static TestApplication Application(IHostApplicationLifetime lifetime) =>
        new(configure: builder => builder.Services.AddSingleton(lifetime));

    private sealed class CountingLifetime : IHostApplicationLifetime
    {
        public int Stops { get; private set; }

        public CancellationToken ApplicationStarted => CancellationToken.None;

        public CancellationToken ApplicationStopping => CancellationToken.None;

        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication() => Stops++;
    }
}
