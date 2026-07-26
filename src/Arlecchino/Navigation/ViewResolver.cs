using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Arlecchino.Navigation;

/// <summary>
/// Turns a route into a view by asking each registered factory in registration order. Each screen is
/// built inside its own container scope, which is what lets a view take a scoped service and have it
/// released the moment the screen goes away.
/// </summary>
public sealed class ViewResolver
{
    private readonly IViewFactory[] _factories;
    private readonly IServiceScopeFactory _scopes;

    /// <summary>Creates the resolver.</summary>
    /// <param name="factories">Factories to ask, in the order they were registered.</param>
    /// <param name="scopes">Where the per-screen scopes come from.</param>
    public ViewResolver(IEnumerable<IViewFactory> factories, IServiceScopeFactory scopes)
    {
        _factories = factories.ToArray();
        _scopes = scopes;
    }

    /// <summary>Builds the view for a route, in a scope of its own.</summary>
    /// <param name="route">The route to show.</param>
    /// <returns>The view and the scope it lives in; dispose it when navigating away.</returns>
    /// <exception cref="InvalidOperationException">
    /// No factory owns the route; the message names both ways to register it.
    /// </exception>
    public ActiveView Create(ViewRoute route)
    {
        var scope = _scopes.CreateScope();

        try
        {
            foreach (var factory in _factories)
            {
                if (factory.TryCreate(scope.ServiceProvider, route, out var view))
                {
                    return new(view, scope);
                }
            }
        }
        catch
        {
            scope.Dispose();
            throw;
        }

        scope.Dispose();

        throw new InvalidOperationException(
            $"No view is registered for route '{route}'. Register it with AddView<T>(\"{route}\") or let the Arlecchino generator discover an IArlecchinoView named '{route}View'.");
    }
}
