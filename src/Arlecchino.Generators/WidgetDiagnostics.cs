using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

public static class WidgetDiagnostics
{
    public static readonly DiagnosticDescriptor CannotBeBuilt = new(
        "ARL007",
        "Widget cannot be registered",
        "'{0}' is a widget the container cannot build ({1}), so it is left out of AddGeneratedWidgets; " +
        "construct it in the view instead",
        ViewDiagnostics.Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
