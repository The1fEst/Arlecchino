using System;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Navigation;

/// <summary>Serves the views registered by name through <c>AddView</c>.</summary>
internal sealed class RegisteredViewFactory : IArlecchinoViewFactory
{
    private readonly ViewRegistrations _registrations;

    /// <summary>Creates the factory.</summary>
    /// <param name="registrations">The registered route names.</param>
    public RegisteredViewFactory(ViewRegistrations registrations)
    {
        _registrations = registrations;
    }

    /// <summary>Builds a view when its route was registered explicitly.</summary>
    /// <param name="services">The scope this screen lives in.</param>
    /// <param name="route">The route being shown.</param>
    /// <param name="view">The view, when the route is registered.</param>
    /// <returns><c>true</c> when the route was registered.</returns>
    public bool TryCreate(IServiceProvider services, ViewRoute route, [NotNullWhen(true)] out IArlecchinoView? view)
    {
        if (route.IsNone || !_registrations.TryGet(route.Name, out var factory))
        {
            view = null;
            return false;
        }

        view = factory(services);
        return true;
    }
}
