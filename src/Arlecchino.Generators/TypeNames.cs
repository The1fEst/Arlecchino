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
}
