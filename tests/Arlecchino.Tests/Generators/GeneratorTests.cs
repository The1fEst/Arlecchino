using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Arlecchino.Generators;
using Xunit;

namespace Arlecchino.Tests.Generators;

public sealed class GeneratorTests
{
    private const string TwoViews = """
        using System;
        using Arlecchino.Input;
        using Arlecchino.Navigation;

        namespace Sample;

        public class ModsView : IArlecchinoView
        {
            public void Draw() { }
            public ViewRoute Handle(KeyPress key) => ViewRoute.None;
        }

        public class AboutView : IArlecchinoView
        {
            public void Draw() { }
            public ViewRoute Handle(KeyPress key) => ViewRoute.None;
        }
        """;

    private const string Words = """
        [localization]
        language = "en"

        [strings]
        Copy = "Copy"
        CopyManyTitle = "Copy {0} items"
        """;

    private const string TwoStores = """
        using Arlecchino.Rendering;
        using Arlecchino.Atoms;

        namespace Sample.Stores;

        public sealed class SettingsStore : IArlecchinoStore
        {
            public Atom<string> Profile { get; } = new TrackedAtom<string>("");
        }

        public sealed class DraftStore : IArlecchinoScopedStore
        {
            public DraftStore(Surface surface) { }
        }
        """;

    private const string AsyncStore = """
        using System.Threading;
        using System.Threading.Tasks;
        using Arlecchino.Atoms;

        namespace Sample.Stores;

        public sealed class CatalogStore : ArlecchinoAsyncStore
        {
            public Atom<string> Name { get; } = new LocalAtom<string>("");

            protected override Task LoadAsync(CancellationToken token) => Task.CompletedTask;
        }
        """;

    private const string ThreeWidgets = """
        using System;
        using Arlecchino.Hosting;
        using Arlecchino.Rendering;
        using Arlecchino.Widgets;

        namespace Sample.Panels;

        public sealed class ClockWidget : IArlecchinoWidget
        {
            public ClockWidget(ArlecchinoKeymap keymap) { }
            public void Draw(SurfaceRegion region) { }
        }

        public sealed class LabelWidget : IArlecchinoWidget
        {
            public required Func<string> Text { get; init; }
            public void Draw(SurfaceRegion region) { }
        }

        public sealed class GaugeWidget<T> : IArlecchinoWidget
        {
            public void Draw(SurfaceRegion region) { }
        }
        """;

    private static (string Source, ImmutableArray<Diagnostic> Diagnostics) RunWidgets(
        string source,
        string? generate = null)
    {
        var options = new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "Sample",
            ["build_property.ArlecchinoViewNamespace"] = "Sample.Views",
        };

        if (generate is not null)
        {
            options["build_property.ArlecchinoGenerateWidgets"] = generate;
        }

