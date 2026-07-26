using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// A scrolling list of items, one per row. It keeps only the selected index, never a copy of the
/// items, so replacing <see cref="Items"/> between frames is a normal thing to do.
/// </summary>
/// <typeparam name="T">What the list holds.</typeparam>
public sealed class ListBox<T> : IArlecchinoInteractiveWidget
{
    private const int PageRows = 10;

    private readonly ArlecchinoKeymap _keymap;

    private SurfaceRegion _drawn;
    private ScrollWindow _window;

    /// <summary>Creates the list.</summary>
    /// <param name="keymap">Keys to obey, so the list follows the application's bindings.</param>
    public ListBox(ArlecchinoKeymap keymap)
    {
        _keymap = keymap;
    }

    /// <summary>Turns an item into its row of text. Longer text is truncated by column, not by character.</summary>
    public required Func<T, string> Render { get; init; }

    /// <summary>Colours an item. Ignored for the selected row, which has to stand out.</summary>
    public Func<T, IArlecchinoColor>? ItemStyle { get; set; }

    /// <summary>
    /// What confirming an item does. Returning a route navigates; without this the list simply reports
    /// the key as handled.
    /// </summary>
    public Func<T, ViewRoute>? OnActivate { get; init; }

    /// <summary>What to show. Replacing this pulls the selection back into range on the next frame.</summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>Index of the selected row.</summary>
    public int Selected { get; set; }

    /// <summary>Whether the list has focus, which decides how strongly the selection is drawn.</summary>
    public bool IsFocused { get; set; }

    /// <summary>The selected item, or the type's default when the list is empty.</summary>
    public T? SelectedItem => Items.Count == 0 ? default : Items[Math.Clamp(Selected, 0, Items.Count - 1)];

    /// <inheritdoc />
    [Obsolete(
        "Draw is replaced by Place, which draws the same thing and returns the region left over. " +
        "Draw is removed in 2.0, where Place takes its name.",
        DiagnosticId = ArlecchinoDiagnostics.ObsoleteDraw)]
    public void Draw(SurfaceRegion region) => Place(region);

    /// <summary>
    /// Draws the rows around the selection and remembers where they landed, which is what lets clicks
    /// and wheel events be resolved afterwards. The list fills whatever it is given, so nothing is left
    /// underneath it.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region: the list uses every row it is handed.</returns>
    public SurfaceRegion Place(SurfaceRegion region)
    {
        _drawn = region;

        if (region.IsEmpty)
        {
            return region;
        }

        Selected = Math.Clamp(Selected, 0, Math.Max(0, Items.Count - 1));
        _window = ScrollWindow.Around(Selected, Items.Count, region.Height);

        var barred = ScrollBar.IsNeeded(Items.Count, region.Height);
        var textWidth = barred ? Math.Max(0, region.Width - 1) : region.Width;

        for (var row = 0; row < _window.Count; row++)
        {
            var index = _window.First + row;

            if (index >= Items.Count)
            {
                DrawFaults.RowVanishedWhileDrawing();
                break;
            }

            var item = Items[index];
            var text = TextWidth.PadRight(TextWidth.Truncate(Render(item), textWidth), textWidth);

            region.Write(row, 0, text, StyleOf(item, index));
        }

        if (barred)
        {
            ScrollBar.Draw(region, _window.First, Items.Count);
        }

        return region.Rows(region.Height, 0);
    }

    /// <summary>Moves the selection or confirms it.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>What became of the key, including a route when confirming navigates.</returns>
    public FocusResult Handle(ConsoleKeyInfo key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            Move(-1);
        }
        else if (_keymap.MoveDown.Matches(key))
        {
            Move(1);
        }
        else if (_keymap.JumpUp.Matches(key))
        {
            Move(-PageRows);
        }
        else if (_keymap.JumpDown.Matches(key))
        {
            Move(PageRows);
        }
        else if (_keymap.First.Matches(key))
        {
            Selected = 0;
        }
        else if (_keymap.Last.Matches(key))
        {
            Selected = Math.Max(0, Items.Count - 1);
        }
        else if (_keymap.Confirm.Matches(key))
        {
            return Activate();
        }
        else
        {
            return FocusResult.Ignored;
        }

        return FocusResult.Handled;
    }

    /// <summary>
    /// Scrolls with the wheel and selects with a click. Clicking the already selected row confirms it,
    /// so a double click reads as select-then-activate without the widget timing anything.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>What became of the event, including a route when a row was activated.</returns>
    public FocusResult HandleMouse(MouseEvent mouse)
    {
        if (_drawn.IsEmpty || !_drawn.Contains(mouse.Row, mouse.Column))
        {
            return FocusResult.Ignored;
        }

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                Move(-1);
                return FocusResult.Handled;
            case MouseAction.ScrolledDown:
                Move(1);
                return FocusResult.Handled;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left:
                var (row, _) = _drawn.ToLocal(mouse.Row, mouse.Column);
                var index = _window.First + row;

                if (index < 0 || index >= Items.Count)
                {
                    return FocusResult.Handled;
                }

                var wasSelected = index == Selected;
                Selected = index;
                return wasSelected ? Activate() : FocusResult.Handled;
            default:
                return FocusResult.Ignored;
        }
    }

    private FocusResult Activate() =>
        SelectedItem is { } item && OnActivate is not null
            ? FocusResult.Navigate(OnActivate(item))
            : FocusResult.Handled;

    private void Move(int delta) =>
        Selected = Math.Clamp(Selected + delta, 0, Math.Max(0, Items.Count - 1));

    private IArlecchinoColor StyleOf(T item, int index)
    {
        if (index != Selected)
        {
            return ItemStyle?.Invoke(item) ?? Theme.Default;
        }

        return IsFocused ? Theme.ActiveSelected : Theme.Selected;
    }
}
