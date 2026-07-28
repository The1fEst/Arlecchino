using System.Threading;
using System.Threading.Tasks;

namespace Arlecchino.Atoms;

/// <summary>
/// A store that has to fetch something before it holds the truth — settings read from disk, a session
/// restored from a server, a catalogue that lives in a file. The framework starts the load as the
/// application starts and reports where it got to, so a store needs neither a worker of its own nor a
/// <c>TaskCompletionSource</c> for the screens waiting on it.
///
/// The first frame is drawn without waiting: a terminal that hangs black on a slow disk is worse than
/// a screen that says it is loading. A view reads <c>StoreLoading</c> to draw a spinner, a notice, or
/// simply the values the atoms started with.
///
/// <code>
/// public sealed class SettingsStore : IArlecchinoAsyncStore
/// {
///     public TrackedAtom&lt;string&gt; Server { get; } = new("127.0.0.1");
///
///     public async Task LoadAsync(CancellationToken token)
///     {
///         var saved = await Settings.ReadAsync(token);
///
///         Server.Post(saved.Server);
///     }
/// }
/// </code>
/// </summary>
public interface IArlecchinoAsyncStore : IArlecchinoStore
{
    /// <summary>
    /// Fetches what the store needs. It runs off the drawing thread, so what it loads reaches the
    /// atoms through <c>Post</c> — writing <c>Value</c> from here throws, and says so.
    ///
    /// Throwing is a normal outcome: the loading state turns to failed, the exception is kept for the
    /// view to draw, and the rest of the application carries on with whatever the atoms hold.
    /// </summary>
    /// <param name="token">Cancelled when the application is shutting down.</param>
    /// <returns>A task that completes when the store is ready.</returns>
    Task LoadAsync(CancellationToken token);
}
