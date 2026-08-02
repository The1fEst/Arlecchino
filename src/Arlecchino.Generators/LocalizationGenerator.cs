using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Arlecchino.Generators;

/// <summary>
/// Turns localization files into one enum of names and one function that resolves them.
///
/// The point is not translation — an application with one language wants this too. Text written in
/// the place it is drawn gets written twice: the same sentence appears in a dialog and in the log
/// that follows it, and the day one of them is reworded the two quietly disagree. A name that is
/// checked at compile time cannot drift, and the file it resolves to is the one place to read the
/// application's whole voice at once.
///
/// The files come from the project being built, as <c>AdditionalFiles</c> named <c>*.toml</c> under a
/// folder the project names in <c>ArlecchinoLocalizationFolder</c>. The default file is the one whose
/// language matches <c>ArlecchinoLocalizationLanguage</c>, which is <c>en</c> unless it is set; every
/// other file is a translation, and a string it leaves out falls back to the default rather than
/// leaving a hole on the screen.
/// </summary>
[Generator]
public sealed partial class LocalizationGenerator : IIncrementalGenerator
{
    private const string DefaultFolder = "Localization";
    private const string DefaultLanguage = "en";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var settings = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        {
            provider.GlobalOptions.TryGetValue("build_property.ArlecchinoLocalizationFolder", out var folder);
            provider.GlobalOptions.TryGetValue("build_property.ArlecchinoLocalizationLanguage", out var language);
            provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

            return (
                Folder: string.IsNullOrWhiteSpace(folder) ? DefaultFolder : folder!,
                Language: string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language!,
                Namespace: string.IsNullOrWhiteSpace(rootNamespace) ? "Localization" : rootNamespace!);
        });

        var files = context.AdditionalTextsProvider
            .Where(static text => text.Path.EndsWith(".toml", StringComparison.OrdinalIgnoreCase))
            .Select(static (text, token) => new LocalizationSource(text.Path, text.GetText(token)?.ToString() ?? ""))
            .Collect();

        context.RegisterSourceOutput(files.Combine(settings), Generate);
    }

    private static void Generate(
        SourceProductionContext context,
        (ImmutableArray<LocalizationSource> Files, (string Folder, string Language, string Namespace) Settings) input)
    {
        var (files, settings) = input;
        var wanted = files
            .Where(source => InFolder(source.Path, settings.Folder))
            .OrderBy(static source => source.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (wanted.Length == 0)
        {
            return;
        }

        var localizations = new List<LocalizationFile>();

        foreach (var source in wanted)
        {
            var parsed = Parse(source);

            foreach (var error in parsed.Errors)
            {
                context.ReportDiagnostic(Diagnostic.Create(LocalizationDiagnostics.InvalidToml, Location.None, error));
            }

            if (parsed.Errors.Count == 0)
            {
                localizations.Add(parsed);
            }
        }

        var standard = localizations.FirstOrDefault(localization =>
            localization.Language.Equals(settings.Language, StringComparison.OrdinalIgnoreCase));

        if (standard == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                LocalizationDiagnostics.NoDefaultLocalization,
                Location.None,
                settings.Language));

            return;
        }

        Compare(context, standard, localizations);

        context.AddSource(
            "Localization.g.cs",
            SourceText.From(BuildSource(settings.Namespace, standard, localizations), Encoding.UTF8));
    }

    /// <summary>
    /// Whether a file is one of the application's localizations rather than any other TOML it happens
    /// to hand the compiler.
    /// </summary>
    /// <param name="path">Where the file is.</param>
    /// <param name="folder">The folder the project keeps them in.</param>
    /// <returns><c>true</c> when it is one of them.</returns>
    private static bool InFolder(string path, string folder)
    {
        var directory = Path.GetDirectoryName(path) ?? "";

        return directory.Replace('\\', '/')
            .Split('/')
            .Any(part => part.Equals(folder, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Holds the translations to the default: a name it does not have is a name nothing will ever ask
    /// for, and a name it is missing is a sentence that will come out in the wrong language.
    /// </summary>
    /// <param name="context">Where the findings go.</param>
    /// <param name="standard">The default localization.</param>
    /// <param name="localizations">Every localization, the default included.</param>
    private static void Compare(
        SourceProductionContext context,
        LocalizationFile standard,
        IEnumerable<LocalizationFile> localizations)
    {
        var known = new HashSet<string>(standard.Entries.Select(static entry => entry.Key), StringComparer.Ordinal);

        foreach (var translation in localizations.Where(localization => !ReferenceEquals(localization, standard)))
        {
            var carried = new HashSet<string>(
                translation.Entries.Select(static entry => entry.Key),
                StringComparer.Ordinal);

            foreach (var entry in translation.Entries.Where(entry => !known.Contains(entry.Key)))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    LocalizationDiagnostics.UnknownTranslationKey,
                    Location.None,
                    translation.Language,
                    entry.Key));
            }

            foreach (var key in known.Where(key => !carried.Contains(key)))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    LocalizationDiagnostics.MissingTranslationKey,
                    Location.None,
                    translation.Language,
                    key));
            }
        }
    }
}
