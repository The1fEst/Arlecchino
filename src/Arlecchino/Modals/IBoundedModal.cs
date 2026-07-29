using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Modals;

/// <summary>
/// A value that moves in steps between two ends. Shared by the number field and the slider, so the
/// stepping keys work the same in both.
/// </summary>
public interface IBoundedModal
{
    /// <summary>Lowest value allowed.</summary>
    decimal Minimum { get; }

    /// <summary>Highest value allowed.</summary>
    decimal Maximum { get; }

    /// <summary>How far the arrow keys move the value.</summary>
    [SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "Step has been the name of this member since 1.0; renaming it to spare Visual Basic " +
                        "would break every application that implements the interface.")]
    decimal Step { get; }

    /// <summary>How far the page keys move the value.</summary>
    decimal LargeStep { get; }

    /// <summary>Moves the value and clamps it to the bounds.</summary>
    /// <param name="delta">How far to move; negative goes down.</param>
    void Add(decimal delta);
}
