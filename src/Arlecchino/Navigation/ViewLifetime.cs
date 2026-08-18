using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Atoms;

namespace Arlecchino.Navigation;

/// <summary>
/// How long the screen is on. Take it in a view's constructor to tie background work and subscriptions to
/// the screen, and navigating away cancels the token and releases everything registered here.
/// </summary>
public sealed class ViewLifetime : IDisposable
{
    private readonly CancellationTokenSource _closing = new();
    private readonly List<IDisposable> _ownedItems = [];

    private bool _closed;

    /// <summary>Creates the lifetime. Resolved once per screen.</summary>
    public ViewLifetime()
    {
        Closing = _closing.Token;
    }

    /// <summary>
    /// Canceled when the screen goes away, to be passed into work started by the view. It stays readable
    /// afterward, so work coming back late can see the screen has gone.
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
        var loading = new AsyncAtom<T>(initial);

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
        if (_closed)
        {
            resource.Dispose();
            return resource;
        }

        _ownedItems.Add(resource);
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
        if (_closed)
        {
            return;
        }

        _closed = true;
        _closing.Cancel();

        var ownedItems = _ownedItems.ToArray();
        _ownedItems.Clear();

        foreach (var resource in ownedItems)
        {
            resource.Dispose();
        }

        _closing.Dispose();
    }
}
