using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

public static class ViewDiagnostics
{
    public const string Category = "Arlecchino";

    public static readonly DiagnosticDescriptor DuplicateRoute = new(
        "TSR001",
        "Two views produce the same route",
        "'{0}' and '{1}' both produce the route '{2}'; only the first one is reachable",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoPublicConstructor = new(
        "TSR002",
        "View has no public constructor",
        "'{0}' implements IView but has no public constructor, so the generated factory cannot create it",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ViewNamespaceNotSet = new(
        "TSR003",
        "ArlecchinoViewNamespace is not set",
        "ViewKind is emitted into '{0}'; set <ArlecchinoViewNamespace> in the project file to choose it",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoViews = new(
        "TSR004",
        "No views were found",
        "No class implements IView, so ViewKind holds no routes and the application has nowhere to start",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
