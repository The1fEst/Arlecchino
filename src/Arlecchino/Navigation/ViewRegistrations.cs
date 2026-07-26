using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Navigation;

/// <summary>
/// Views registered by name through <c>AddView</c>. Consulted before the generated factory, so an
/// explicit registration wins over a generated route of the same name.
/// </summary>
public sealed class ViewRegistrations
{
    private readonly Dictionary<string, Func<IServiceProvider, IArlecchinoView>> _factories = new(StringComparer.Ordinal);

    /// <summary>Names that have been registered.</summary>
    public IReadOnlyCollection<string> Routes => _factories.Keys;

    /// <summary>Registers how to build a view for a route name, replacing any earlier entry.</summary>
    /// <param name="name">Route name.</param>
    /// <param name="factory">How to build the view from the container.</param>
    /// <exception cref="ArgumentException">The name is empty.</exception>
    public void Add(string name, Func<IServiceProvider, IArlecchinoView> factory)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("A view route name must not be empty.", nameof(name));
        }

        _factories[name] = factory;
    }

    /// <summary>Looks up how to build a view.</summary>
    /// <param name="name">Route name.</param>
    /// <param name="factory">The registered builder, when there is one.</param>
    /// <returns><c>true</c> when the name is registered.</returns>
    public bool TryGet(string name, [NotNullWhen(true)] out Func<IServiceProvider, IArlecchinoView>? factory)
    {
        return _factories.TryGetValue(name, out factory);
    }
}
