using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Arlecchino.Navigation;

namespace Arlecchino.Forms;

/// <summary>
/// One row of a form over a value of a known kind. It holds no value of its own, so a row shows what the
/// state holds now; build one through <see cref="Fields"/> and add the rest with the <c>With</c> members.
/// </summary>
/// <typeparam name="T">The kind of value the row is over.</typeparam>
public class Field<T> : IField
{
    /// <summary>Creates a row over an atom.</summary>
    /// <param name="value">The atom to read and write.</param>
    public Field(IReadableAtom value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public IReadableAtom Value { get; }

    /// <inheritdoc />
    public Func<string>? Label { get; internal set; }

    /// <inheritdoc />
    public Func<string>? Help { get; internal set; }

    /// <inheritdoc />
    public Func<string>? Template { get; internal set; }

    /// <inheritdoc />
    public Func<ViewRoute>? Action { get; internal set; }

    /// <summary>
    /// Whether a value is allowed, answered when the dialog is confirmed; <c>true</c> lets it through.
    /// Nothing accepts whatever was typed.
    /// </summary>
    public Func<T, bool>? Validation { get; internal set; }

    /// <summary>
    /// What the value may be, which is what turns a row into one that is picked from rather than typed
    /// into. For a number the lowest and the highest of them are the ends of its range.
    /// </summary>
    public Func<IReadOnlyList<T>>? Items { get; internal set; }

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <summary>Names the row.</summary>
    /// <param name="label">What the row is called.</param>
    /// <returns>The same row, to go on building it.</returns>
    public Field<T> WithLabel(Func<string> label)
    {
        Label = label;
        return this;
    }

    /// <summary>
    /// Gives the row some help, shown under it while it is selected. It wraps over as many rows as it
    /// takes, and a line break in it is kept.
    /// </summary>
    /// <param name="help">What to say.</param>
    /// <returns>The same row, to go on building it.</returns>
    public Field<T> WithHelp(Func<string> help)
    {
        Help = help;
        return this;
    }

    /// <summary>Words the value for the form, where the atom does not write itself out the way it reads.</summary>
    /// <param name="template">What to show instead.</param>
    /// <returns>The same row, to go on building it.</returns>
    public Field<T> WithTemplate(Func<string> template)
    {
        Template = template;
        return this;
    }

    /// <summary>
    /// Makes confirming the row do this rather than open the dialog its value would otherwise get.
    /// </summary>
    /// <param name="action">What to run. Returning a route navigates.</param>
    /// <returns>The same row, to go on building it.</returns>
    public Field<T> WithAction(Func<ViewRoute> action)
    {
        Action = action;
        return this;
    }

    /// <summary>Decides which values the dialog will accept.</summary>
    /// <param name="validation">Answers whether a value is allowed.</param>
    /// <returns>The same row, to go on building it.</returns>
    public Field<T> WithValidation(Func<T, bool> validation)
    {
        Validation = validation;
        return this;
    }

    /// <summary>Gives the row the values it is picked from, or the range it moves along.</summary>
    /// <param name="items">What the value may be, in the order they are offered.</param>
    /// <returns>The same row, to go on building it.</returns>
    public Field<T> WithItems(Func<T[]> items)
    {
        Items = items;
        return this;
    }

    /// <inheritdoc />
    public bool TryReset() => Value is IResettableAtom atom && atom.TryReset();
}
