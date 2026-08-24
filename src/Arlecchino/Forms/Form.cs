using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Collections;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.State;
using Arlecchino.Widgets;

namespace Arlecchino.Forms;

/// <summary>
/// A column of fields with their values lined up, and a help line under the selected one. It holds no values
/// of its own, and the atom behind each field decides whether an edit can be undone.
/// </summary>
public sealed class Form : IArlecchinoInteractiveWidget
{
    private const string ValueSeparator = " = ";
    private const string ActionMarker = "> ";

    /// <summary>The ends of a track a field named none of its own, which are the ones it draws by default.</summary>
    private const decimal SliderLowest = 0m;

    private const decimal SliderHighest = 100m;

    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly ArlecchinoStrings _strings;

    private readonly Dictionary<int, int> _fieldOfRow = [];

    private SurfaceRegion _lastRows;

    /// <summary>Creates the form.</summary>
    /// <param name="state">Where the fields open their dialogs.</param>
    /// <param name="options">Supplies the keymap and the wording.</param>
    public Form(ArlecchinoState state, ArlecchinoOptions options)
    {
        _state = state;
        _keymap = options.Keymap;
        _strings = options.Strings;
    }

    /// <summary>The rows, top to bottom.</summary>
    public required IReadOnlyList<IField> Fields { get; init; }

    /// <summary>Index of the selected field.</summary>
    public int SelectedIndex { get; private set; }

    /// <summary>Whether the form has focus, which decides how strongly the selection is drawn.</summary>
    public bool IsFocused { get; set; } = true;

    /// <summary>The selected field, or <c>null</c> when the form has none.</summary>
    public IField? Current => Fields.Count == 0 ? null : Fields[Math.Clamp(SelectedIndex, 0, Fields.Count - 1)];

    /// <summary>
    /// Draws the fields with their labels aligned, scrolled, so the selection stays in view, and returns
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

        SelectedIndex = Math.Clamp(SelectedIndex, 0, Fields.Count - 1);

        var labelWidth = 0;
        foreach (var field in Fields)
        {
            if (field.Value != null)
            {
                labelWidth = Math.Max(labelWidth, TextWidth.Of(Named(field)));
            }
        }

        var helpRows = (Current?.Help?.Invoke().Length ?? 0) > 0 ? 1 : 0;
        var fieldRows = Math.Max(1, region.Height - helpRows);
        var first = Math.Clamp(SelectedIndex - fieldRows / 2, 0, Math.Max(0, Fields.Count - fieldRows));

        _lastRows = region;
        _fieldOfRow.Clear();

