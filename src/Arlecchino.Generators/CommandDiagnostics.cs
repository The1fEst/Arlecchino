using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

public static class CommandDiagnostics
{
    public static readonly DiagnosticDescriptor NoPublicConstructor = new(
        "TSR006",
        "Command has no public constructor",
        "'{0}' implements IArlecchinoCommand but has no public constructor, so the container cannot build it",
        ViewDiagnostics.Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
