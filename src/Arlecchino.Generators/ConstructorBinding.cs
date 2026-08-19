using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Arlecchino.Generators;

internal static class ConstructorBinding
{
    private const int Longest = 120;

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

    /// <summary>
    /// The expression that builds the type out of the container, on one line where it fits and with one
    /// argument to a line where it does not. The width is the one the code style holds every line to.
    /// </summary>
    /// <param name="typeName">The type to build.</param>
    /// <param name="parameters">What its constructor takes.</param>
    /// <param name="indent">The indent the statement holding it starts at.</param>
    /// <param name="column">The column the expression starts at, counting what stands to its left.</param>
    /// <returns>The expression.</returns>
    public static string CreateExpression(
        string typeName,
        IReadOnlyList<ParameterModel> parameters,
        string indent = "",
        int column = 0)
    {
        if (parameters.Count == 0)
        {
            return $"new {typeName}()";
        }

        var services = parameters
            .Select(static parameter => $"services.GetRequiredService<{parameter.TypeName}>()")
            .ToArray();

        var line = $"new {typeName}({string.Join(", ", services)})";

        if (column + line.Length <= Longest)
        {
            return line;
        }

        var step = indent + "    ";

        return $"new {typeName}(\n{step}{string.Join($",\n{step}", services)})";
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
