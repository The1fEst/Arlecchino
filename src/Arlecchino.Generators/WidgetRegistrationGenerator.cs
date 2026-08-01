using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Arlecchino.Generators;

[Generator]
public sealed class WidgetRegistrationGenerator : IIncrementalGenerator
{
    private const string WidgetInterfaceNamespace = "Arlecchino.Widgets";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var settings = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        {
            provider.GlobalOptions.TryGetValue("build_property.ArlecchinoGenerateWidgets", out var enabled);
            provider.GlobalOptions.TryGetValue("build_property.ArlecchinoViewNamespace", out var viewNamespace);
            provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

            var generate = string.IsNullOrEmpty(enabled) ||
                           !string.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(viewNamespace))
            {
                viewNamespace = string.IsNullOrEmpty(rootNamespace)
                    ? "Views"
                    : rootNamespace + ".Navigation";
            }

            return new Settings(generate, viewNamespace!);
        });

        var widgetDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                static (ctx, _) => GetWidget(ctx))
            .Where(static widget => widget != null);

        context.RegisterSourceOutput(widgetDeclarations.Collect().Combine(settings),
            static (ctx, pair) =>
            {
                var (widgets, settings) = pair;
                if (!settings.IsEnabled)
                {
                    return;
                }

                var declared = widgets
                    .OfType<WidgetModel>()
                    .OrderBy(static widget => widget.TypeName, StringComparer.Ordinal)
                    .ToArray();

                foreach (var widget in declared.Where(static widget => widget.Obstacle.Length > 0))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        WidgetDiagnostics.CannotBeBuilt,
                        widget.Location,
                        widget.TypeName,
                        widget.Obstacle));
                }

                var buildable = declared.Where(static widget => widget.Obstacle.Length == 0).ToArray();

                ctx.AddSource("ArlecchinoWidgetRegistration.g.cs",
                    SourceText.From(Generate(buildable, settings.WidgetNamespace), Encoding.UTF8));
            });
    }

    private static WidgetModel? GetWidget(GeneratorSyntaxContext context)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (symbol.IsAbstract || !TypeNames.IsReachable(symbol) || !ImplementsWidget(symbol))
        {
            return null;
        }

        var typeName = TypeNames.Of(symbol);
        var containingNamespace = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        return new(
            typeName,
            containingNamespace,
            ConstructorBinding.Of(symbol),
            ObstacleTo(symbol),
            declaration.Identifier.GetLocation());
    }

    private static string ObstacleTo(INamedTypeSymbol symbol)
    {
        if (symbol.IsGenericType)
        {
            return "it is generic, so there is no single type to register";
        }

        if (!symbol.InstanceConstructors.Any(static item => item.DeclaredAccessibility == Accessibility.Public))
        {
            return "it has no public constructor";
        }

        return HasRequiredMembers(symbol)
            ? "it has required members, which a factory cannot fill in"
            : string.Empty;
    }

    private static bool HasRequiredMembers(INamedTypeSymbol symbol)
    {
        for (var type = symbol; type is not null; type = type.BaseType)
        {
            foreach (var member in type.GetMembers())
            {
                if (member is IPropertySymbol { IsRequired: true } or IFieldSymbol { IsRequired: true })
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool ImplementsWidget(INamedTypeSymbol symbol)
    {
        return symbol.AllInterfaces.Any(static item =>
            item.Name == "IArlecchinoWidget" &&
            item.ContainingNamespace.ToDisplayString() == WidgetInterfaceNamespace);
    }

    private static string Generate(IReadOnlyList<WidgetModel> widgets, string widgetNamespace)
    {
        return $$"""
            // <auto-generated/>
            #nullable enable
            using Microsoft.Extensions.DependencyInjection;
            using Arlecchino.Hosting;{{Imports(widgets, widgetNamespace)}}

            namespace {{widgetNamespace}};

            public static class GeneratedWidgetRegistration
            {
                public static ArlecchinoBuilder AddGeneratedWidgets(this ArlecchinoBuilder builder)
                {
            {{Body(widgets)}}
                }
            }

            """;
    }

    /// <summary>
    /// The namespaces the generated file needs beyond the ones it always writes. Each is led by a
    /// newline rather than followed by one, so that none of them leaves a blank line behind.
    /// </summary>
    /// <param name="widgets">The widgets being registered.</param>
    /// <param name="widgetNamespace">Where the generated file itself lives.</param>
    /// <returns>The lines, or an empty string.</returns>
    private static string Imports(IReadOnlyList<WidgetModel> widgets, string widgetNamespace)
    {
        var names = widgets
            .SelectMany(static widget => widget.ConstructorParameters
                .Select(static parameter => parameter.Namespace)
                .Concat([widget.Namespace]))
            .Where(namespaceName => namespaceName.Length > 0 &&
                                    namespaceName != "Microsoft.Extensions.DependencyInjection" &&
                                    namespaceName != "Arlecchino.Hosting" &&
                                    namespaceName != widgetNamespace)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static namespaceName => namespaceName, StringComparer.Ordinal)
            .Select(static namespaceName => $"using {namespaceName};");

        return string.Concat(names.Select(static line => "\n" + line));
    }

    private static string Body(IReadOnlyList<WidgetModel> widgets)
    {
        var registrations = widgets.Select(static widget =>
            "        builder.Services.AddSingleton(static services => " +
            $"{ConstructorBinding.CreateExpression(widget.TypeName, widget.ConstructorParameters)});");

        return string.Join("\n", (string[])[.. registrations, "        return builder;"]);
    }

    private sealed class Settings
    {
        public Settings(bool generate, string widgetNamespace)
        {
            IsEnabled = generate;
            WidgetNamespace = widgetNamespace;
        }

        public bool IsEnabled { get; }
        public string WidgetNamespace { get; }
    }

    private sealed class WidgetModel
    {
        public WidgetModel(
            string typeName,
            string @namespace,
            IReadOnlyList<ParameterModel> constructorParameters,
            string obstacle,
            Location location)
        {
            TypeName = typeName;
            Namespace = @namespace;
            ConstructorParameters = constructorParameters;
            Obstacle = obstacle;
            Location = location;
        }

        public string TypeName { get; }
        public string Namespace { get; }
        public IReadOnlyList<ParameterModel> ConstructorParameters { get; }
        public string Obstacle { get; }
        public Location Location { get; }
    }
}
