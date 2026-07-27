using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Arlecchino.Modals;
using Arlecchino.Input;

namespace Arlecchino.Tests;

public sealed class EscapeTimeoutTests
{
    [Fact]
    public async Task AnArrowSplitAcrossTwoReadsIsStillOneKey()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["alpha", "beta", "gamma"], static _ => { });
        app.Terminal.EnqueueText("\e");

        var rest = Task.Run(async () =>
        {
            await Task.Delay(5);
            app.Terminal.EnqueueText("[B");
        });

        app.Services.GetRequiredService<TerminalInputReader>().ReadPending();
        app.DrainInput();
        await rest;

        Assert.Equal(1, ((ChoiceModal)app.State.Modal!).Index);
    }

    [Fact]
    public void ALoneEscapeStillCancelsAfterTheWait()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["alpha"], static _ => { });
        app.ReadFromTerminal("\e");

        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void TheWaitIsWhateverTheOptionsSay()
    {
        using var app = new TestApplication();
        app.Options.EscapeTimeout = TimeSpan.FromMilliseconds(200);

        app.State.RequestChoice("Pick", ["alpha", "beta"], static _ => { });
        app.Terminal.EnqueueText("\e");

        var rest = new Thread(() =>
        {
            Thread.Sleep(60);
            app.Terminal.EnqueueText("[B");
        });

        rest.Start();
        app.Services.GetRequiredService<TerminalInputReader>().ReadPending();
        app.DrainInput();
        rest.Join();

        Assert.Equal(1, ((ChoiceModal)app.State.Modal!).Index);
    }
}
