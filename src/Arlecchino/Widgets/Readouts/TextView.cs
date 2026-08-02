using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

using Arlecchino.Widgets.Lists;

namespace Arlecchino.Widgets.Readouts;

/// <summary>
/// A block of text to read: wrapped to the width it is given, scrolled with the movement keys and the
/// wheel. This is the widget for a description, a log, the output of something that ran — anything
/// longer than the space available and not meant to be edited.
///
/// The text is re-wrapped whenever the width changes, so resizing the terminal reflows it rather than
/// cutting it off.
/// </summary>
public sealed class TextView : IArlecchinoInteractiveWidget
{
    private readonly ScrollPane _pane;

    private List<string> _wrapped = [];
    private string _wrappedFrom = "";
    private int _wrappedTo = -1;

    /// <summary>Creates the view.</summary>
    /// <param name="keymap">Keys to obey, so scrolling follows the application's bindings.</param>
    public TextView(ArlecchinoKeymap keymap)
    {
        _pane = new(keymap)
        {
            ContentHeight = () => _wrapped.Count,
            Content = DrawLines,
        };
    }

    /// <summary>The text shown. Line breaks are kept; long lines wrap on spaces.</summary>
    public string Text { get; set; } = "";

    /// <summary>Colour of the text. The theme's default when left alone.</summary>
    public IArlecchinoColor? Style { get; set; }

    /// <summary>Whether a scroll bar is drawn when the text does not fit.</summary>
    public bool ShowScrollBar
    {
        get => _pane.ShowScrollBar;
        init => _pane.ShowScrollBar = value;
    }

    /// <summary>First wrapped line shown.</summary>
    public int Offset
    {
        get => _pane.Offset;
        set => _pane.Offset = value;
    }

    /// <summary>How many lines the text takes once wrapped to the last width it was drawn at.</summary>
    public int LineCount => _wrapped.Count;

    /// <summary>Whether the view has focus. Only a focused view answers keys.</summary>
    public bool IsFocused
    {
        get => _pane.IsFocused;
        set => _pane.IsFocused = value;
    }

    /// <summary>
    /// Wraps the text to the region and draws the part that fits. The view fills whatever it is given,
    /// so nothing is left underneath it.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region: the view uses every row it is handed.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        var width = _pane.ShowScrollBar ? Math.Max(1, region.Width - 1) : region.Width;
        Rewrap(width);

        return _pane.Draw(region);
    }

    /// <summary>Scrolls the text.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>Whether the view took it.</returns>
    public FocusResult Handle(ConsoleKeyInfo key) => _pane.Handle(key);

    /// <summary>Scrolls with the wheel while the pointer is over the text.</summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>Whether the view took it.</returns>
    public FocusResult HandleMouse(MouseEvent mouse) => _pane.HandleMouse(mouse);

    private void Rewrap(int width)
    {
        if (_wrappedTo == width && _wrappedFrom == Text)
        {
            return;
        }

        _wrapped = TextWidth.Wrap(Text, width);
        _wrappedFrom = Text;
        _wrappedTo = width;
    }

    private void DrawLines(SurfaceRegion region)
    {
        var style = Style ?? Theme.Default;

        for (var row = 0; row < _wrapped.Count; row++)
        {
            region.WriteLine(row, _wrapped[row], style);
        }
    }
}
