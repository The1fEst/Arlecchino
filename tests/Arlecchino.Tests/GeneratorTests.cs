using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Arlecchino.Generators;
using Xunit;

namespace Arlecchino.Tests;

public sealed class GeneratorTests
{
    private const string TwoViews = """
        using System;
        using Arlecchino.Navigation;

        namespace Sample;

        public class ModsView : IView
        {
            public void Draw() { }
            public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
        }

        public class AboutView : IView
        {
            public void Draw() { }
            public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
        }
        """;

    private static (string Source, ImmutableArray<Diagnostic> Diagnostics) Run(
        string source,
        string? viewNamespace = "Sample.Views",
        string? generate = null)
    {
        var compilation = CSharpCompilation.Create(
            "SampleApplication",
            [CSharpSyntaxTree.ParseText(source)],
            LoadedReferences(),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var options = new Dictionary<string, string> { ["build_property.RootNamespace"] = "Sample" };

        if (viewNamespace is not null)
        {
            options["build_property.ArlecchinoViewNamespace"] = viewNamespace;
        }

        if (generate is not null)
        {
            options["build_property.ArlecchinoGenerateViews"] = generate;
        }

        var driver = CSharpGeneratorDriver
            .Create([new ViewNavigationGenerator().AsSourceGenerator()], optionsProvider: new FixedOptions(options))
            .RunGenerators(compilation);

        var result = driver.GetRunResult();
        var generated = result.GeneratedTrees.Length == 0 ? "" : result.GeneratedTrees[0].ToString();

        return (generated, result.Diagnostics);
    }

    private static IEnumerable<MetadataReference> LoadedReferences()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && assembly.Location.Length > 0)
            {
                yield return MetadataReference.CreateFromFile(assembly.Location);
            }
        }
    }

    [Fact]
    public void RoutesAreNamedAfterViewsWithoutTheSuffix()
    {
        var (source, diagnostics) = Run(TwoViews);

        Assert.Contains("namespace Sample.Views;", source, StringComparison.Ordinal);
        Assert.Contains("public static readonly ViewRoute Mods = new ViewRoute(\"Mods\");", source, StringComparison.Ordinal);
        Assert.Contains("public static readonly ViewRoute About = new ViewRoute(\"About\");", source, StringComparison.Ordinal);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void NothingIsGeneratedWhenTheProjectTurnsItOff()
    {
        var (source, _) = Run(TwoViews, generate: "false");

        Assert.Equal("", source);
    }

    [Fact]
    public void DuplicateRoutesAreReported()
    {
        const string source = """
            using System;
            using Arlecchino.Navigation;

            namespace Sample
            {
                public class ModsView : IView
                {
                    public void Draw() { }
                    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
                }
            }

            namespace Sample.Extra
            {
                using Arlecchino.Navigation;

                public class ModsView : IView
                {
                    public void Draw() { }
                    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
                }
            }
            """;

        var (generated, diagnostics) = Run(source);
        var duplicate = Assert.Single(diagnostics, item => item.Id == "TSR001");

        Assert.Equal(DiagnosticSeverity.Warning, duplicate.Severity);
        Assert.Contains("Mods", duplicate.GetMessage(), StringComparison.Ordinal);
        Assert.Single(FindAll(generated, "ViewRoute Mods ="));
    }

    [Fact]
    public void ViewWithoutAPublicConstructorIsReported()
    {
        const string source = """
            using System;
            using Arlecchino.Navigation;

            namespace Sample;

            public class HiddenView : IView
            {
                private HiddenView() { }
                public void Draw() { }
                public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
            }
            """;

        var (_, diagnostics) = Run(source);
        var reported = Assert.Single(diagnostics, item => item.Id == "TSR002");

        Assert.Contains("HiddenView", reported.GetMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public void MissingViewNamespaceIsReportedAsInformation()
    {
        var (source, diagnostics) = Run(TwoViews, viewNamespace: null);
        var reported = Assert.Single(diagnostics, item => item.Id == "TSR003");

        Assert.Equal(DiagnosticSeverity.Info, reported.Severity);
        Assert.Contains("Sample.Navigation", reported.GetMessage(), StringComparison.Ordinal);
        Assert.Contains("namespace Sample.Navigation;", source, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedFactoryResolvesConstructorParametersFromTheContainer()
    {
        const string source = """
            using System;
            using Arlecchino.Navigation;
            using Arlecchino.Rendering;

            namespace Sample;

            public class ModsView : IView
            {
                public ModsView(Surface surface) { }
                public void Draw() { }
                public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
            }
            """;

        var (generated, _) = Run(source);

        Assert.Contains("using Arlecchino.Rendering;", generated, StringComparison.Ordinal);
        Assert.Contains("new ModsView(services.GetRequiredService<Surface>())", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewsOutsideTheGeneratedNamespaceAreReachedThroughAUsing()
    {
        const string source = """
            using System;
            using Arlecchino.Navigation;

            namespace Sample.Screens;

            public class ModsView : IView
            {
                public void Draw() { }
                public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
            }
            """;

        var (generated, _) = Run(source);

        Assert.Contains("using Sample.Screens;", generated, StringComparison.Ordinal);
        Assert.Contains("view = new ModsView()", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void RegistrationIsGeneratedEvenWhenNoViewExistsYet()
    {
        const string source = """
            namespace Sample;

            public class Nothing
            {
            }
            """;

        var (generated, diagnostics) = Run(source);

        Assert.Contains("public static ArlecchinoBuilder AddGeneratedViews(this ArlecchinoBuilder builder)",
            generated, StringComparison.Ordinal);
        Assert.Contains("public static ViewRoute None => ViewRoute.None;", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("switch (route.Name)", generated, StringComparison.Ordinal);

        var reported = Assert.Single(diagnostics, item => item.Id == "TSR004");
        Assert.Equal(DiagnosticSeverity.Info, reported.Severity);
    }

    [Fact]
    public void AbstractViewsAreSkipped()
    {
        const string source = """
            using System;
            using Arlecchino.Navigation;

            namespace Sample;

            public abstract class BaseView : IView
            {
                public abstract void Draw();
                public abstract ViewRoute Handle(ConsoleKeyInfo key);
            }

            public class RealView : BaseView
            {
                public override void Draw() { }
                public override ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
            }
            """;

        var (generated, _) = Run(source);

        Assert.DoesNotContain("ViewRoute Base =", generated, StringComparison.Ordinal);
        Assert.Contains("ViewRoute Real =", generated, StringComparison.Ordinal);
    }

    private static List<int> FindAll(string text, string needle)
    {
        var found = new List<int>();
        var index = text.IndexOf(needle, StringComparison.Ordinal);

        while (index >= 0)
        {
            found.Add(index);
            index = text.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
        }

        return found;
    }

    private sealed class FixedOptions : AnalyzerConfigOptionsProvider
    {
        public FixedOptions(Dictionary<string, string> values)
        {
            GlobalOptions = new Options(values);
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;

        private sealed class Options : AnalyzerConfigOptions
        {
            private readonly Dictionary<string, string> _values;

            public Options(Dictionary<string, string> values)
            {
                _values = values;
            }

            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
                _values.TryGetValue(key, out value);
        }
    }
}
