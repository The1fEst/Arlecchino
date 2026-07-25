using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Commands;

/// <summary>
/// An application-wide command. It appears in the command palette from every screen, and fires
/// globally when its binding carries a modifier — a plain letter would swallow typing, so those are
/// only reachable through the palette.
/// </summary>
public interface IArlecchinoCommand
{
    /// <summary>Key that runs the command.</summary>
    KeyBinding Binding { get; }

    /// <summary>Short marker to draw beside the label; yours to use or ignore.</summary>
    string Icon { get; }

    /// <summary>Name shown in the palette.</summary>
    string Label { get; }

    /// <summary>Runs the command.</summary>
    /// <returns>A route to navigate to, or <see cref="ViewRoute.None"/> to stay.</returns>
    ViewRoute Execute();
}
