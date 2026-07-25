using System;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Navigation;

/// <summary>
/// Builds views for routes. Register one with <c>AddViewFactory&lt;T&gt;()</c> to serve a whole
/// family of routes at once — a plugin directory, or routes carrying an id in the name.
/// </summary>
public interface IViewFactory
{
    /// <summary>Creates the view for a route.</summary>
    /// <param name="services">
    /// The scope this screen lives in. Resolve from it rather than from a captured container, so
    /// scoped services belong to the screen and go away with it.
    /// </param>
    /// <param name="route">The route being shown.</param>
    /// <param name="view">The view, when this factory owns the route.</param>
    /// <returns><c>false</c> for routes you do not own, so the next factory gets a turn.</returns>
    bool TryCreate(IServiceProvider services, ViewRoute route, [NotNullWhen(true)] out IView? view);
}
