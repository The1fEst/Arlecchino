using System;
using Arlecchino.Atoms;
using Arlecchino.Navigation;

namespace Arlecchino.Forms;

/// <summary>
/// One row of a form, as the form sees it. Rows over values of different kinds stand in one list through
/// this, and the form asks the atom rather than the row for what is shown.
/// </summary>
public interface IField
{
    /// <summary>
    /// The atom the row shows and edits, or <c>null</c> for a button, which has no value to show and is
    /// drawn as a button because of it.
    /// </summary>
    IReadableAtom? Value { get; }

    /// <summary>What the row is called.</summary>
    Func<string>? Label { get; }

    /// <summary>A line shown under the row while it is selected. Nothing means none is shown.</summary>
    Func<string>? Help { get; }

    /// <summary>
    /// How the value is worded, for the values an atom does not write out the way a form should show
    /// them — a secret as dots, a date in the order the reader expects. Nothing leaves it to the atom.
    /// </summary>
    Func<string>? Template { get; }

    /// <summary>
    /// What confirming the row does instead of opening the dialog its value would otherwise get.
    /// Returning a route navigates and <see cref="ViewRoute.None"/> stays put.
    /// </summary>
    Func<ViewRoute>? Action { get; }

    /// <summary>
    /// Whether the row can be used. A disabled row is drawn muted and confirming it does nothing.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>Puts the value back to what the atom held before anything was written to it.</summary>
    /// <returns><c>false</c> when the atom has no starting value to go back to.</returns>
    bool TryReset();
}
