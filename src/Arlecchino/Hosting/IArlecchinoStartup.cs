using Arlecchino.Navigation;

namespace Arlecchino.Hosting;

/// <summary>
/// Work to do once before the first frame. Several may be registered; they run in registration order,
/// and the last route that is not <see cref="ViewRoute.None"/> decides where the application opens.
/// </summary>
public interface IArlecchinoStartup
{
    /// <summary>
    /// Runs the work. Return a route to open somewhere other than the configured start, or
    /// <see cref="ViewRoute.None"/> to leave that alone.
    /// </summary>
    /// <returns>Where to open, or none.</returns>
    ViewRoute Start();
}
