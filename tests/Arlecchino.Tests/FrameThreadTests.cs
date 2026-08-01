using System;
using System.Threading;
using Arlecchino.Atoms;
using Arlecchino.Hosting;
using Arlecchino.Modals;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Tests.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arlecchino.Tests;

public sealed class FrameThreadTests
{
    [Fact]
    public void NothingIsCheckedUntilSomeoneIsDrawing()
    {
        var value = new LocalAtom<int>(0);

        value.Value = 1;

        Assert.True(FrameThread.IsCurrent);
        Assert.Equal(1, value.Value);
    }

    [Fact]
    public void TheThreadThatClaimedDrawingMayWrite()
    {
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();

        value.Value = 1;

        Assert.Equal(1, value.Value);
    }

    [Fact]
    public void AnotherThreadWritingAnAtomIsToldWhereToPostIt()
    {
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();

        var failure = Refused(() => value.Value = 1);

        Assert.Contains("FrameThread.Post", failure.Message, StringComparison.Ordinal);
        Assert.Equal(0, value.Value);
    }

    [Fact]
    public void AnotherThreadNavigatingIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = Refused(() => app.Navigator.Apply(ViewKind.Other));

        Assert.Contains(
            FrameMembers.Of<Navigator>(nameof(Navigator.Apply)),
            failure.Message,
            StringComparison.Ordinal);

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void AnotherThreadWritingTheOutputRowIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = Refused(() => app.State.Output = "from the background");

        Assert.Contains(Member(nameof(ArlecchinoState.Output)), failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherThreadOpeningADialogIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = Refused(() => app.State.Modal = Dialog("from the background"));

        Assert.Contains(Member(nameof(ArlecchinoState.Modal)), failure.Message, StringComparison.Ordinal);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void AnotherThreadStackingADialogIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = Refused(() => app.State.PushModal(Dialog("from the background")));

        Assert.Contains(Member(nameof(ArlecchinoState.PushModal)), failure.Message, StringComparison.Ordinal);
        Assert.Empty(app.State.Modals);
    }

    [Fact]
    public void AnotherThreadClosingADialogIsToldTheSame()
    {
        using var app = new TestApplication();

        app.State.Modal = Dialog("open");

        using var drawing = FrameThread.Claim();

        Assert.Contains(
            Member(nameof(ArlecchinoState.CloseModal)),
            Refused(app.State.CloseModal).Message,
            StringComparison.Ordinal);

        Assert.Contains(
            Member(nameof(ArlecchinoState.CloseAllModals)),
            Refused(app.State.CloseAllModals).Message,
            StringComparison.Ordinal);

        Assert.NotNull(app.State.Modal);
    }

    [Fact]
    public void AnotherThreadAskingForTheFilePickerIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = Refused(() => app.State.FilePicker = new("pick", false, "C:/", ViewKind.Probe, _ => { }));

        Assert.Contains(Member(nameof(ArlecchinoState.FilePicker)), failure.Message, StringComparison.Ordinal);
        Assert.Null(app.State.FilePicker);

        Assert.Contains(
            Member(nameof(ArlecchinoState.PickerLastFolder)),
            Refused(() => app.State.PickerLastFolder = "C:/somewhere").Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherThreadRestylingTheApplicationIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        Assert.Contains(
            FrameMembers.Of(typeof(Theme), nameof(Theme.Palette)),
            Refused(() => Theme.Palette = new()).Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherThreadChangingWhatWeDrawWithIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        Assert.Contains(
            FrameMembers.Of(typeof(Glyphs), nameof(Glyphs.Graph)),
            Refused(() => Glyphs.Graph = GraphSymbols.Tty).Message,
            StringComparison.Ordinal);

        Assert.Contains(
            FrameMembers.Of(typeof(Glyphs), nameof(Glyphs.Picture)),
            Refused(() => Glyphs.Picture = ImageProtocol.Kitty).Message,
            StringComparison.Ordinal);

        Assert.Contains(
            FrameMembers.Of(typeof(Glyphs), nameof(Glyphs.CellWidth)),
            Refused(() => Glyphs.CellWidth = 7).Message,
            StringComparison.Ordinal);

        Assert.Contains(
            FrameMembers.Of(typeof(Glyphs), nameof(Glyphs.CellHeight)),
            Refused(() => Glyphs.CellHeight = 7).Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheLookAsksForAFrameWithoutBeingTold()
    {
        using var app = new TestApplication();
        var was = (Theme.Palette, Glyphs.Graph, Glyphs.CellWidth);

        try
        {
            app.Repaint.TakeRequested();
            Theme.Palette = new();

            Assert.True(app.Repaint.IsRequested);

            app.Repaint.TakeRequested();
            Glyphs.Graph = GraphSymbols.Tty;

            Assert.True(app.Repaint.IsRequested);

            app.Repaint.TakeRequested();
            Glyphs.CellWidth = 7;

            Assert.True(app.Repaint.IsRequested);
        }
        finally
        {
            (Theme.Palette, Glyphs.Graph, Glyphs.CellWidth) = was;
        }
    }

    [Fact]
    public void AskingTheContainerForTheOptionsIsNotAChangeOfLook()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        Exception? thrown = null;

        var reading = new Thread(() =>
        {
            try
            {
                _ = app.Services.GetRequiredService<ArlecchinoOptions>();
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });

        reading.Start();
        reading.Join();

        Assert.Null(thrown);
    }

    [Fact]
    public void ADialogPostedFromAnotherThreadOpensOnTheNextFrame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var posted = new Thread(() => FrameThread.Post(() => app.State.Modal = Dialog("posted")));

        posted.Start();
        posted.Join();

        Assert.Null(app.State.Modal);

        FrameThread.RunPending(static _ => { });

        Assert.Equal("posted", app.State.Modal?.Title);
    }

    [Fact]
    public void PostingFromAnotherThreadIsTheWayThrough()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();
        var posted = new Thread(() => FrameThread.Post(() => value.Value = 7));

        posted.Start();
        posted.Join();

        FrameThread.RunPending(static _ => { });

        Assert.Equal(7, value.Value);
    }

    [Fact]
    public void GivingTheClaimUpLetsAnyThreadWriteAgain()
    {
        var value = new LocalAtom<int>(0);

        using (FrameThread.Claim()) { }

        value.Value = 3;

        Assert.Equal(3, value.Value);
    }

    private static string Member(string member) => FrameMembers.Of<ArlecchinoState>(member);

    private static MessageModal Dialog(string title) => new() { Title = title, Text = title };

    private static InvalidOperationException Refused(Action write)
    {
        Exception? thrown = null;

        var writing = new Thread(() =>
        {
            try
            {
                write();
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });

        writing.Start();
        writing.Join();

        return Assert.IsType<InvalidOperationException>(thrown);
    }
}