        return RunGenerator(new WidgetRegistrationGenerator(), source, options);
    }

    private const string TwoCommands = """
        using Arlecchino.Commands;
        using Arlecchino.Input;
        using Arlecchino.Navigation;
        using Arlecchino.Rendering;
        using System;

        namespace Sample.Actions;

        public sealed class QuitCommand : IArlecchinoCommand
        {
            public KeyBinding Binding => new KeyBinding(ConsoleKey.Q);
            public string Icon => "×";
            public string Label => "Quit";
            public ViewRoute Execute() => ViewRoute.None;
        }

        public sealed class ClearCommand : IArlecchinoCommand
        {
            public ClearCommand(Surface surface) { }
            public KeyBinding Binding => new KeyBinding(ConsoleKey.C);
            public string Icon => "";
            public string Label => "Clear";
            public ViewRoute Execute() => ViewRoute.None;
        }
        """;

    private static (string Source, ImmutableArray<Diagnostic> Diagnostics) RunCommands(
        string source,
        string? generate = null)
    {
        var options = new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "Sample",
            ["build_property.ArlecchinoViewNamespace"] = "Sample.Views",
        };

        if (generate is not null)
        {
            options["build_property.ArlecchinoGenerateCommands"] = generate;
        }

        return RunGenerator(new CommandRegistrationGenerator(), source, options);
    }

    private static (string Source, ImmutableArray<Diagnostic> Diagnostics) RunGenerator(
        IIncrementalGenerator generator,
        string source,
        Dictionary<string, string> options)
    {
        var compilation = CSharpCompilation.Create(
            "SampleApplication",
            [CSharpSyntaxTree.ParseText(source)],
            LoadedReferences(),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var driver = CSharpGeneratorDriver
            .Create([generator.AsSourceGenerator()], optionsProvider: new FixedOptions(options))
            .RunGenerators(compilation);

        var result = driver.GetRunResult();
        var output = result.GeneratedTrees.Length == 0 ? "" : result.GeneratedTrees[0].ToString();

        return (output, result.Diagnostics);
    }

    private static (string Source, ImmutableArray<Diagnostic> Diagnostics) RunStores(
        string source,
        string? generate = null)
    {
        var compilation = CSharpCompilation.Create(
            "SampleApplication",
            [CSharpSyntaxTree.ParseText(source)],
            LoadedReferences(),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var options = new Dictionary<string, string>
        {
            ["build_property.RootNamespace"] = "Sample",
            ["build_property.ArlecchinoViewNamespace"] = "Sample.Views",
        };

        if (generate is not null)
        {
            options["build_property.ArlecchinoGenerateStores"] = generate;
        }

        var driver = CSharpGeneratorDriver
            .Create([new StoreRegistrationGenerator().AsSourceGenerator()], optionsProvider: new FixedOptions(options))
            .RunGenerators(compilation);

        var result = driver.GetRunResult();
        var output = result.GeneratedTrees.Length == 0 ? "" : result.GeneratedTrees[0].ToString();

        return (output, result.Diagnostics);
    }

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
        var output = result.GeneratedTrees.Length == 0 ? "" : result.GeneratedTrees[0].ToString();

        return (output, result.Diagnostics);
    }

    private static IEnumerable<MetadataReference> LoadedReferences()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly is { IsDynamic: false, Location.Length: > 0 })
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
            using Arlecchino.Input;
            using Arlecchino.Navigation;

            namespace Sample
            {
                public class ModsView : IArlecchinoView
                {
                    public void Draw() { }
                    public ViewRoute Handle(KeyPress key) => ViewRoute.None;
                }
            }

            namespace Sample.Extra
            {
                using Arlecchino.Input;
                using Arlecchino.Navigation;

                public class ModsView : IArlecchinoView
                {
                    public void Draw() { }
                    public ViewRoute Handle(KeyPress key) => ViewRoute.None;
                }
            }
            """;

        var (output, diagnostics) = Run(source);
        var duplicate = Assert.Single(diagnostics, item => item.Id == "ARL001");

        Assert.Equal(DiagnosticSeverity.Warning, duplicate.Severity);
        Assert.Contains("Mods", duplicate.GetMessage(), StringComparison.Ordinal);
        Assert.Single(FindAll(output, "ViewRoute Mods ="));
    }

    [Fact]
    public void ViewWithoutAPublicConstructorIsReported()
    {
        const string source = """
            using System;
            using Arlecchino.Input;
            using Arlecchino.Navigation;

            namespace Sample;

            public class HiddenView : IArlecchinoView
            {
                private HiddenView() { }
                public void Draw() { }
                public ViewRoute Handle(KeyPress key) => ViewRoute.None;
            }
            """;

        var (output, diagnostics) = Run(source);
        var diagnostic = Assert.Single(diagnostics, item => item.Id == "ARL002");

        Assert.Contains("HiddenView", diagnostic.GetMessage(), StringComparison.Ordinal);
        Assert.DoesNotContain("new HiddenView(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AViewNestedInAnotherTypeIsNamedThroughIt()
    {
        const string source = """
            using System;
            using Arlecchino.Input;
            using Arlecchino.Navigation;

            namespace Sample;

            public static class Screens
            {
                public class ModsView : IArlecchinoView
                {
                    public void Draw() { }
                    public ViewRoute Handle(KeyPress key) => ViewRoute.None;
                }
            }
            """;

        var (output, _) = Run(source);

        Assert.Contains("new Screens.ModsView(", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AViewTheGeneratedCodeCannotReachIsLeftOut()
    {
        const string source = """
            using System;
            using Arlecchino.Input;
            using Arlecchino.Navigation;

            namespace Sample;

            public static class Screens
            {
                private sealed class HiddenView : IArlecchinoView
                {
                    public void Draw() { }
                    public ViewRoute Handle(KeyPress key) => ViewRoute.None;
                }
            }
            """;

        var (output, _) = Run(source);

        Assert.DoesNotContain("HiddenView", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoreTheGeneratedCodeCannotReachIsLeftOut()
    {
        const string source = """
            using Arlecchino.Atoms;

            namespace Sample;

            public static class Owner
            {
                private sealed class HiddenStore : IArlecchinoStore
                {
                }
            }
            """;

        var (output, _) = RunStores(source);

        Assert.DoesNotContain("HiddenStore", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoreNestedInAnotherTypeIsNamedThroughIt()
    {
        const string source = """
            using Arlecchino.Atoms;

            namespace Sample;

            public static class Owner
            {
                public sealed class SettingsStore : IArlecchinoStore
                {
                    public Atom<string> Profile { get; } = new TrackedAtom<string>("");
                }
            }
            """;

        var (output, _) = RunStores(source);

        Assert.Contains("new Owner.SettingsStore()", output, StringComparison.Ordinal);
    }

    [Fact]
    public void StoreWithoutAPublicConstructorIsReportedAndLeftOut()
    {
        const string source = """
            using Arlecchino.Atoms;

            namespace Sample.Stores;

            public sealed class HiddenStore : IArlecchinoStore
            {
                private HiddenStore() { }
            }
            """;

        var (output, diagnostics) = RunStores(source);

        Assert.Single(diagnostics, item => item.Id == "ARL005");
        Assert.DoesNotContain("HiddenStore", output, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingViewNamespaceIsReportedAsInformation()
    {
        var (source, diagnostics) = Run(TwoViews, viewNamespace: null);
        var diagnostic = Assert.Single(diagnostics, item => item.Id == "ARL003");

        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        Assert.Contains("Sample.Navigation", diagnostic.GetMessage(), StringComparison.Ordinal);
        Assert.Contains("namespace Sample.Navigation;", source, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedFactoryResolvesConstructorParametersFromTheContainer()
    {
        const string source = """
            using System;
            using Arlecchino.Input;
            using Arlecchino.Navigation;
            using Arlecchino.Rendering;

            namespace Sample;

            public class ModsView : IArlecchinoView
            {
                public ModsView(Surface surface) { }
                public void Draw() { }
                public ViewRoute Handle(KeyPress key) => ViewRoute.None;
            }
            """;

        var (output, _) = Run(source);

        Assert.Contains("using Arlecchino.Rendering;", output, StringComparison.Ordinal);
        Assert.Contains("new ModsView(services.GetRequiredService<Surface>())", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewsOutsideTheGeneratedNamespaceAreReachedThroughAUsing()
    {
        const string source = """
            using System;
            using Arlecchino.Input;
            using Arlecchino.Navigation;

            namespace Sample.Screens;

            public class ModsView : IArlecchinoView
            {
                public void Draw() { }
                public ViewRoute Handle(KeyPress key) => ViewRoute.None;
            }
            """;

        var (output, _) = Run(source);

        Assert.Contains("using Sample.Screens;", output, StringComparison.Ordinal);
        Assert.Contains("view = new ModsView()", output, StringComparison.Ordinal);
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

        var (output, diagnostics) = Run(source);

        Assert.Contains("public static ArlecchinoBuilder AddGeneratedViews(this ArlecchinoBuilder builder)",
            output,
            StringComparison.Ordinal);
        Assert.Contains("public static ViewRoute None => ViewRoute.None;", output, StringComparison.Ordinal);
        Assert.DoesNotContain("switch (route.Name)", output, StringComparison.Ordinal);

        var diagnostic = Assert.Single(diagnostics, item => item.Id == "ARL004");
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
    }

    [Fact]
    public void AStoreThatLoadsItselfIsAlsoRegisteredForTheHostToStart()
    {
        var (output, diagnostics) = RunStores(AsyncStore);

        Assert.Contains(
            "builder.Services.AddSingleton(static services => new CatalogStore());",
            output,
            StringComparison.Ordinal);

        Assert.Contains(
            "builder.Services.AddSingleton<global::Arlecchino.Atoms.ArlecchinoAsyncStore>(" +
            "static services => services.GetRequiredService<CatalogStore>());",
            output,
            StringComparison.Ordinal);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void StoresAreRegisteredWithTheLifetimeTheirMarkerAsksFor()
    {
        var (output, diagnostics) = RunStores(TwoStores);

        Assert.Contains("namespace Sample.Views;", output, StringComparison.Ordinal);
        Assert.Contains("using Sample.Stores;", output, StringComparison.Ordinal);
        Assert.Contains(
            "builder.Services.AddSingleton(static services => new SettingsStore());",
            output,
            StringComparison.Ordinal);
        Assert.Contains(
            "builder.Services.AddScoped(static services => new DraftStore(services.GetRequiredService<Surface>()));",
            output,
            StringComparison.Ordinal);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void StoreRegistrationIsGeneratedEvenWhenNoStoreExistsYet()
    {
        var (output, _) = RunStores(TwoViews);

        Assert.Contains(
            "public static ArlecchinoBuilder AddGeneratedStores(this ArlecchinoBuilder builder)",
            output,
            StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Services.Add", output, StringComparison.Ordinal);
    }

    [Fact]
    public void NoStoresAreGeneratedWhenTheProjectTurnsThemOff()
    {
        var (output, _) = RunStores(TwoStores, generate: "false");

        Assert.Equal("", output);
    }

    [Fact]
    public void StoreWithoutAPublicConstructorIsReported()
    {
        const string source = """
            using Arlecchino.Atoms;

            namespace Sample;

            public sealed class HiddenStore : IArlecchinoStore
            {
                private HiddenStore() { }
            }
            """;

        var (_, diagnostics) = RunStores(source);
        var diagnostic = Assert.Single(diagnostics, item => item.Id == "ARL005");

        Assert.Contains("HiddenStore", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public void WidgetsOfTheProjectAreRegisteredAsSingletons()
    {
        var (output, diagnostics) = RunWidgets(ThreeWidgets);

        Assert.Contains("using Sample.Panels;", output, StringComparison.Ordinal);
        Assert.Contains(
            "builder.Services.AddSingleton(static services => new ClockWidget(services.GetRequiredService<ArlecchinoKeymap>()));",
            output,
            StringComparison.Ordinal);
        Assert.DoesNotContain("LabelWidget", output, StringComparison.Ordinal);
        Assert.DoesNotContain("GaugeWidget", output, StringComparison.Ordinal);

        var empty = MessagesOf(diagnostics, "ARL007");

        Assert.Equal(2, empty.Count);
        Assert.Contains(empty, message => message.Contains("required members", StringComparison.Ordinal));
        Assert.Contains(empty, message => message.Contains("generic", StringComparison.Ordinal));
    }

    [Fact]
    public void WidgetRegistrationIsGeneratedEvenWhenNoWidgetExistsYet()
    {
        var (output, _) = RunWidgets(TwoViews);

        Assert.Contains(
            "public static ArlecchinoBuilder AddGeneratedWidgets(this ArlecchinoBuilder builder)",
            output,
            StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Services.Add", output, StringComparison.Ordinal);
    }

    [Fact]
    public void NoWidgetsAreGeneratedWhenTheProjectTurnsThemOff()
    {
        var (output, _) = RunWidgets(ThreeWidgets, generate: "false");

        Assert.Equal("", output);
    }

    [Fact]
    public void CommandsOfTheProjectAreRegisteredWithTheirDependencies()
    {
        var (output, diagnostics) = RunCommands(TwoCommands);

        Assert.Contains("namespace Sample.Views;", output, StringComparison.Ordinal);
        Assert.Contains("using Sample.Actions;", output, StringComparison.Ordinal);
        Assert.Contains(
            "builder.Services.AddSingleton<IArlecchinoCommand>(static services => new QuitCommand());",
            output,
            StringComparison.Ordinal);
        Assert.Contains(
            "builder.Services.AddSingleton<IArlecchinoCommand>(static services => new ClearCommand(services.GetRequiredService<Surface>()));",
            output,
            StringComparison.Ordinal);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void CommandRegistrationIsGeneratedEvenWhenNoCommandExistsYet()
    {
        var (output, _) = RunCommands(TwoViews);

        Assert.Contains(
            "public static ArlecchinoBuilder AddGeneratedCommands(this ArlecchinoBuilder builder)",
            output,
            StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Services.Add", output, StringComparison.Ordinal);
    }

    [Fact]
    public void NoCommandsAreGeneratedWhenTheProjectTurnsThemOff()
    {
        var (output, _) = RunCommands(TwoCommands, generate: "false");

        Assert.Equal("", output);
    }

    [Fact]
    public void CommandWithoutAPublicConstructorIsReported()
    {
        const string source = """
            using Arlecchino.Commands;
            using Arlecchino.Input;
            using Arlecchino.Navigation;
            using System;

            namespace Sample.Actions;

            public sealed class HiddenCommand : IArlecchinoCommand
            {
                private HiddenCommand() { }
                public KeyBinding Binding => new KeyBinding(ConsoleKey.H);
                public string Icon => "";
                public string Label => "Hidden";
                public ViewRoute Execute() => ViewRoute.None;
            }
            """;

        var (output, diagnostics) = RunCommands(source);

        Assert.Single(diagnostics, item => item.Id == "ARL006");
        Assert.DoesNotContain("HiddenCommand", output, StringComparison.Ordinal);
    }

    [Fact]
    public void CommandsAreRegisteredInAStableOrder()
    {
        var (output, _) = RunCommands(TwoCommands);

        Assert.True(
            output.IndexOf("ClearCommand", StringComparison.Ordinal) <
            output.IndexOf("QuitCommand", StringComparison.Ordinal));
    }

    [Fact]
    public void AbstractViewsAreSkipped()
    {
        const string source = """
            using System;
            using Arlecchino.Input;
            using Arlecchino.Navigation;

            namespace Sample;

            public abstract class BaseView : IArlecchinoView
            {
                public abstract void Draw();
                public abstract ViewRoute Handle(KeyPress key);
            }

            public class RealView : BaseView
            {
                public override void Draw() { }
                public override ViewRoute Handle(KeyPress key) => ViewRoute.None;
            }
            """;

        var (output, _) = Run(source);

        Assert.DoesNotContain("ViewRoute Base =", output, StringComparison.Ordinal);
        Assert.Contains("ViewRoute Real =", output, StringComparison.Ordinal);
    }

    private static List<string> MessagesOf(ImmutableArray<Diagnostic> diagnostics, string id)
    {
        var messages = new List<string>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Id == id)
            {
                messages.Add(diagnostic.GetMessage());
            }
        }

        return messages;
    }

    /// <summary>Every entry of the file becomes a name, and the default text sits in its documentation.</summary>
    [Fact]
    public void LocalizationBecomesAnEnumOfNames()
    {
        var (output, diagnostics) = RunLocalization(Words);

        Assert.Empty(diagnostics);
        Assert.Contains("public enum LocString", output, StringComparison.Ordinal);
        Assert.Contains("Copy = 0,", output, StringComparison.Ordinal);
        Assert.Contains("CopyManyTitle = 1,", output, StringComparison.Ordinal);
        Assert.Contains("<c>Copy {0} items</c>", output, StringComparison.Ordinal);
    }

    /// <summary>
    /// The factories that name a key come with it. They cannot live in the framework — a
    /// <c>ViewCommand</c> would have to name the enum, and the enum is written here, per application,
    /// out of that application's own file.
    /// </summary>
    [Fact]
    public void LocalizationBringsTheFactoriesThatNameAKey()
    {
        var (output, _) = RunLocalization(Words);

        Assert.Contains("public static class Bind", output, StringComparison.Ordinal);
        Assert.Contains("ViewCommand To(KeyBinding binding, LocString name, Action run)",
            output,
            StringComparison.Ordinal);
        Assert.Contains("ViewCommand Going(", output, StringComparison.Ordinal);
        Assert.Contains("ViewCommand When(", output, StringComparison.Ordinal);
        Assert.Contains("Label = () => Localization.Loc(name)", output, StringComparison.Ordinal);
    }

    /// <summary>A localization missing the default language is an error rather than a silent miss.</summary>
    [Fact]
    public void ALocalizationWithoutTheDefaultLanguageIsAnError()
    {
        var (_, diagnostics) = RunLocalization("""
            [localization]
            language = "fr"

            [strings]
            Copy = "Copier"
            """);

        Assert.Contains(diagnostics, static single => single.Id == "ARL022");
    }

    private static List<int> FindAll(string text, string needle)
    {
        var single = new List<int>();
        var index = text.IndexOf(needle, StringComparison.Ordinal);

        while (index >= 0)
        {
            single.Add(index);
            index = text.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
        }

        return single;
    }

    private static (string Source, ImmutableArray<Diagnostic> Diagnostics) RunLocalization(string toml)
    {
        var compilation = CSharpCompilation.Create(
            "SampleApplication",
            [],
            LoadedReferences(),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var options = new Dictionary<string, string> { ["build_property.RootNamespace"] = "Sample" };

        var driver = CSharpGeneratorDriver
            .Create(
                [new LocalizationGenerator().AsSourceGenerator()],
                additionalTexts: [new Toml("Localization/Localization.toml", toml)],
                optionsProvider: new FixedOptions(options))
            .RunGenerators(compilation);

        var result = driver.GetRunResult();
        var output = result.GeneratedTrees.Length == 0 ? "" : result.GeneratedTrees[0].ToString();

        return (output, result.Diagnostics);
    }

    private sealed class Toml : AdditionalText
    {
        private readonly string _text;

        public Toml(string path, string text)
        {
            Path = path;
            _text = text;
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default) =>
            SourceText.From(_text);
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
