using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

public static class StoreDiagnostics
{
    public static readonly DiagnosticDescriptor NoPublicConstructor = new(
        "TSR005",
        "Store has no public constructor",
        "'{0}' implements IStore but has no public constructor, so the container cannot build it",
        ViewDiagnostics.Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
