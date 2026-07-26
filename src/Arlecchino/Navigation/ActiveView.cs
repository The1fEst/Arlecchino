using System;
using Microsoft.Extensions.DependencyInjection;

namespace Arlecchino.Navigation;

/// <summary>
/// A screen together with the container scope it was built from. Navigating away disposes both, in
/// that order, so anything the screen took from the container — a database context, a connection, a
/// file handle — goes away with the screen rather than living as long as the application.
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
