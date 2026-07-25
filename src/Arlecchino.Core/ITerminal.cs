using System;
using Arlecchino.Input;

namespace Arlecchino;

/// <summary>
/// Everything the framework needs from a console. Replace it with <c>UseTerminal&lt;T&gt;()</c> to
/// drive a test harness or a remote session; <c>SystemTerminal</c> is the real one.
/// </summary>
public interface ITerminal
{
    /// <summary>Width of the window in cells.</summary>
    int Width { get; }

    /// <summary>Height of the window in rows.</summary>
    int Height { get; }

    /// <summary>Whether <see cref="ReadKey"/> can return immediately.</summary>
    bool KeyAvailable { get; }

    /// <summary>
    /// Whether <see cref="ReadMouse"/> can return immediately. Only terminals that deliver the mouse
    /// outside the key stream — the Windows console — ever report <c>true</c>; elsewhere mouse reports
    /// arrive as escape sequences among the keys.
    /// </summary>
    bool MouseAvailable { get; }

    /// <summary>Writes composed output. A frame arrives as one call.</summary>
    /// <param name="text">Text with escape sequences already embedded.</param>
    void Write(string text);

    /// <summary>Takes the next key, blocking until one arrives.</summary>
    /// <returns>The key that was pressed.</returns>
    ConsoleKeyInfo ReadKey();

    /// <summary>Takes the next mouse event. Only call it while <see cref="MouseAvailable"/> is true.</summary>
    /// <returns>What the mouse did, in frame cells.</returns>
    MouseEvent ReadMouse();

    /// <summary>Switches to the alternate screen and hides the cursor.</summary>
    void EnterFullScreen();

    /// <summary>Returns to the normal screen and shows the cursor again.</summary>
    void LeaveFullScreen();

    /// <summary>Starts reporting mouse events, if the platform supports it.</summary>
    void EnableMouse();

    /// <summary>Stops reporting mouse events.</summary>
    void DisableMouse();

    /// <summary>
    /// Asks the terminal to wrap pasted text in markers, so a paste arrives as one block instead of
    /// looking like someone typing very fast.
    /// </summary>
    void EnablePaste();

    /// <summary>Stops the paste markers.</summary>
    void DisablePaste();

    /// <summary>
    /// Puts text on the clipboard of whatever is showing the terminal, which is the machine the user
    /// is sitting at even over a remote session. Terminals may refuse it, and none report back, so
    /// there is no way to tell whether it worked.
    /// </summary>
    /// <param name="text">What to copy.</param>
    void CopyToClipboard(string text);
}
