using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Arlecchino.Generators;

[Generator]
public sealed class ViewNavigationGenerator : IIncrementalGenerator
{
    private const string ViewInterfaceNamespace = "Arlecchino.Navigation";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var settings = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        {
            provider.GlobalOptions.TryGetValue("build_property.ArlecchinoGenerateViews", out var enabled);
            provider.GlobalOptions.TryGetValue("build_property.ArlecchinoViewNamespace", out var viewNamespace);
            provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

            var generate = string.IsNullOrEmpty(enabled) ||
                           !string.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase);

            var namespaceWasChosen = !string.IsNullOrEmpty(viewNamespace);

            if (!namespaceWasChosen)
            {
                viewNamespace = string.IsNullOrEmpty(rootNamespace)
                    ? "Views"
                    : rootNamespace + ".Navigation";
            }

            return new Settings(generate, viewNamespace!, namespaceWasChosen);
        });

        var viewDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                static (ctx, _) => GetView(ctx))
            .Where(static view => view != null);

        context.RegisterSourceOutput(viewDeclarations.Collect().Combine(settings), static (ctx, pair) =>
        {
            var (views, settings) = pair;
            if (!settings.IsEnabled)
            {
                return;
            }

            var declared = views.OfType<ViewModel>().ToArray();

            var named = declared
                .GroupBy(static view => view.RouteName)
                .Select(group => ReportDuplicates(ctx, group))
                .OrderBy(static view => view.RouteName == "Default" ? 0 : 1)
                .ThenBy(static view => view.RouteName, StringComparer.Ordinal)
                .ToArray();

            foreach (var view in named)
            {
                if (!view.HasPublicConstructor)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        ViewDiagnostics.NoPublicConstructor, view.Location, view.TypeName));
                }
            }

            var viewModels = named.Where(static view => view.HasPublicConstructor).ToArray();

            if (!settings.NamespaceWasChosen)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    ViewDiagnostics.ViewNamespaceNotSet,
                    viewModels.Length == 0 ? Location.None : viewModels[0].Location,
                    settings.ViewNamespace));
            }

            if (viewModels.Length == 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(ViewDiagnostics.NoViews, Location.None));
            }

            ctx.AddSource("ArlecchinoViewNavigation.g.cs",
                SourceText.From(Generate(viewModels, settings.ViewNamespace), Encoding.UTF8));
        });
    }

    private static ViewModel? GetView(GeneratorSyntaxContext context)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (symbol.IsAbstract || !ImplementsView(symbol))
        {
            return null;
        }

        var typeName = TypeNames.Of(symbol);
        var routeName = TrimViewSuffix(symbol.Name);
        var containingNamespace = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();
        var constructorParameters = ConstructorBinding.Of(symbol);
        var hasPublicConstructor = symbol.InstanceConstructors
            .Any(static item => item.DeclaredAccessibility == Accessibility.Public);

        return new(
            routeName,
            typeName,
            containingNamespace,
            constructorParameters,
            hasPublicConstructor,
            declaration.Identifier.GetLocation());
    }

    private static ViewModel ReportDuplicates(SourceProductionContext context, IGrouping<string, ViewModel> group)
    {
        var kept = group.First();

        foreach (var ignored in group.Skip(1))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ViewDiagnostics.DuplicateRoute, ignored.Location, ignored.TypeName, kept.TypeName, group.Key));
        }

        return kept;
    }

    private static bool ImplementsView(INamedTypeSymbol symbol)
    {
        return symbol.AllInterfaces.Any(static item =>
            item.Name == "IArlecchinoView" &&
            item.ContainingNamespace.ToDisplayString() == ViewInterfaceNamespace);
    }

    private static string TrimViewSuffix(string name)
    {
        return name.EndsWith("View", StringComparison.Ordinal) && name.Length > "View".Length
            ? name.Substring(0, name.Length - "View".Length)
            : name;
    }

    private static string Generate(IReadOnlyList<ViewModel> views, string viewNamespace)
    {
        var builder = new StringBuilder();

        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Diagnostics.CodeAnalysis;");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using Arlecchino.Hosting;");
        builder.AppendLine("using Arlecchino.Navigation;");

        foreach (var namespaceName in views
                     .SelectMany(static view => view.ConstructorParameters
                         .Select(static parameter => parameter.Namespace)
                         .Concat([view.Namespace]))
                     .Where(namespaceName => namespaceName.Length > 0 &&
                                             namespaceName != "System" &&
                                             namespaceName != "System.Diagnostics.CodeAnalysis" &&
                                             namespaceName != "Microsoft.Extensions.DependencyInjection" &&
                                             namespaceName != "Arlecchino.Hosting" &&
                                             namespaceName != "Arlecchino.Navigation" &&
                                             namespaceName != viewNamespace)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(static namespaceName => namespaceName, StringComparer.Ordinal))
        {
            builder.Append("using ").Append(namespaceName).AppendLine(";");
        }

        builder.AppendLine();
        builder.Append("namespace ").Append(viewNamespace).AppendLine(";");
        builder.AppendLine();
        builder.AppendLine("public static class ViewKind");
        builder.AppendLine("{");
        builder.AppendLine("    public static ViewRoute None => ViewRoute.None;");

        foreach (var view in views)
        {
            builder.Append("    public static readonly ViewRoute ").Append(view.RouteName)
                .Append(" = new ViewRoute(\"").Append(view.RouteName).AppendLine("\");");
        }

        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("public sealed class GeneratedViewFactory : IArlecchinoViewFactory");
        builder.AppendLine("{");
        builder.AppendLine("    public bool TryCreate(IServiceProvider services, ViewRoute route, [NotNullWhen(true)] out IArlecchinoView? view)");
        builder.AppendLine("    {");

        if (views.Count == 0)
        {
            builder.AppendLine("        view = null;");
            builder.AppendLine("        return false;");
        }
        else
        {
            builder.AppendLine("        switch (route.Name)");
            builder.AppendLine("        {");

            foreach (var view in views)
            {
                builder.Append("            case \"").Append(view.RouteName).AppendLine("\":");
                builder.Append("                view = ")
                    .Append(ConstructorBinding.CreateExpression(view.TypeName, view.ConstructorParameters))
                    .AppendLine(";");
                builder.AppendLine("                return true;");
            }

            builder.AppendLine("            default:");
            builder.AppendLine("                view = null;");
            builder.AppendLine("                return false;");
            builder.AppendLine("        }");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("public static class GeneratedViewRegistration");
        builder.AppendLine("{");
        builder.AppendLine("    public static ArlecchinoBuilder AddGeneratedViews(this ArlecchinoBuilder builder)");
        builder.AppendLine("    {");
        builder.AppendLine("        return builder.AddViewFactory<GeneratedViewFactory>();");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private sealed class Settings
    {
        public Settings(bool generate, string viewNamespace, bool namespaceWasChosen)
        {
            IsEnabled = generate;
            ViewNamespace = viewNamespace;
            NamespaceWasChosen = namespaceWasChosen;
        }

        public bool IsEnabled { get; }
        public string ViewNamespace { get; }
        public bool NamespaceWasChosen { get; }
    }

    private sealed class ViewModel
    {
        public ViewModel(
            string routeName,
            string typeName,
            string @namespace,
            IReadOnlyList<ParameterModel> constructorParameters,
            bool hasPublicConstructor,
            Location location)
        {
            RouteName = routeName;
            TypeName = typeName;
            Namespace = @namespace;
            ConstructorParameters = constructorParameters;
            HasPublicConstructor = hasPublicConstructor;
            Location = location;
        }

        public string RouteName { get; }
        public string TypeName { get; }
        public string Namespace { get; }
        public IReadOnlyList<ParameterModel> ConstructorParameters { get; }
        public bool HasPublicConstructor { get; }
        public Location Location { get; }
    }
}
