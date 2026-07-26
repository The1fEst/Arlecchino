using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// A row of labels where one is current. The widget only tracks which that is; what each tab shows is
/// left to the view, which draws whatever fits the selection.
/// </summary>
public sealed class Tabs : IArlecchinoInteractiveWidget
{
    private const int TabPadding = 1;

    private readonly ArlecchinoKeymap _keymap;

    private SurfaceRegion _drawn;
    private int[] _starts = [];

    /// <summary>Creates the strip.</summary>
    /// <param name="keymap">Keys to obey, so the strip follows the application's bindings.</param>
    public Tabs(ArlecchinoKeymap keymap)
    {
        _keymap = keymap;
    }

    /// <summary>The labels, as delegates so a tab can show a count or a marker that changes.</summary>
    public required IReadOnlyList<Func<string>> Titles { get; init; }

    /// <summary>Called when the selection actually changes, not on every attempt to move.</summary>
    public Action<int>? OnSelected { get; init; }

    /// <summary>Index of the current tab.</summary>
    public int Selected { get; private set; }

    /// <summary>Whether the strip has focus, which decides how strongly the current tab is drawn.</summary>
    public bool IsFocused { get; set; }

    /// <summary>Switches tabs, ignoring indexes outside the strip and moves that change nothing.</summary>
    /// <param name="index">Tab to switch to.</param>
    public void Select(int index)
    {
        var clamped = Math.Clamp(index, 0, Math.Max(0, Titles.Count - 1));
        if (clamped == Selected)
        {
            return;
        }

        Selected = clamped;
        OnSelected?.Invoke(Selected);
    }

    /// <summary>
    /// Draws the labels side by side and remembers where each starts, which is what lets a click be
    /// resolved to a tab.
    /// </summary>
    /// <param name="region">Where to draw; only its first row is used.</param>
    public void Draw(SurfaceRegion region)
    {
        _drawn = region;

        if (region.IsEmpty)
        {
            return;
        }

        _starts = new int[Titles.Count];
        var column = 0;

        for (var i = 0; i < Titles.Count; i++)
        {
            var label = $"{new string(' ', TabPadding)}{Titles[i]()}{new string(' ', TabPadding)}";
            _starts[i] = column;

            var style = i == Selected
                ? IsFocused ? Theme.ActiveSelected : Theme.Selected
                : Theme.Muted;

            region.Write(0, column, label, style);
            column += TextWidth.Of(label) + 1;
        }
    }

    /// <summary>Switches tabs with the horizontal arrows, leaving everything else alone.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>What became of the key.</returns>
    public FocusResult Handle(ConsoleKeyInfo key)
    {
        if (_keymap.MoveLeft.Matches(key))
        {
            Select(Selected - 1);
            return FocusResult.Handled;
        }

        if (!_keymap.MoveRight.Matches(key))
        {
            return FocusResult.Ignored;
        }

        Select(Selected + 1);
        return FocusResult.Handled;
    }

    /// <summary>
    /// Switches to the tab that was clicked. A click in the gap between labels lands on the tab to its
    /// left, so the strip has no dead columns.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>What became of the event.</returns>
    public FocusResult HandleMouse(MouseEvent mouse)
    {
        if (_drawn.IsEmpty || !_drawn.Contains(mouse.Row, mouse.Column) ||
            mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left)
        {
            return FocusResult.Ignored;
        }

        var (_, column) = _drawn.ToLocal(mouse.Row, mouse.Column);

        for (var i = _starts.Length - 1; i >= 0; i--)
        {
            if (column < _starts[i])
            {
                continue;
            }

            Select(i);
            return FocusResult.Handled;
        }

        return FocusResult.Handled;
    }
}
