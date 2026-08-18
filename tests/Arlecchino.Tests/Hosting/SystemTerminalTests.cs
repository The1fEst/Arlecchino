using System;
using Arlecchino.Rendering.Terminals;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Hosting;

public sealed class SystemTerminalTests : IDisposable
{
    private const int RedirectedWidth = 120;
    private const int RedirectedHeight = 34;

    private readonly ColorSupportScope _colors = new(TerminalCapabilities.Color);
    private readonly SystemTerminal _terminal = new();

    public void Dispose() => _colors.Dispose();

    [Fact]
    public void RedirectedOutputGetsAFixedSizeInsteadOfAWindow()
    {
        if (!Console.IsOutputRedirected)
        {
            return;
        }

        Assert.Equal(RedirectedWidth, _terminal.Width);
        Assert.Equal(RedirectedHeight, _terminal.Height);
    }

    [Fact]
    public void SizeIsNeverZero()
    {
        Assert.True(_terminal.Width > 0);
        Assert.True(_terminal.Height > 0);
    }

    [Fact]
    public void NoKeyIsWaitingWhenInputIsRedirected()
    {
        if (!Console.IsInputRedirected)
        {
            return;
        }

        Assert.False(_terminal.KeyAvailable);
    }

    [Fact]
    public void TheMouseIsOffUntilItIsTurnedOn()
    {
        Assert.False(_terminal.MouseAvailable);
        Assert.Throws<InvalidOperationException>(() => _terminal.ReadMouse());
    }

    [Fact]
    public void FramesGoToTheConsole()
    {
        using var output = new ConsoleOutputScope();

        _terminal.Write("frame");

        Assert.Equal("frame", output.WrittenText);
    }

    /// <summary>
    /// The keyboard protocol is not among what is asked for. It moves the function keys to a shape
    /// <c>Console.ReadKey</c> reads as the wrong key, and that call sees the bytes first.
    /// </summary>
    [Fact]
    public void FullScreenSwitchesBuffersAndHidesTheCursor()
    {
        using var output = new ConsoleOutputScope();

        _terminal.EnterFullScreen();

        Assert.Equal(Expected("\e[?1049h\e[?25l"), output.WrittenText);
    }

    [Fact]
    public void LeavingFullScreenSwitchesBackAndShowsTheCursor()
    {
        using var output = new ConsoleOutputScope();

        _terminal.LeaveFullScreen();

        Assert.StartsWith(Expected("\e[?1049l\e[?25h"), output.WrittenText);
    }

    [Fact]
    public void PasteIsBracketedWhileItIsOn()
    {
        using var output = new ConsoleOutputScope();

        _terminal.EnablePaste();
        _terminal.DisablePaste();

        Assert.Equal(Expected("\e[?2004h\e[?2004l"), output.WrittenText);
    }

    [Fact]
    public void MouseReportingIsAskedForWithSequencesAwayFromWindows()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        using var output = new ConsoleOutputScope();

        _terminal.EnableMouse();
        _terminal.DisableMouse();

        Assert.Equal(Expected("\e[?1000h\e[?1002h\e[?1006h\e[?1006l\e[?1002l\e[?1000l"), output.WrittenText);
    }

    [Fact]
    public void WindowsReadsTheMouseFromTheConsoleRatherThanFromSequences()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var output = new ConsoleOutputScope();

        _terminal.EnableMouse();
        _terminal.DisableMouse();

        Assert.Equal(string.Empty, output.WrittenText);
    }

    [Fact]
    public void CopyingGoesThroughTheTerminalAsBase64()
    {
        using var output = new ConsoleOutputScope();

        _terminal.CopyToClipboard("привет");

        var sequence = Convert.ToBase64String("привет"u8);
        Assert.Equal(Expected($"\e]52;c;{sequence}\a"), output.WrittenText);
    }

    [Fact]
    public void ACopyOfNothingIsStillAValidRequest()
    {
        using var output = new ConsoleOutputScope();

        _terminal.CopyToClipboard(string.Empty);

        Assert.Equal(Expected("\e]52;c;\a"), output.WrittenText);
    }

    private string Expected(string sequences) => _terminal.EscapeSequencesWork ? sequences : string.Empty;
}
