using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Atoms;

namespace Arlecchino.Navigation;

/// <summary>
/// How long the screen is on. Take it in a view's constructor to tie background work, subscriptions
/// and anything else disposable to the screen: it is scoped, so navigating away cancels the token and
/// releases everything registered here. Without it a load that outlives its screen keeps running and
/// hands its result to a view nobody can see any more.
/// </summary>
public sealed class ViewLifetime : IDisposable
{
    private readonly CancellationTokenSource _closing = new();
    private readonly List<IDisposable> _owned = [];
    private readonly UiDispatcher _dispatcher;

    /// <summary>Creates the lifetime. Resolved once per screen.</summary>
    /// <param name="dispatcher">Handed to the background state this creates.</param>
    public ViewLifetime(UiDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        Closing = _closing.Token;
    }

    /// <summary>
    /// Cancelled when the screen goes away. Pass it into work you start yourself so it stops with the
    /// screen rather than with the application. It stays readable afterwards, so background work that
    /// comes back late can still see that the screen has gone.
    /// </summary>
    public CancellationToken Closing { get; }

    /// <summary>
    /// Creates background state that stops when the screen does — the usual way to load something for
    /// one screen.
    /// </summary>
    /// <typeparam name="T">What is being loaded.</typeparam>
    /// <param name="initial">What to hold until the first load finishes.</param>
    /// <returns>The state, already tied to this screen.</returns>
    public AsyncAtom<T> Loading<T>(T? initial = default)
    {
        var loading = new AsyncAtom<T>(_dispatcher, initial);

        OnClose(loading.Cancel);
        return loading;
    }

    /// <summary>
    /// Hands something over to the screen's lifetime — a subscription, a timer, a file handle. It is
    /// disposed when the screen goes away, in the order it was handed over.
    /// </summary>
    /// <typeparam name="T">Anything disposable.</typeparam>
    /// <param name="resource">What to look after.</param>
    /// <returns>The same object, so it can be assigned in one line.</returns>
    public T Track<T>(T resource)
        where T : IDisposable
    {
        _owned.Add(resource);
        return resource;
    }

    /// <summary>Runs something when the screen goes away, before the scope is released.</summary>
    /// <param name="action">What to run.</param>
    public void OnClose(Action action) => Closing.Register(action);

    /// <summary>
    /// Cancels the token and releases everything tracked. Called by the container when the screen's
    /// scope ends; there is no need to call it from a view.
    /// </summary>
    public void Dispose()
    {
        _closing.Cancel();

        foreach (var resource in _owned)
        {
            resource.Dispose();
        }

        _owned.Clear();
        _closing.Dispose();
    }
}
