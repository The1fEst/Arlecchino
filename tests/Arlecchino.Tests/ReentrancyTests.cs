using System;
using System.Threading.Tasks;
using Arlecchino.Navigation;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class ReentrancyTests
{
    [Fact(Timeout = 5000)]
    public async Task PostingFromInsidePostedWorkDoesNotSpinTheFrame()
    {
        using var app = new TestApplication();
        var runs = 0;

        void Again()
        {
            runs++;
            app.Dispatcher.Post(Again);
        }

        app.Dispatcher.Post(Again);

        await Task.Run(() => app.Dispatcher.RunPending(static _ => { }));

        Assert.Equal(1, runs);
    }

    [Fact]
    public void TrackingSomethingWhileTheScreenIsClosingDoesNotThrow()
    {
        using var app = new TestApplication(80, 24, static builder =>
            builder.AddView<ClosingView>("Closing"));

        app.Navigator.Apply(new("Closing"));
        app.Navigator.Apply(ViewKind.Probe);

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
    }

    public sealed class ClosingView : IArlecchinoView
    {
        private readonly ViewLifetime _lifetime;

        public ClosingView(ViewLifetime lifetime)
        {
            _lifetime = lifetime;
            _lifetime.Track(new Nested(_lifetime));
        }

        public void Draw()
        {
        }

        public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;

        private sealed class Nested : IDisposable
        {
            private readonly ViewLifetime _lifetime;

            public Nested(ViewLifetime lifetime) => _lifetime = lifetime;

            public void Dispose() => _lifetime.Track(new Leaf());
        }

        private sealed class Leaf : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
