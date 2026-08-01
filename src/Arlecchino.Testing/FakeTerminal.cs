using System;
using System.Collections.Concurrent;
using System.Text;
using Arlecchino.Input;

namespace Arlecchino.Testing;

/// <summary>
/// A terminal that keeps everything in memory: keys are queued in, output is collected as text, and
/// the size is whatever a test sets it to. Nothing is written anywhere, so tests can run side by side
/// and assert on what would have been drawn. The input queues are concurrent, so a test can deliver
/// keys late — the way a real terminal splits an escape sequence across two reads.
/// </summary>
public sealed class FakeTerminal : IArlecchinoTerminal
{
    private readonly ConcurrentQueue<ConsoleKeyInfo> _keys = new();
    private readonly ConcurrentQueue<ConsoleKeyInfo> _unread = new();
    private readonly ConcurrentQueue<MouseEvent> _mouse = new();
    private readonly StringBuilder _written = new();

    /// <summary>Creates the terminal at a fixed size.</summary>
    /// <param name="width">Columns.</param>
    /// <param name="height">Rows.</param>
    public FakeTerminal(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>Columns. Assigning simulates a resize.</summary>
    public int Width { get; set; }

    /// <summary>Rows. Assigning simulates a resize.</summary>
    public int Height { get; set; }

    /// <summary>Whether the application took over the screen and has not given it back.</summary>
    public bool IsFullScreen { get; private set; }

    /// <summary>Whether the application asked for the mouse.</summary>
    public bool IsMouseEnabled { get; private set; }

    /// <summary>Whether the application asked for bracketed paste.</summary>
    public bool IsPasteEnabled { get; private set; }

    /// <summary>The last text copied, or <c>null</c> when nothing has been.</summary>
    public string? Copied { get; private set; }

    /// <summary>Whether any queued key is still waiting.</summary>
    public bool KeyAvailable => !_unread.IsEmpty || !_keys.IsEmpty;

    /// <summary>Whether any queued mouse event is still waiting.</summary>
    public bool MouseAvailable => !_mouse.IsEmpty;

    /// <summary>Everything written so far, escape sequences included.</summary>
    public string Written => _written.ToString();

    /// <summary>Queues a key press to be read.</summary>
    /// <param name="key">The key press.</param>
    public void Enqueue(ConsoleKeyInfo key) => _keys.Enqueue(key);

    /// <summary>
    /// Queues text one character at a time, as a terminal reports it. Escapes are marked as such, so
    /// whole escape sequences can be fed in as a plain string.
    /// </summary>
    /// <param name="text">The characters to queue.</param>
    public void EnqueueText(string text)
    {
        foreach (var character in text)
        {
            _keys.Enqueue(new(character, character == '\e' ? ConsoleKey.Escape : default, false, false, false));
        }
    }

    /// <summary>Collects output instead of showing it.</summary>
    /// <param name="text">What was written.</param>
    public void Write(string text) => _written.Append(text);

    /// <summary>Takes the next queued key, or nothing when the queue has run dry.</summary>
    /// <returns>The key press.</returns>
    public ConsoleKeyInfo ReadKey() => _unread.TryDequeue(out var back) ? back
        : _keys.TryDequeue(out var key) ? key : default;

    /// <summary>Puts a key back so the next read returns it.</summary>
    /// <param name="key">The key to put back.</param>
    public void Unread(ConsoleKeyInfo key) => _unread.Enqueue(key);

    /// <summary>
    /// Queues a mouse event to be read, the way a console that reports the mouse outside the key
    /// stream delivers one.
    /// </summary>
    /// <param name="mouse">The event.</param>
    public void EnqueueMouse(MouseEvent mouse) => _mouse.Enqueue(mouse);

    /// <summary>Takes the next queued mouse event, or nothing when the queue has run dry.</summary>
    /// <returns>The event.</returns>
    public MouseEvent ReadMouse() => _mouse.TryDequeue(out var mouse) ? mouse : default;

    /// <summary>Records that the screen was taken over.</summary>
    public void EnterFullScreen() => IsFullScreen = true;

    /// <summary>Records that the screen was given back, which is what a test checks after a crash.</summary>
    public void LeaveFullScreen() => IsFullScreen = false;

    /// <summary>Records that the mouse was asked for.</summary>
    public void EnableMouse() => IsMouseEnabled = true;

    /// <summary>Records that the mouse was released.</summary>
    public void DisableMouse() => IsMouseEnabled = false;

    /// <summary>Records that bracketed paste was asked for.</summary>
    public void EnablePaste() => IsPasteEnabled = true;

    /// <summary>Records that bracketed paste was turned off.</summary>
    public void DisablePaste() => IsPasteEnabled = false;

    /// <summary>Keeps what was copied instead of reaching a real clipboard.</summary>
    /// <param name="text">What was copied.</param>
    public void CopyToClipboard(string text) => Copied = text;

    /// <summary>
    /// Throws away what has been written, so the next assertion sees one frame rather than all of them.
    /// </summary>
    public void Clear() => _written.Clear();
}
