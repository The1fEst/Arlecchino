using System;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Xunit;

namespace Arlecchino.Tests;

/// <summary>
/// What the cell grid cannot express — an image in a graphics protocol — is handed to the terminal
/// verbatim. These are the claims the protocols will rest on: it lands where it was put, it goes out
/// after the cells, and it is not sent again while it stays the same.
/// </summary>
public sealed class PassthroughTests
{
    private const string Payload = "\e_Gf=100,a=T;AAAA\e\\";
    private const string Other = "\e_Gf=100,a=T;BBBB\e\\";

    [Fact]
    public void ItLandsWhereItWasPutAndAfterTheCells()
    {
        var terminal = new FakeTerminal(10, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        surface.Frame.WriteLine(0, "under", Theme.Default);
        surface.Passthrough(1, 2, Payload);
        surface.Build();

        var written = terminal.Written;

        Assert.Contains(Payload, written, StringComparison.Ordinal);
        Assert.True(
            written.IndexOf("under", StringComparison.Ordinal) < written.IndexOf(Payload, StringComparison.Ordinal),
            "the cells are written before the payload");
        Assert.Contains($"\e[2;3H{Payload}", written, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSamePayloadIsNotSentTwice()
    {
        var terminal = new FakeTerminal(10, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        Frame(surface, Payload);

        terminal.Clear();

        Frame(surface, Payload);

        Assert.DoesNotContain(Payload, terminal.Written, StringComparison.Ordinal);
    }

    [Fact]
    public void APayloadThatChangedIsSentAgain()
    {
        var terminal = new FakeTerminal(10, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        Frame(surface, Payload);

        terminal.Clear();

        Frame(surface, Other);

        Assert.Contains(Other, terminal.Written, StringComparison.Ordinal);
    }

    [Fact]
    public void APayloadTakenAwayIsNotSentAgain()
    {
        var terminal = new FakeTerminal(10, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        Frame(surface, Payload);

        terminal.Clear();

        surface.StartFrame();
        surface.Build();

        Assert.DoesNotContain(Payload, terminal.Written, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyPayloadIsNothingToSend()
    {
        var terminal = new FakeTerminal(10, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        surface.Passthrough(0, 0, "");
        surface.Build();

        Assert.Equal("", FrameText.WithoutStyles(terminal.Written).Trim());
    }

    private static void Frame(Surface surface, string payload)
    {
        surface.StartFrame();
        surface.Passthrough(1, 2, payload);
        surface.Build();
    }
}
