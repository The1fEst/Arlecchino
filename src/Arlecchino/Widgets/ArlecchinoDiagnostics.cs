namespace Arlecchino.Widgets;

/// <summary>
/// Diagnostic ids for the framework's own deprecations. Each one names a single removal so an
/// application can silence exactly that, instead of turning off every obsoletion warning it has.
/// </summary>
internal static class ArlecchinoDiagnostics
{
    /// <summary><see cref="IArlecchinoWidget.Draw"/>, replaced by <c>Place</c> and removed in 2.0.</summary>
    internal const string ObsoleteDraw = "ARL0001";
}