        var row = 0;
        for (var i = 0; i < fieldRows && first + i < Fields.Count && row < region.Height; i++)
        {
            var index = first + i;
            var field = Fields[index];
            var line = field.Value == null
                ? ActionMarker + Named(field)
                : $"{TextWidth.PadRight(Named(field), labelWidth)}{ValueSeparator}{Displayed(field)}";

            region.Write(row, 0, line, StyleOf(field, index));
            _fieldOfRow[row] = index;
            row++;

            if (index != SelectedIndex || helpRows == 0 || row >= region.Height || field.Help == null)
            {
                continue;
            }

            region.Write(row, 2, field.Help(), Theme.Secondary);
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
    public FocusResult Handle(KeyPress key) => Press(key);

    private FocusResult Press(KeyPress key)
    {
        if (Fields.Count == 0)
        {
            return FocusResult.Ignored;
        }

        if (_keymap.MoveUp.Matches(key))
        {
            SelectedIndex = Math.Max(0, SelectedIndex - 1);
            return FocusResult.Handled;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            SelectedIndex = Math.Min(Fields.Count - 1, SelectedIndex + 1);
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

        Current?.TryReset();
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
                SelectedIndex = Math.Max(0, SelectedIndex - 1);
                return FocusResult.Handled;
            case MouseAction.ScrolledDown:
                SelectedIndex = Math.Min(Fields.Count - 1, SelectedIndex + 1);
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

        var wasSelected = index == SelectedIndex;
        SelectedIndex = index;

        return wasSelected ? Activate() : ViewRoute.None;
    }

    private ViewRoute Activate()
    {
        if (Current is not { IsEnabled: true } field)
        {
            return ViewRoute.None;
        }

        return field.Action is { } action ? action() : Open(field);
    }

    /// <summary>
    /// Opens what the field is edited in, chosen by its shape: the kind of value it is over, whether that
    /// value is one of a list, and which of the rows sharing a kind it says it is.
    /// </summary>
    /// <param name="field">The field being confirmed.</param>
    /// <returns>Where to go, which is nowhere for everything but the picker.</returns>
    private ViewRoute Open(IField field)
    {
        var title = Named(field);

        switch (field)
        {
            case PathField { Value: Atom<string> value } path:
                var opening = value.Value.Length > 0 ? value.Value : path.Start?.Invoke() ?? "";

                _state.FilePicker = new(title, path.PickFolder, opening, path.ReturnView, picked => value.Value = picked);
                return Routes.FilePicker;

            case SecretField { Value: Atom<string> value }:
                _state.RequestPassword(title, typed => value.Value = typed);
                return ViewRoute.None;

            case SliderField { Value: Atom<decimal> value } slider:
                var (lowest, highest) = Ends(slider.Items, SliderLowest, SliderHighest);

                _state.RequestSlider(title, value.Value, lowest, highest, picked => value.Value = picked);
                return ViewRoute.None;

            case Field<string> { Value: AtomsList<string> chosen } marked:
                _state.RequestMultiChoice(title, Offered(marked.Items), chosen.Value, picked => chosen.Reset(picked));
                return ViewRoute.None;

            case Field<string> { Items: not null, Value: Atom<string> value } choice:
                _state.RequestChoice(title, choice.Items(), picked => value.Value = picked, value.Value);
                return ViewRoute.None;

            case Field<string> { Value: Atom<string> value } text:
                _state.RequestText(title, value.Value, Allows(text.Validation), typed => value.Value = typed);
                return ViewRoute.None;

            case Field<decimal> { Value: Atom<decimal> value } number:
                var (least, most) = Ends(number.Items, decimal.MinValue, decimal.MaxValue);

                _state.RequestNumber(title, value.Value, least, most, picked => value.Value = picked);
                return ViewRoute.None;

            case Field<bool> { Value: Atom<bool> value }:
                _state.RequestToggle(title, value.Value, picked => value.Value = picked);
                return ViewRoute.None;

            case Field<DateOnly> { Value: Atom<DateOnly> value }:
                _state.RequestDate(title, value.Value, picked => value.Value = picked);
                return ViewRoute.None;

            case Field<TimeOnly> { Value: Atom<TimeOnly> value }:
                _state.RequestTime(title, value.Value, picked => value.Value = picked);
                return ViewRoute.None;

            case Field<RGB> { Value: Atom<RGB> value }:
                _state.RequestColor(title, value.Value, picked => value.Value = picked);
                return ViewRoute.None;

            default:
                return ViewRoute.None;
        }
    }

    /// <summary>
    /// Turns "this value is allowed" into the message the text dialog stays open on, since a form says
    /// which values it takes and the dialog says why one was refused.
    /// </summary>
    /// <param name="validation">What the field allows, or nothing to allow anything.</param>
    /// <returns>The check the dialog runs, or nothing to run none.</returns>
    private Func<string, string?>? Allows(Func<string, bool>? validation) =>
        validation == null ? null : typed => validation(typed) ? null : _strings.NotAnAllowedValue();

    private static IReadOnlyList<string> Offered(Func<IReadOnlyList<string>>? items) => items?.Invoke() ?? [];

    /// <summary>The ends of the range a number is held between, which are the ends of what it may be.</summary>
    /// <param name="items">The values the field may hold, or nothing when it is not held to any.</param>
    /// <param name="lowest">What to fall back to at the low end.</param>
    /// <param name="highest">What to fall back to at the high end.</param>
    /// <returns>The two ends.</returns>
    private static (decimal Lowest, decimal Highest) Ends(
        Func<IReadOnlyList<decimal>>? items,
        decimal lowest,
        decimal highest)
    {
        if (items?.Invoke() is not { Count: > 0 } values)
        {
            return (lowest, highest);
        }

        var low = values[0];
        var high = values[0];

        foreach (var value in values)
        {
            low = Math.Min(low, value);
            high = Math.Max(high, value);
        }

        return (low, high);
    }

    private static string Named(IField field) => field.Label?.Invoke() ?? "";

    private string Displayed(IField field)
    {
        if (field.Template is { } template)
        {
            return template();
        }

        if (field.Value is Atom<bool> flag)
        {
            return flag.Value ? _strings.Yes() : _strings.No();
        }

        var value = field.Value?.ToString();
        return string.IsNullOrEmpty(value) ? _strings.Empty() : value;
    }

    private TermColor StyleOf(IField field, int index)
    {
        if (index == SelectedIndex)
        {
            return !IsFocused ? Theme.Secondary : field.IsEnabled ? Theme.Selection : Theme.Warning;
        }

        if (field.Value == null)
        {
            return field.IsEnabled ? Theme.Active : Theme.Secondary;
        }

        return Theme.Default;
    }
}
