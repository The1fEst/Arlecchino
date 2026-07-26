using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

public static class StoreDiagnostics
{
    public static readonly DiagnosticDescriptor NoPublicConstructor = new(
        "ARL005",
        "Store has no public constructor",
        "'{0}' implements IArlecchinoStore but has no public constructor, so the container cannot build it",
        ViewDiagnostics.Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
