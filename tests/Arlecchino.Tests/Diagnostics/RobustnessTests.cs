using System;
using System.Threading.Tasks;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Input;
using Arlecchino.Modals.Asking;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Diagnostics;

public sealed class RobustnessTests
{
    [Fact]
    public void AClickOutsideTheFrame()
    {
        using var app = new TestApplication(40, 10);

        app.Click(-5, -5);
        app.Click(400, 400);
        app.Scroll(-1, -1, down: true);

        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void NestedUndoGroups()
    {
        using var app = new TestApplication();
        var value = new TrackedAtom<int>(0);

        using (app.History.Group())
        {
            value.Value = 1;

            using (app.History.Group())
            {
                value.Value = 2;
            }

            value.Value = 3;
        }

        var depth = app.History.Depth;
        app.History.Undo();

        Assert.Equal(1, depth);
        Assert.Equal(0, value.Value);
    }

    [Fact]
    public void AGroupThatRecordsNothing()
    {
        using var app = new TestApplication();

        using (app.History.Group()) { }

        Assert.False(app.History.CanUndo);
    }

    [Fact]
    public async Task AnAsyncAtomLoadedTwiceKeepsTheSecondAnswer()
    {
        using var app = new TestApplication();
        var loading = new AsyncAtom<string>("");
        var second = new TaskCompletionSource();

        loading.Load(async _ =>
        {
            await second.Task;
            return "first";
        });

        loading.Load(_ => Task.FromResult("second"));
        second.SetResult();

        await Task.Delay(50);
        FrameThread.RunPending(static _ => { });

        Assert.Equal("second", loading.Value);
    }

    [Fact]
    public async Task AnAsyncAtomCancelledMidFlightKeepsItsOldValue()
    {
        using var app = new TestApplication();
        var loading = new AsyncAtom<string>("before");
        var started = new TaskCompletionSource();

        loading.Load(async token =>
        {
            started.SetResult();
            await Task.Delay(2000, token);
            return "after";
        });

        await started.Task;
        loading.Cancel();
        await Task.Delay(50);
        FrameThread.RunPending(static _ => { });

        Assert.Equal("before", loading.Value);
    }

    [Fact]
    public void AValidatorThatRefusesEverythingKeepsTheModalOpen()
    {
        using var app = new TestApplication();
        var submitted = 0;

        app.State.RequestText("Name", "x", static _ => "never good enough", _ => submitted++);

        app.Press(ConsoleKey.Enter);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(0, submitted);
        Assert.NotNull(app.State.Modal);
        Assert.Contains("never good enough", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void APasteOfSomethingEnormous()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.ReadFromTerminal($"\e[200~{new string('x', 200_000)}\e[201~");

        Assert.NotNull(app.Frame());
        Assert.Equal(200_000, ((TextModal)app.State.Modal!).Text.Length);
    }

    [Fact]
    public void AnEscapeFollowedImmediatelyByASequence()
    {
        using var app = new TestApplication();

        app.ReadFromTerminal("\e");
        app.ReadFromTerminal("\e[A");

        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void AKeyBindingThatMatchesNothing()
    {
        var binding = new KeyBinding(default);

        Assert.False(binding.Matches(new(ConsoleKey.A, default, 'a')));
    }
}
