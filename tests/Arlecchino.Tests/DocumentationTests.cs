using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Arlecchino.Hosting;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Arlecchino.Tests;

public sealed class DocumentationTests
{
    [Fact]
    public void EveryTranslatableStringIsInTheLocalizationPage()
    {
        var page = Page("docs/localization.md");

        Assert.Empty(Undocumented(NamesOf(typeof(ArlecchinoStrings)), page));
    }

    [Fact]
    public void EveryFilePickerStringIsInTheLocalizationPage()
    {
        var page = Page("docs/localization.md");

        Assert.Empty(Undocumented(NamesOf(typeof(ArlecchinoStrings.FilePickerStrings)), page));
    }

    [Fact]
    public void EveryKeyBindingIsInTheInputPage()
    {
        var page = Page("docs/commands-and-input.md");
        var bindings = typeof(ArlecchinoKeymap)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static property => property.PropertyType == typeof(Input.KeyBinding))
            .Select(static property => property.Name);

        Assert.Empty(Undocumented(bindings, page));
    }

    [Fact]
    public void EveryDiagnosticIsInTheGeneratorPage()
    {
        var page = Page("docs/source-generator.md");
        var descriptors = typeof(Generators.ViewNavigationGenerator).Assembly
            .GetTypes()
            .Where(static type => type.Name.EndsWith("Diagnostics", StringComparison.Ordinal))
            .SelectMany(static type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(static field => field.FieldType == typeof(DiagnosticDescriptor))
            .Select(static field => ((DiagnosticDescriptor)field.GetValue(null)!).Id)
            .Distinct(StringComparer.Ordinal);

        Assert.Empty(Undocumented(descriptors, page));
    }

    [Fact]
    public void TheDocumentationIsWhereTheTestThinksItIs()
    {
        Assert.Contains("Arlecchino", Page("docs/README.md"), StringComparison.Ordinal);
    }

    private static IEnumerable<string> NamesOf(Type type) => type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(static property => property.PropertyType != typeof(ArlecchinoStrings.FilePickerStrings))
        .Select(static property => property.Name);

    private static string[] Undocumented(IEnumerable<string> names, string page) =>
        names.Where(name => !page.Contains($"`{name}", StringComparison.Ordinal)).ToArray();

    private static string Page(string path)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "docs")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return File.ReadAllText(Path.Combine(directory.FullName, path));
    }
}
