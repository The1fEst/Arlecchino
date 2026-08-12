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
    /// Whether the generated file, which lands in the same assembly, can name the type at all. A registration
    /// for a privately nested type would fail the build in generated code.
    /// </summary>
    /// <param name="symbol">The type to name.</param>
    /// <returns>Whether the generated file may name it.</returns>
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
