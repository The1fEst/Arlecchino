using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;

namespace Arlecchino.Navigation;

/// <summary>
/// Holds the screen being shown and the history behind it. Routes returned from handlers pass
/// through here, and the view that leaves is disposed if it asked to be.
/// </summary>
public class Navigator
{
    private readonly ViewResolver _resolver;
    private readonly Repaint _repaint;
    private readonly CommandConflicts _conflicts;
    private readonly Stack<ViewRoute> _back = new();
    private readonly Stack<ViewRoute> _forward = new();

    private ActiveView? _active;
    private ViewRoute _currentRoute;

    /// <summary>Creates the navigator and shows the configured start route, if there is one.</summary>
    /// <param name="resolver">How routes become views.</param>
    /// <param name="options">Configured options, read for the start route.</param>
    /// <param name="repaint">Signal raised whenever the screen changes.</param>
    /// <param name="conflicts">Checks the commands of each screen as it is shown.</param>
    internal Navigator(ViewResolver resolver, ArlecchinoOptions options, Repaint repaint, CommandConflicts conflicts)
    {
        _resolver = resolver;
        _repaint = repaint;
        _conflicts = conflicts;

        if (!options.StartRoute.IsNone)
        {
            Show(options.StartRoute);
        }
    }

    /// <summary>The route being shown.</summary>
    public ViewRoute CurrentRoute => _currentRoute;

    /// <summary>Commands of the screen being shown, for the router and the palette.</summary>
    public IReadOnlyList<ViewCommand> CurrentCommands => _active?.View.Commands() ?? [];

    internal ViewRoute PreviousRoute => _back.Count > 0 ? _back.Peek() : ViewRoute.None;

    internal IReadOnlyList<ViewCommand> PreviousCommands { get; private set; } = [];

    /// <summary>
    /// What the hints box should show: whatever the screen returned, or its commands when it
    /// returned nothing.
    /// </summary>
    public (string Key, string Description)[] CurrentHints
    {
        get
        {
            if (_active is null)
            {
                return [];
            }

            var hints = _active.View.Hints();
            return hints.Length > 0 ? hints : HintsOf(_active.View.Commands());
        }
    }

    private static (string Key, string Description)[] HintsOf(IReadOnlyList<ViewCommand> commands)
    {
        var hints = new (string Key, string Description)[commands.Count];

        for (var i = 0; i < commands.Count; i++)
        {
            hints[i] = (commands[i].Binding.ToString(), commands[i].Label());
        }

        return hints;
    }

    /// <summary>Whether there is somewhere to go back to.</summary>
    public bool CanGoBack => _back.Count > 0;

    /// <summary>Whether a step back can be retraced.</summary>
    public bool CanGoForward => _forward.Count > 0;

    /// <summary>Draws the current screen. Called once per frame.</summary>
    public void Draw() => _active?.View.Draw();

    /// <summary>Passes a key to the current screen and applies the route it returns.</summary>
    /// <param name="key">The key that was pressed.</param>
    public void Handle(ConsoleKeyInfo key)
    {
        if (_active is null)
        {
            return;
        }

        Apply(_active.View.Handle(key));
    }

    /// <summary>Passes a mouse event to the current screen and applies the route it returns.</summary>
    /// <param name="mouse">The event, in frame coordinates.</param>
    public void HandleMouse(MouseEvent mouse)
    {
        if (_active is not null)
        {
            Apply(_active.View.HandleMouse(mouse));
        }
    }

    /// <summary>Passes pasted text to the current screen and applies the route it returns.</summary>
    /// <param name="text">What was pasted.</param>
    public void HandlePaste(string text)
    {
        if (_active is not null)
        {
            Apply(_active.View.HandlePaste(text));
        }
    }

    /// <summary>
    /// Goes to a route. Ignores <see cref="ViewRoute.None"/> and the route already shown; anything
    /// else pushes the current one onto the back stack and drops the forward stack.
    /// </summary>
    /// <param name="route">Where to go.</param>
    public void Apply(ViewRoute route)
    {
        FrameThread.Verify("Navigator.Apply");

        if (route.IsNone || route == _currentRoute)
        {
            return;
        }

        var leaving = _currentRoute;

        Show(route);

        if (!leaving.IsNone)
        {
            _back.Push(leaving);
        }

        _forward.Clear();
    }

    /// <summary>Builds the current screen again from scratch, losing its per-screen state.</summary>
    public void Reload() => Show(_currentRoute);

    /// <summary>Goes back one step in the history.</summary>
    /// <returns><c>false</c> when there was nothing to go back to.</returns>
    public bool Back()
    {
        if (_back.Count == 0)
        {
            return false;
        }

        var leaving = _currentRoute;

        Show(_back.Peek());

        _back.Pop();
        _forward.Push(leaving);
        return true;
    }

    /// <summary>Retraces a step that was gone back from.</summary>
    /// <returns><c>false</c> when there was nothing to retrace.</returns>
    public bool Forward()
    {
        if (_forward.Count == 0)
        {
            return false;
        }

        var leaving = _currentRoute;

        Show(_forward.Peek());

        _forward.Pop();
        _back.Push(leaving);
        return true;
    }

    private void Show(ViewRoute route)
    {
        var next = _resolver.Create(route);

        if (_active is { } leaving)
        {
            PreviousCommands = leaving.View.Commands();
        }

        _active?.Dispose();

        _currentRoute = route;
        _active = next;
        _conflicts.Report(route, _active.View.Commands());
        _repaint.Request();
    }
}
