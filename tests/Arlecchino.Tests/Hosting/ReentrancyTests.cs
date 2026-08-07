using System;
using System.Threading.Tasks;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Hosting;

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
            FrameThread.Post(Again);
        }

        FrameThread.Post(Again);

        await Task.Run(() => FrameThread.RunPending(static _ => { }));

        Assert.Equal(1, runs);
    }

    [Fact]
    public void TrackingSomethingWhileTheScreenIsClosingDoesNotThrow()
    {
        using var app = new TestApplication(80,
            24,
            static builder =>
                builder.AddView<ClosingView>("Closing"));

        app.Navigator.Apply(new("Closing"));
        app.Navigator.Apply(ViewKind.Probe);

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
    }

    public sealed class ClosingView : IArlecchinoView
    {
        public ClosingView(ViewLifetime lifetime) => lifetime.Track(new Nested(lifetime));

        public void Draw() { }

        public ViewRoute Handle(KeyPress key) => ViewRoute.None;

        private sealed class Nested : IDisposable
        {
            private readonly ViewLifetime _lifetime;

            public Nested(ViewLifetime lifetime) => _lifetime = lifetime;

            public void Dispose() => _lifetime.Track(new Leaf());
        }

        private sealed class Leaf : IDisposable
        {
            public void Dispose() { }
        }
    }
}
