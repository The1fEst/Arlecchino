using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Arlecchino.Input;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino;

/// <summary>
/// The real console. Registered by default and replaceable through <c>UseTerminal&lt;T&gt;()</c>.
/// On Windows it turns on virtual terminal output at startup and turns off virtual terminal input,
/// because that flag stops <c>Console.ReadKey</c> from delivering keys at all.
/// </summary>
public sealed partial class SystemTerminal : IArlecchinoTerminal
{
    private const int StandardOutputHandle = -11;
    private const int StandardInputHandle = -10;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private const uint EnableVirtualTerminalInput = 0x0200;
    private const int RedirectedWidth = 120;
    private const int RedirectedHeight = 34;

    private readonly bool _escapeSequencesWork;
    private readonly ConcurrentQueue<ConsoleKeyInfo> _unread = new();

    private volatile WindowsConsoleInput? _windowsInput;

    /// <summary>
    /// Prepares the console: UTF-8 output, hidden cursor, and escape sequences where the platform
    /// allows them. A console that refuses them drops colour to <see cref="ColorSupport.None"/>.
    /// </summary>
    public SystemTerminal()
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
        }
        catch (IOException) { }

        if (OperatingSystem.IsWindows())
        {
            TryRemoveConsoleMode(StandardInputHandle, EnableVirtualTerminalInput);
        }

        _escapeSequencesWork = !OperatingSystem.IsWindows() ||
                               TryAddConsoleMode(StandardOutputHandle, EnableVirtualTerminalProcessing);

        if (!_escapeSequencesWork)
        {
            TerminalCapabilities.Color = ColorSupport.None;
        }
    }

    internal bool EscapeSequencesWork => _escapeSequencesWork;

    /// <summary>Window width, or a fixed width when output is redirected.</summary>
    public int Width => Console.IsOutputRedirected ? RedirectedWidth : Math.Max(1, Console.WindowWidth);

    /// <summary>Window height, or a fixed height when output is redirected.</summary>
    public int Height => Console.IsOutputRedirected ? RedirectedHeight : Math.Max(1, Console.WindowHeight);

    /// <summary>
    /// Whether a key is waiting. Always false when input is redirected. With the Windows mouse on, the
    /// answer comes from the console's own event queue rather than from <c>Console</c>, because the two
    /// cannot both consume it.
    /// </summary>
    public bool KeyAvailable => !_unread.IsEmpty ||
                                (_windowsInput is { } input
                                    ? input.KeyAvailable
                                    : !Console.IsInputRedirected && Console.KeyAvailable);

    /// <summary>Whether a mouse event is waiting. Only ever true while the Windows mouse is on.</summary>
    public bool MouseAvailable => _windowsInput?.MouseAvailable ?? false;

    /// <summary>Writes a composed frame.</summary>
    /// <param name="text">Text with escape sequences already embedded.</param>
    public void Write(string text) => Console.Out.Write(text);

    /// <summary>Takes the next key without echoing it.</summary>
    /// <returns>The key that was pressed.</returns>
    public ConsoleKeyInfo ReadKey() => _unread.TryDequeue(out var back)
        ? back
        : _windowsInput is { } input
            ? input.ReadKey()
            : Console.ReadKey(true);

    /// <summary>Puts a key back so the next read returns it.</summary>
    /// <param name="key">The key to put back.</param>
    public void Unread(ConsoleKeyInfo key) => _unread.Enqueue(key);

    /// <summary>Takes the next mouse event read from the console's event queue.</summary>
    /// <returns>What the mouse did, in frame cells.</returns>
    /// <exception cref="InvalidOperationException">The mouse is not being read on this platform.</exception>
    public MouseEvent ReadMouse() =>
        _windowsInput?.ReadMouse() ?? throw new InvalidOperationException("No mouse events are being read.");

    /// <summary>Switches to the alternate screen and hides the cursor.</summary>
    public void EnterFullScreen()
    {
        if (_escapeSequencesWork)
        {
            Console.Out.Write("\e[?1049h\e[?25l");
        }
    }

    /// <summary>Returns to the normal screen and makes the cursor visible again.</summary>
    public void LeaveFullScreen()
    {
        if (_escapeSequencesWork)
        {
            Console.Out.Write("\e[?1049l\e[?25h");
        }

        try
        {
            Console.CursorVisible = true;
        }
        catch (IOException) { }
    }

    /// <summary>
    /// Starts reporting presses, releases, drags and the wheel. Elsewhere that means SGR reports mixed
    /// into the key stream; on Windows the console is read record by record instead, because the flag
    /// that delivers SGR reports there also silences the keyboard. Quick-edit mode is turned off while
    /// this is on, since otherwise the console eats clicks as text selection.
    /// </summary>
    public void EnableMouse()
    {
        if (OperatingSystem.IsWindows())
        {
            if (_windowsInput is null &&
                WindowsConsoleInput.TryStart() is { } started &&
                Interlocked.CompareExchange(ref _windowsInput, started, null) is not null)
            {
                started.Stop();
            }

            return;
        }

        if (_escapeSequencesWork)
        {
            Console.Out.Write("\e[?1000h\e[?1002h\e[?1006h");
        }
    }

    /// <summary>Stops mouse reporting and gives the console back the mode it had.</summary>
    public void DisableMouse()
    {
        if (OperatingSystem.IsWindows())
        {
            Interlocked.Exchange(ref _windowsInput, null)?.Stop();
            return;
        }

        if (_escapeSequencesWork)
        {
            Console.Out.Write("\e[?1006l\e[?1002l\e[?1000l");
        }
    }

    /// <summary>Turns on bracketed paste. Terminals that do not know the mode ignore it.</summary>
    public void EnablePaste()
    {
        if (_escapeSequencesWork)
        {
            Console.Out.Write("\e[?2004h");
        }
    }

    /// <summary>Turns bracketed paste off again.</summary>
    public void DisablePaste()
    {
        if (_escapeSequencesWork)
        {
            Console.Out.Write("\e[?2004l");
        }
    }

    /// <summary>
    /// Copies through the terminal itself, encoded as base64. This is the only way to reach the
    /// clipboard of the machine the user is actually at when the application runs over a remote
    /// session; terminals that have it switched off silently drop it.
    /// </summary>
    /// <param name="text">What to copy.</param>
    public void CopyToClipboard(string text)
    {
        if (!_escapeSequencesWork)
        {
            return;
        }

        Console.Out.Write($"\e]52;c;{Convert.ToBase64String(Encoding.UTF8.GetBytes(text))}\a");
    }

    private static void TryRemoveConsoleMode(int standardHandle, uint flag)
    {
        try
        {
            var handle = GetStdHandle(standardHandle);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1) || !GetConsoleMode(handle, out var mode))
            {
                return;
            }

            if ((mode & flag) != 0)
            {
                SetConsoleMode(handle, mode & ~flag);
            }
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
    }

    private static bool TryAddConsoleMode(int standardHandle, uint flag)
    {
        try
        {
            var handle = GetStdHandle(standardHandle);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            {
                return false;
            }

            if (!GetConsoleMode(handle, out var mode))
            {
                return Console.IsOutputRedirected;
            }

            return (mode & flag) != 0 || SetConsoleMode(handle, mode | flag);
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int handle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr handle, out uint mode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr handle, uint mode);
}
