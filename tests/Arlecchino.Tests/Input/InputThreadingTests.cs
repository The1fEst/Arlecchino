using System.Threading.Tasks;
using Arlecchino.Input;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Choosing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class InputThreadingTests
{
    [Fact]
    public void TheReaderQueuesRatherThanTouchingTheState()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["alpha", "beta", "gamma"], static _ => { });
        app.Terminal.EnqueueText("\e[B");
        app.Services.GetRequiredService<TerminalInputReader>().ReadPending();

        Assert.Equal(0, ((ChoiceModal)app.State.Modal!).Index);

        app.DrainInput();

        Assert.Equal(1, ((ChoiceModal)app.State.Modal!).Index);
    }

    [Fact]
    public async Task ReadingFromAnotherThreadChangesNothingUntilTheFrameLoopLooks()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["alpha", "beta", "gamma"], static _ => { });
        app.Terminal.EnqueueText("\e[B\e[B");

        await Task.Run(() => app.Services.GetRequiredService<TerminalInputReader>().ReadPending());

        Assert.Equal(0, ((ChoiceModal)app.State.Modal!).Index);

        app.DrainInput();

        Assert.Equal(2, ((ChoiceModal)app.State.Modal!).Index);
    }

    [Fact]
    public void DrawingAFrameRoutesWhatIsWaiting()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["alpha", "beta"], static _ => { });
        app.Terminal.EnqueueText("\e[B");
        app.Services.GetRequiredService<TerminalInputReader>().ReadPending();

        app.Frame();

        Assert.Equal(1, ((ChoiceModal)app.State.Modal!).Index);
    }

    [Fact]
    public void EverythingReadIsRoutedInOrder()
    {
        using var app = new TestApplication();
        var result = "";

        app.State.RequestText("Name", "", null, value => result = value);
        app.Terminal.EnqueueText("fEst\r");
        app.Services.GetRequiredService<TerminalInputReader>().ReadPending();

        app.DrainInput();

        Assert.Equal("fEst", result);
    }

    [Fact]
    public void APastedBlockSurvivesTheQueue()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.ReadFromTerminal("\e[200~pasted whole\e[201~");

        Assert.Equal("pasted whole", ((TextModal)app.State.Modal!).Text);
    }
}
