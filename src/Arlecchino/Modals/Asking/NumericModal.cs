using System.Globalization;

namespace Arlecchino.Modals.Asking;

/// <summary>What the number field and the slider have in common: stepping, precision and affixes.</summary>
public abstract class NumericModal : Modal, IAffixedModal
{
    /// <summary>How far the arrow keys move the value.</summary>
    public decimal Step { get; init; } = 1m;

    /// <summary>How far the page keys move the value.</summary>
    public decimal LargeStep { get; init; } = 10m;

    /// <summary>
    /// Digits kept after the separator. Zero also means a decimal separator cannot be typed at all.
    /// </summary>
    public int Decimals { get; init; }

    /// <summary>Drawn before the value.</summary>
    public string Prefix { get; init; } = "";

    /// <summary>Drawn after the value.</summary>
    public string Suffix { get; init; } = "";

    /// <summary>Formats a value with the configured precision, culture-independently.</summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The number as text, without affixes.</returns>
    public string FormatNumber(decimal value) =>
        value.ToString("F" + Decimals, CultureInfo.InvariantCulture);

    /// <summary>Formats a value the way the user sees it, affixes included.</summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The number as text, with affixes.</returns>
    public string Display(decimal value) => Prefix + FormatNumber(value) + Suffix;
}
