using System;
using Arlecchino.Hosting;
using Arlecchino.Input;

namespace Arlecchino.Diagnostics;

/// <summary>
/// Whether the log is being shown and where it is scrolled to. Kept apart from the buffer so that
/// collecting lines costs nothing while the overlay is closed, which is nearly always.
/// </summary>
internal sealed class LogOverlay
{
    private readonly Repaint _repaint;

    private bool _isVisible;
    private int _scroll;

    /// <summary>Creates the overlay state.</summary>
    /// <param name="buffer">The lines to show.</param>
    /// <param name="repaint">Asked for a frame when the overlay is opened, closed or scrolled.</param>
    public LogOverlay(LogBuffer buffer, Repaint repaint)
    {
        Buffer = buffer;
        _repaint = repaint;
    }

    /// <summary>The lines being shown.</summary>
    public LogBuffer Buffer { get; }

    /// <summary>Whether the overlay is showing.</summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            _repaint.Request();
        }
    }

    /// <summary>
    /// How many lines back from the newest the view is scrolled. Zero means pinned to the bottom, so a
    /// line arriving while the overlay is open is seen straight away.
    /// </summary>
    public int Scroll
    {
        get => _scroll;
        set
        {
            _scroll = Math.Max(0, value);
            _repaint.Request();
        }
    }

    /// <summary>Opens the overlay, or closes it when it is already open. Opening pins it to the newest line.</summary>
    public void Toggle()
    {
        _scroll = 0;
        IsVisible = !_isVisible;
    }

    /// <summary>
    /// Scrolling and closing. Only these keys are taken while the overlay is open, so the screen behind
    /// it keeps working — it is for reading, not a mode to get stuck in.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    /// <param name="keymap">Keys to obey.</param>
    /// <returns><c>true</c> when the overlay took it.</returns>
    public bool Handle(KeyPress key, ArlecchinoKeymap keymap)
    {
        if (keymap.Cancel.Matches(key))
        {
            IsVisible = false;

            return true;
        }

        if (keymap.MoveUp.Matches(key))
        {
            Scroll++;

            return true;
        }

        if (keymap.MoveDown.Matches(key))
        {
            Scroll--;

            return true;
        }

        if (keymap.Last.Matches(key))
        {
            Scroll = 0;

            return true;
        }

        if (!keymap.Erase.Matches(key))
        {
            return false;
        }

        Buffer.Clear();
        Scroll = 0;

        return true;
    }
}
