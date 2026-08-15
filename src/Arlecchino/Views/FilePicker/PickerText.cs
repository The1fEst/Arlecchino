using System;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// Fitting text to a column of the picker. A name too long for its column ends in an ellipsis rather than
/// running into the column beside it.
/// </summary>
internal static class PickerText
{
    /// <summary>Fits the text to a width and fills what is left of it.</summary>
    /// <param name="text">The text.</param>
    /// <param name="width">Columns it is drawn in.</param>
    /// <returns>The text, cut or padded to the width.</returns>
    public static string Pad(string text, int width) => TextWidth.PadRight(Clip(text, width), width);

    /// <summary>The same, filled on the left, so the text ends at the far edge.</summary>
    /// <param name="text">The text.</param>
    /// <param name="width">Columns it is drawn in.</param>
    /// <returns>The text, cut or padded to the width.</returns>
    public static string PadLeft(string text, int width) => TextWidth.PadLeft(Clip(text, width), width);

    /// <summary>Cuts the text to a width, ending it in an ellipsis when anything was left out.</summary>
    /// <param name="text">The text.</param>
    /// <param name="width">Columns it is drawn in.</param>
    /// <returns>What fits.</returns>
    public static string Clip(string text, int width) =>
        width <= 0 ? "" :
        TextWidth.Of(text) > width ? TextWidth.Truncate(text, Math.Max(0, width - 1)) + "…" : text;
}
