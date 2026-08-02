using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;
using Xunit;

namespace Arlecchino.Tests.Atoms;

public sealed class PostedAtomTests
{
    [Fact]
    public void APostedValueLandsWhenTheFrameRunsWhatIsWaiting()
    {
        var rows = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();

        FromAnotherThread(() => rows.Post(7));

        Assert.Equal(0, rows.Value);

        FrameThread.RunPending(static _ => { });

        Assert.Equal(7, rows.Value);
    }

    [Fact]
    public void APostedValueNotifiesLikeAnyOtherWrite()
    {
        var name = new LocalAtom<string>("");
        var seen = new List<string>();

        using var subscription = name.Subscribe(() => seen.Add(name.Value));
        using var drawing = FrameThread.Claim();

        FromAnotherThread(() => name.Post("loaded"));
        FrameThread.RunPending(static _ => { });

        Assert.Equal(["loaded"], seen);
    }

    [Fact]
    public void PostedValuesArriveInTheOrderTheyWerePosted()
    {
        var count = new LocalAtom<int>(0);
        var seen = new List<int>();

        using var subscription = count.Subscribe(() => seen.Add(count.Value));
        using var drawing = FrameThread.Claim();

        FromAnotherThread(() =>
        {
            count.Post(1);
            count.Post(2);
            count.Post(3);
        });

        FrameThread.RunPending(static _ => { });

        Assert.Equal([1, 2, 3], seen);
    }

    [Fact]
    public void APostedEditIsUndoneLikeAnyOther()
    {
        using var history = new AtomHistory();
        var name = new TrackedAtom<string>("before");

        using var drawing = FrameThread.Claim();

        FromAnotherThread(() => name.Post("after"));
        FrameThread.RunPending(static _ => { });

        Assert.Equal("after", name.Value);

        history.Undo();

        Assert.Equal("before", name.Value);
    }

    [Fact]
    public void PostingIsHowAnAtomIsWrittenFromAnotherThreadAtAll()
    {
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();

        Exception? refused = null;

        FromAnotherThread(() =>
        {
            try
            {
                value.Value = 1;
            }
            catch (Exception exception)
            {
                refused = exception;
            }

            value.Post(2);
        });

        FrameThread.RunPending(static _ => { });

        Assert.IsType<InvalidOperationException>(refused);
        Assert.Equal(2, value.Value);
    }

    private static void FromAnotherThread(Action work)
    {
        var thread = new Thread(() => work());

        thread.Start();
        thread.Join();
    }
}
