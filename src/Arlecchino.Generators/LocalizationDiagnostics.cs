using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

public static class LocalizationDiagnostics
{
    public const string Category = "Arlecchino";

    public static readonly DiagnosticDescriptor InvalidToml = new(
        "ARL021",
        "The localization file could not be read",
        "{0}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoDefaultLocalization = new(
        "ARL022",
        "No localization file says it is the default",
        "One localization file must set language to '{0}'; every other one is a translation of it",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnknownTranslationKey = new(
        "ARL023",
        "A translation names a string the default does not have",
        "'{0}' has '{1}', which the default localization does not; it would never be reached",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingTranslationKey = new(
        "ARL024",
        "A translation is missing a string",
        "'{0}' has no '{1}'; the default is drawn there instead",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
