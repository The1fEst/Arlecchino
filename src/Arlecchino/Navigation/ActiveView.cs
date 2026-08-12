using System;
using Microsoft.Extensions.DependencyInjection;

namespace Arlecchino.Navigation;

/// <summary>
/// A screen together with the container scope it was built from. Navigating away disposes the screen and
/// then the scope, so whatever it took from the container goes with it.
/// </summary>
public sealed class ActiveView : IDisposable
{
    private readonly IServiceScope _scope;

    /// <summary>Pairs a screen with its scope.</summary>
    /// <param name="view">The screen.</param>
    /// <param name="scope">The scope it was built from.</param>
    public ActiveView(IArlecchinoView view, IServiceScope scope)
    {
        View = view;
        _scope = scope;
    }

    /// <summary>The screen being shown.</summary>
    public IArlecchinoView View { get; }

    /// <summary>Disposes the screen if it asked to be, then the scope behind it.</summary>
    public void Dispose()
    {
        if (View is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _scope.Dispose();
    }
}
