using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Collections;
using Arlecchino.Navigation;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Forms;

/// <summary>
/// The rows a form is built out of. Each one gives the row the shape <see cref="Form"/> reads it by, so an
/// application says what a row is over and never says which dialog to open.
/// </summary>
public static class Fields
{
    /// <summary>A line of text, edited in a dialog.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<string> Text(Func<string> label, Atom<string> value) =>
        new(value)
        {
            Label = label,
        };

    /// <summary>
    /// A secret, shown as dots both in the form and while it is typed. The atom still holds the text as
    /// it is, so treat it as sensitive.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static SecretField Secret(Func<string> label, Atom<string> value) =>
        new(value)
        {
            Label = label,
            Template = () => new('•', value.Value.Length),
        };

    /// <summary>
    /// A number that can be typed or stepped. Without <see cref="Field{T}.Items"/> it takes any number at
    /// all; with them, it is held between the lowest and the highest.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<decimal> Number(Func<string> label, Atom<decimal> value) =>
        new(value)
        {
            Label = label,
        };

    /// <summary>
    /// A number picked on a slider rather than typed, for values where the range matters more than the
    /// exact figure. The ends of the track are the lowest and the highest of
    /// <see cref="Field{T}.Items"/>, or 0 and 100 without them.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static SliderField Slider(Func<string> label, Atom<decimal> value) =>
        new(value)
        {
            Label = label,
        };

    /// <summary>A yes-or-no answer, worded by the application's strings.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<bool> Toggle(Func<string> label, Atom<bool> value) =>
        new(value)
        {
            Label = label,
        };

    /// <summary>One option out of a list, chosen in a filterable dialog.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="options">Everything that can be chosen.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<string> Choice(Func<string> label, IReadOnlyList<string> options, Atom<string> value) =>
        new(value)
        {
            Label = label,
            Items = () => options,
        };

    /// <summary>Any number of options out of a list, held in a list atom.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="options">Everything that can be chosen.</param>
    /// <param name="value">The list to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<string> MultiChoice(
        Func<string> label,
        IReadOnlyList<string> options,
        AtomsList<string> value) =>
        new(value)
        {
            Label = label,
            Items = () => options,
        };

    /// <summary>A calendar date.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<DateOnly> Date(Func<string> label, Atom<DateOnly> value) =>
        new(value)
        {
            Label = label,
        };

    /// <summary>A time of day.</summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<TimeOnly> Time(Func<string> label, Atom<TimeOnly> value) =>
        new(value)
        {
            Label = label,
        };

    /// <summary>
    /// A color, shown as its hex code and picked on three sliders. Reopening the dialog can shift the
    /// color by one unit, since it is edited as hue, saturation and lightness.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <returns>The field.</returns>
    public static Field<RGB> Color(Func<string> label, Atom<RGB> value) =>
        new(value)
        {
            Label = label,
        };

    /// <summary>
    /// A path on disk. It leaves the form, since the picker is a view of its own, and so it has to be told
    /// where to come back to.
    /// </summary>
    /// <param name="label">What the field is called.</param>
    /// <param name="value">The atom to read and write.</param>
    /// <param name="returnView">The view to return to once the picker is done.</param>
    /// <param name="pickFolder">Whether a folder is being chosen rather than a file.</param>
    /// <returns>The field.</returns>
    public static PathField Path(Func<string> label, Atom<string> value, ViewRoute returnView, bool pickFolder) =>
        new(value, returnView, pickFolder)
        {
            Label = label,
        };

    /// <summary>A button rather than a value, for the things a form does once it has been filled in.</summary>
    /// <param name="label">What the button says.</param>
    /// <param name="run">What pressing it does. Returning a route navigates.</param>
    /// <returns>The field.</returns>
    public static ActionField Action(Func<string> label, Func<ViewRoute> run) => new(label, run);
}
