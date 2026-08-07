using System;
using System.Collections.Generic;
using System.Globalization;
using Arlecchino.Navigation;
using Arlecchino.Rendering.Colors;
using Arlecchino.State;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;

namespace Arlecchino.Forms;

/// <summary>
/// One row of a form: a label, the value beside it, and what happens when it is confirmed. Everything
/// is read through delegates rather than stored, so a field always shows what the state holds now and
/// follows the language the application is running in. The factories bind a field to an atom and pick
/// the dialog that suits its type, which is the usual way to build one.
///
/// The atom they take is a <see cref="Atom{T}"/>, which is either a <see cref="TrackedAtom{T}"/> —
/// so that editing the field can be undone — or a <see cref="LocalAtom{T}"/> when it should not be.
/// </summary>
public sealed class Field
{
    /// <summary>What the field is called.</summary>
    public required Func<string> Label { get; init; }

    /// <summary>The value as the user should see it, which is not always what is stored.</summary>
    public required Func<string> Value { get; init; }

    /// <summary>A line shown under the field while it is selected. Empty means nothing is shown.</summary>
    public Func<string> Help { get; init; } = static () => "";

    /// <summary>
    /// Whether the field can be used. A disabled field is drawn muted and skipped when moving through
    /// the form.
    /// </summary>
    public Func<bool> IsEnabled { get; init; } = static () => true;

    /// <summary>
    /// What confirming the field does, usually opening a dialog. Returning a route navigates; return
    /// <see cref="ViewRoute.None"/> to stay put. Without this the field is read-only.
    /// </summary>
    public Func<ArlecchinoState, ViewRoute>? Activate { get; init; }

    /// <summary>
    /// Puts the field back to its empty or lowest value. Fields whose type has no sensible empty value,
    /// such as a date, leave this out and simply cannot be cleared.
    /// </summary>
    public Action? Reset { get; init; }

    /// <summary>
    /// Whether the row is a button rather than a value. Buttons have no value to align, so they are not
    /// counted when working out the label column.
    /// </summary>
    public bool IsAction { get; init; }

    private static Func<string> NoHelp => static () => "";

