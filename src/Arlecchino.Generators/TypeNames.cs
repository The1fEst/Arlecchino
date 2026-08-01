using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

internal static class TypeNames
{
    private static readonly SymbolDisplayFormat Format = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                              SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public static string Of(ITypeSymbol symbol) => symbol.ToDisplayString(Format);

    /// <summary>
    /// Whether the generated file, which lands in the same assembly, can name the type at all. A type
    /// nested privately inside another is deliberately invisible, and emitting a registration for it
    /// would fail the build in generated code rather than in the file the author wrote.
    /// </summary>
    public static bool IsReachable(INamedTypeSymbol symbol)
    {
        for (var type = symbol; type is not null; type = type.ContainingType)
        {
            if (type.DeclaredAccessibility is not (Accessibility.Public or
                Accessibility.Internal or
                Accessibility.ProtectedOrInternal))
            {
                return false;
            }
        }

        return true;
    }
}
