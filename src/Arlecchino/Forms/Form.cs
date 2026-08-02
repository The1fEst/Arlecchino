using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.State;
using Arlecchino.Widgets;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;

namespace Arlecchino.Forms;

/// <summary>
/// A column of fields with their values lined up, and a help line under the selected one. The form
/// holds no values of its own: the fields read and write atoms, so it draws whatever the state says
/// without any copying back and forth. Whether an edit made here can be undone is decided by the atom
/// behind the field — <see cref="TrackedAtom{T}"/> or <see cref="LocalAtom{T}"/> — not by the form.
/// </summary>
public sealed class Form : IArlecchinoInteractiveWidget
{
    private const string ValueSeparator = " = ";
    private const string ActionMarker = "> ";

    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly ArlecchinoStrings _strings;

    private readonly Dictionary<int, int> _fieldOfRow = [];

    private SurfaceRegion _lastRows;

    /// <summary>Creates the form.</summary>
    /// <param name="state">Where fields open their dialogs.</param>
    /// <param name="options">Supplies the keymap and the wording.</param>
    public Form(ArlecchinoState state, ArlecchinoOptions options)
    {
        _state = state;
        _keymap = options.Keymap;
        _strings = options.Strings;
    }

    /// <summary>The rows, top to bottom.</summary>
    public required IReadOnlyList<Field> Fields { get; init; }

    /// <summary>Index of the selected field.</summary>
    public int Selected { get; private set; }

    /// <summary>Whether the form has focus, which decides how strongly the selection is drawn.</summary>
    public bool IsFocused { get; set; } = true;

    /// <summary>The selected field, or <c>null</c> when the form has none.</summary>
    public Field? Current => Fields.Count == 0 ? null : Fields[Math.Clamp(Selected, 0, Fields.Count - 1)];

    /// <summary>
    /// Draws the fields with their labels aligned, scrolled so the selection stays in view, and returns
    /// the rows below the last one written. Buttons are left out of the alignment, since they have no
    /// value to line up against.
    /// </summary>
    /// <param name="region">Where to draw, help line included.</param>
    /// <returns>The region below the fields, which is empty when they filled it.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (Fields.Count == 0 || region.IsEmpty)
        {
            return region;
        }

        Selected = Math.Clamp(Selected, 0, Fields.Count - 1);

        var labelWidth = 0;
        foreach (var field in Fields)
        {
            if (!field.IsAction)
            {
                labelWidth = Math.Max(labelWidth, TextWidth.Of(field.Label()));
            }
        }

        var helpRows = (Current?.Help().Length ?? 0) > 0 ? 1 : 0;
        var fieldRows = Math.Max(1, region.Height - helpRows);
        var first = Math.Clamp(Selected - fieldRows / 2, 0, Math.Max(0, Fields.Count - fieldRows));

        _lastRows = region;
        _fieldOfRow.Clear();

        var row = 0;
        for (var i = 0; i < fieldRows && first + i < Fields.Count && row < region.Height; i++)
        {
            var index = first + i;
            var field = Fields[index];
            var line = field.IsAction
                ? ActionMarker + field.Label()
                : $"{TextWidth.PadRight(field.Label(), labelWidth)}{ValueSeparator}{Displayed(field)}";

            region.Write(row, 0, line, StyleOf(field, index));
            _fieldOfRow[row] = index;
            row++;

            if (index != Selected || helpRows == 0 || row >= region.Height)
            {
                continue;
            }

            region.Write(row, 2, field.Help(), Theme.Muted);
            row++;
        }

        return region.Rows(row, region.Height - row);
    }

    /// <summary>
    /// Moves through the fields, opens one, or clears it. For a view that is nothing but a form; views
    /// that mix a form with other panes hand it to the focus ring instead.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>What was done with it, and where to go.</returns>
    public FocusResult Handle(ConsoleKeyInfo key) => Press(key);

    private FocusResult Press(ConsoleKeyInfo key)
    {
        if (Fields.Count == 0)
        {
            return FocusResult.Ignored;
        }

        if (_keymap.MoveUp.Matches(key))
        {
            Selected = Math.Max(0, Selected - 1);
            return FocusResult.Handled;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            Selected = Math.Min(Fields.Count - 1, Selected + 1);
            return FocusResult.Handled;
        }

        if (_keymap.Confirm.Matches(key))
        {
            return FocusResult.Navigate(Activate());
        }

        if (!_keymap.Erase.Matches(key))
        {
            return FocusResult.Ignored;
        }

        Current?.Reset?.Invoke();
        return FocusResult.Handled;
    }

    /// <summary>
    /// Scrolls with the wheel and selects with a click. Clicking the already selected field opens it,
    /// so a double click reads as select-then-edit.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>What was done with it, and where to go.</returns>
    public FocusResult HandleMouse(MouseEvent mouse)
    {
        if (!_lastRows.IsEmpty && !_lastRows.Contains(mouse.Row, mouse.Column))
        {
            return FocusResult.Ignored;
        }

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                Selected = Math.Max(0, Selected - 1);
                return FocusResult.Handled;
            case MouseAction.ScrolledDown:
                Selected = Math.Min(Fields.Count - 1, Selected + 1);
                return FocusResult.Handled;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left:
                return FocusResult.Navigate(ClickAt(mouse));
            default:
                return FocusResult.Ignored;
        }
    }

    /// <summary>
    /// What the form does with keys, worded and bound as the application configured it, ready to be
    /// shown in the hint line.
    /// </summary>
    /// <returns>The key and its description, one pair per action.</returns>
    public (string Key, string Description)[] Hints() =>
    [
        ($"{_keymap.MoveUp}{_keymap.MoveDown}", _strings.FormMove()),
        (_keymap.Confirm.ToString(), _strings.FormEdit()),
        (_keymap.Erase.ToString(), _strings.FormReset()),
    ];

    private ViewRoute ClickAt(MouseEvent mouse)
    {
        if (_lastRows.IsEmpty || !_lastRows.Contains(mouse.Row, mouse.Column))
        {
            return ViewRoute.None;
        }

        var (row, _) = _lastRows.ToLocal(mouse.Row, mouse.Column);

        if (!_fieldOfRow.TryGetValue(row, out var index))
        {
            return ViewRoute.None;
        }

        var wasSelected = index == Selected;
        Selected = index;

        return wasSelected ? Activate() : ViewRoute.None;
    }

    private ViewRoute Activate()
    {
        if (Current is not { } field || !field.IsEnabled())
        {
            return ViewRoute.None;
        }

        return field.Activate?.Invoke(_state) ?? ViewRoute.None;
    }

    private string Displayed(Field field)
    {
        var value = field.Value();
        return value.Length == 0 ? _strings.Empty() : value;
    }

    private TermColor StyleOf(Field field, int index)
    {
        if (index == Selected)
        {
            return !IsFocused ? Theme.Muted : field.IsEnabled() ? Theme.Selected : Theme.Warning;
        }

        if (field.IsAction)
        {
            return field.IsEnabled() ? Theme.Active : Theme.Muted;
        }

        return Theme.Default;
    }
}
