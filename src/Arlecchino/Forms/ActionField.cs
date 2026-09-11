using System;
using Arlecchino.Atoms;
using Arlecchino.Navigation;

namespace Arlecchino.Forms;

/// <summary>
/// A button rather than a value, for the things a form does once it has been filled in. It holds no atom,
/// which is what the form draws it as a button by.
/// </summary>
public sealed class ActionField : IField
{
    /// <summary>Creates the button.</summary>
    /// <param name="label">What the button says.</param>
    /// <param name="action">What pressing it does. Returning a route navigates.</param>
    public ActionField(Func<string> label, Func<ViewRoute> action)
    {
        Label = label;
        Action = action;
    }

    /// <inheritdoc />
    public IReadableAtom? Value => null;

    /// <inheritdoc />
    public Func<string>? Label { get; }

    /// <inheritdoc />
    public Func<string>? Help { get; private set; }

    /// <inheritdoc />
    public Func<string>? Template => null;

    /// <inheritdoc />
    public Func<ViewRoute>? Action { get; }

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gives the button some help, shown under it while it is selected. It wraps over as many rows as it
    /// takes, and a line break in it is kept.
    /// </summary>
    /// <param name="help">What to say.</param>
    /// <returns>The same button, to go on building it.</returns>
    public ActionField WithHelp(Func<string> help)
    {
        Help = help;
        return this;
    }

    /// <inheritdoc />
    public bool TryReset() => false;
}