    /// <summary>A line of text, edited in a dialog.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="validate">Checked when the dialog is confirmed; return a message to keep it open.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Text(
        Func<string> label,
        Atom<string> value,
        Func<string, string?>? validate = null,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => value.Value,
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestText(label(), value.Value, validate, typed => value.Value = typed);
            return ViewRoute.None;
        },
        Reset = () => value.Value = "",
    };

    /// <summary>
    /// A secret, shown as dots both in the form and while it is typed. The atom still holds the text as
    /// it is, so treat it as sensitive.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Secret(Func<string> label, Atom<string> value, Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => new('•', value.Value.Length),
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestPassword(label(), typed => value.Value = typed);
            return ViewRoute.None;
        },
        Reset = () => value.Value = "",
    };

    /// <summary>A number that can be typed or stepped. Clearing it puts the value back to the lowest allowed.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="minimum">Lowest value allowed.</param>
    /// <param name="maximum">Highest value allowed.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Number(
        Func<string> label,
        Atom<decimal> value,
        decimal minimum,
        decimal maximum,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => value.Value.ToString(CultureInfo.InvariantCulture),
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestNumber(label(), value.Value, minimum, maximum, picked => value.Value = picked);
            return ViewRoute.None;
        },
        Reset = () => value.Value = minimum,
    };

    /// <summary>
    /// A number picked on a slider rather than typed, for values where the range matters more than the
    /// exact figure.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="minimum">Value at the left end.</param>
    /// <param name="maximum">Value at the right end.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Slider(
        Func<string> label,
        Atom<decimal> value,
        decimal minimum,
        decimal maximum,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => value.Value.ToString(CultureInfo.InvariantCulture),
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestSlider(label(), value.Value, minimum, maximum, picked => value.Value = picked);
            return ViewRoute.None;
        },
        Reset = () => value.Value = minimum,
    };

    /// <summary>A yes-or-no answer.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="render">Words for the two answers, since "on" and "off" do not suit every question.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Toggle(
        Func<string> label,
        Atom<bool> value,
        Func<bool, string> render,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => render(value.Value),
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestToggle(label(), value.Value, picked => value.Value = picked);
            return ViewRoute.None;
        },
        Reset = () => value.Value = false,
    };

    /// <summary>One option out of a list, chosen in a filterable dialog.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="options">Everything that can be chosen.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Choice(
        Func<string> label,
        IReadOnlyList<string> options,
        Atom<string> value,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => value.Value,
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestChoice(label(), options, picked => value.Value = picked, value.Value);
            return ViewRoute.None;
        },
        Reset = () => value.Value = "",
    };

    /// <summary>Any number of options out of a list.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="options">Everything that can be chosen.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="render">Sums up what is chosen for the one row the form has to show it in.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field MultiChoice(
        Func<string> label,
        IReadOnlyList<string> options,
        Atom<IReadOnlyList<string>> value,
        Func<IReadOnlyList<string>, string> render,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => render(value.Value),
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestMultiChoice(label(), options, value.Value, picked => value.Value = picked);
            return ViewRoute.None;
        },
        Reset = () => value.Value = [],
    };

    /// <summary>A calendar date. There is no empty date, so the field cannot be cleared.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="render">Formats the date for the form, where a local format usually beats the ISO one.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Date(
        Func<string> label,
        Atom<DateOnly> value,
        Func<DateOnly, string> render,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => render(value.Value),
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestDate(label(), value.Value, picked => value.Value = picked);
            return ViewRoute.None;
        },
    };

    /// <summary>A time of day. There is no empty time, so the field cannot be cleared.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="render">Formats the time for the form.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Time(
        Func<string> label,
        Atom<TimeOnly> value,
        Func<TimeOnly, string> render,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => render(value.Value),
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestTime(label(), value.Value, picked => value.Value = picked);
            return ViewRoute.None;
        },
    };

    /// <summary>
    /// A color, shown as its hex code and picked on three sliders. Reopening the dialog can shift the
    /// color by one unit, since it is edited as hue, saturation and lightness.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Color(Func<string> label, Atom<Rgb> value, Func<string>? help = null) => new()
    {
        Label = label,
        Value = () => value.Value.Hex,
        Help = help ?? NoHelp,
        Activate = state =>
        {
            state.RequestColor(label(), value.Value, picked => value.Value = picked);
            return ViewRoute.None;
        },
    };

    /// <summary>
    /// A path on disk. Unlike the other fields this leaves the form: the picker is a view of its own,
    /// which is why it has to be told where to come back to.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="returnView">The view to return to once the picker is done.</param>
    /// <param name="pickFolder">Whether a folder is being chosen rather than a file.</param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Path(
        Func<string> label,
        Atom<string> value,
        ViewRoute returnView,
        bool pickFolder,
        Func<string>? help = null) =>
        Picker(label, value, returnView, pickFolder, help, start: null);

    /// <summary>
    /// A path on disk that opens the picker somewhere in particular while the field is still empty —
    /// a project folder, the last folder the user was in, wherever the answer is likely to be.
    ///
    /// It is a separate member rather than another argument to <see cref="Path"/> because adding one
    /// to a method that already ships would break every application compiled against it.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="returnView">The view to return to once the picker is done.</param>
    /// <param name="pickFolder">Whether a folder is being chosen rather than a file.</param>
    /// <param name="start">
    /// Where the picker opens while the field is empty. A field that already holds a path opens there
    /// instead, and a path that no longer exists lands the picker on the drives.
    ///
    /// It is a delegate rather than a string so that "where we were last time" is answered when the
    /// picker opens rather than when the form is built:
    /// <c>start: () => state.PickerLastFolder</c>.
    /// </param>
    /// <param name="help">A line shown under the field while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field PathFrom(
        Func<string> label,
        Atom<string> value,
        ViewRoute returnView,
        bool pickFolder,
        Func<string> start,
        Func<string>? help = null) =>
        Picker(label, value, returnView, pickFolder, help, start);

    private static Field Picker(
        Func<string> label,
        Atom<string> value,
        ViewRoute returnView,
        bool pickFolder,
        Func<string>? help,
        Func<string>? start) => new()
    {
        Label = label,
        Value = () => value.Value,
        Help = help ?? NoHelp,
        Activate = state =>
        {
            var opening = value.Value.Length > 0 ? value.Value : start?.Invoke() ?? "";

            state.FilePicker = new(label(), pickFolder, opening, returnView, picked => value.Value = picked);
            return Routes.FilePicker;
        },
        Reset = () => value.Value = "",
    };

    /// <summary>A button rather than a value, for the things a form does once it has been filled in.</summary>
    /// <param name="label">What the button says.</param>
    /// <param name="run">What pressing it does. Returning a route navigates.</param>
    /// <param name="enabled">Whether it can be pressed; use it to require the form to be complete.</param>
    /// <param name="help">A line shown under the button while it is selected.</param>
    /// <returns>The field.</returns>
    public static Field Action(
        Func<string> label,
        Func<ViewRoute> run,
        Func<bool>? enabled = null,
        Func<string>? help = null) => new()
    {
        Label = label,
        Value = static () => "",
        Help = help ?? NoHelp,
        IsEnabled = enabled ?? (static () => true),
        IsAction = true,
        Activate = _ => run(),
    };
}
