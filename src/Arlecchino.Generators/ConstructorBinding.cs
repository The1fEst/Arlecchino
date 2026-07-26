using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

internal static class ConstructorBinding
{
    public static IReadOnlyList<ParameterModel> Of(INamedTypeSymbol symbol)
    {
        var constructor = symbol.InstanceConstructors
            .Where(static item => item.DeclaredAccessibility == Accessibility.Public)
            .OrderByDescending(static item => item.Parameters.Length)
            .FirstOrDefault();

        if (constructor == null)
        {
            return Array.Empty<ParameterModel>();
        }

        return constructor.Parameters
            .Select(static parameter => new ParameterModel(
                TypeNames.Of(parameter.Type),
                parameter.Type.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : parameter.Type.ContainingNamespace.ToDisplayString()))
            .ToArray();
    }

    public static string CreateExpression(string typeName, IReadOnlyList<ParameterModel> parameters)
    {
        if (parameters.Count == 0)
        {
            return $"new {typeName}()";
        }

        var services = parameters.Select(static parameter =>
            $"services.GetRequiredService<{parameter.TypeName}>()");

        return $"new {typeName}({string.Join(", ", services)})";
    }
}

internal sealed class ParameterModel
{
    public ParameterModel(string typeName, string @namespace)
    {
        TypeName = typeName;
        Namespace = @namespace;
    }

    public string TypeName { get; }
    public string Namespace { get; }
}
