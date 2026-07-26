using System;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// A window onto content taller than the space it has. Lists scroll themselves, but a block of text,
/// a long form or a pane of anything at all does not — this is the widget for those: it draws the
/// content shifted up by the offset, confines it to its own rectangle, and answers the movement keys
/// and the wheel.
///
/// The content is drawn by a delegate rather than owned, so whatever can paint a region can live in
/// here, including other widgets.
/// </summary>
public sealed class ScrollPane : IArlecchinoInteractiveWidget
{
    private const int PageRows = 10;

    private readonly ArlecchinoKeymap _keymap;

    private SurfaceRegion _drawn;
    private int _offset;

    /// <summary>Creates the pane.</summary>
    /// <param name="keymap">Keys to obey, so the pane follows the application's bindings.</param>
    public ScrollPane(ArlecchinoKeymap keymap)
    {
        _keymap = keymap;
    }

    /// <summary>
    /// Draws the content. The region it is handed is as tall as <see cref="ContentHeight"/> and moved
    /// up by the offset, so the delegate always writes at row zero for the first line and never has to
    /// know where the window is.
    /// </summary>
    public required Action<SurfaceRegion> Content { get; init; }

    /// <summary>How many rows the content occupies, asked once per frame.</summary>
    public required Func<int> ContentHeight { get; init; }

    /// <summary>Whether a scroll bar is drawn down the last column when the content does not fit.</summary>
    public bool ShowScrollBar { get; set; } = true;

    /// <summary>First content row shown, clamped to what there is on every frame.</summary>
    public int Offset
    {
        get => _offset;
        set => _offset = Math.Max(0, value);
    }

    /// <summary>Whether the pane has focus. Only a focused pane answers keys.</summary>
    public bool IsFocused { get; set; }

    /// <summary>Draws the visible slice of the content, and the scroll bar when one is needed.</summary>
    /// <param name="region">Where to draw.</param>
    public void Draw(SurfaceRegion region)
    {
        _drawn = region;

        if (region.IsEmpty)
        {
            return;
        }

        var height = Math.Max(0, ContentHeight());
        var scrolls = height > region.Height;
        var content = scrolls && ShowScrollBar
            ? region.Inset(new Margin(0, 0, 1, 0))
            : region;

        _offset = Math.Clamp(_offset, 0, Math.Max(0, height - region.Height));

        using (region.Surface.Clip(region))
        {
            Content(content with { Top = content.Top - _offset, Height = height });
        }

        if (scrolls && ShowScrollBar)
        {
            ScrollBar.Draw(region, _offset, height);
        }
    }

    /// <summary>Moves the window a row, a page, or to either end.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>Whether the pane took it.</returns>
    public FocusResult Handle(ConsoleKeyInfo key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            return Move(-1);
        }

        if (_keymap.MoveDown.Matches(key))
        {
            return Move(1);
        }

        if (_keymap.JumpUp.Matches(key))
        {
            return Move(-PageRows);
        }

        if (_keymap.JumpDown.Matches(key))
        {
            return Move(PageRows);
        }

        if (_keymap.First.Matches(key))
        {
            _offset = 0;
            return FocusResult.Handled;
        }

        if (!_keymap.Last.Matches(key))
        {
            return FocusResult.Ignored;
        }

        _offset = int.MaxValue;
        return FocusResult.Handled;
    }

    /// <summary>Scrolls with the wheel while the pointer is over the pane.</summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>Whether the pane took it.</returns>
    public FocusResult HandleMouse(MouseEvent mouse)
    {
        if (_drawn.IsEmpty || !_drawn.Contains(mouse.Row, mouse.Column))
        {
            return FocusResult.Ignored;
        }

        return mouse.Action switch
        {
            MouseAction.ScrolledUp => Move(-1),
            MouseAction.ScrolledDown => Move(1),
            MouseAction.Pressed when mouse.Button == MouseButton.Left => FocusResult.Handled,
            _ => FocusResult.Ignored,
        };
    }

    private FocusResult Move(int rows)
    {
        _offset = Math.Max(0, _offset + rows);
        return FocusResult.Handled;
    }
}
