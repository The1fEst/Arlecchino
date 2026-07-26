using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Input;

namespace Arlecchino.Navigation;

/// <summary>
/// A screen. Constructor parameters come from the container, and the instance lives as long as the
/// route is shown — navigating away and back builds a new one, so per-screen state can live in
/// fields. Implement <see cref="IDisposable"/> to be told when the screen goes away.
/// </summary>
public interface IArlecchinoView
{
    /// <summary>Draws the screen. Called once per frame against the shared surface.</summary>
    void Draw();

    /// <summary>
    /// Handles a key the router did not claim: typing, arrows, list filters. Keys that belong to a
    /// command should be declared in <see cref="Commands"/> instead.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>A route to navigate to, or <see cref="ViewRoute.None"/> to stay.</returns>
    ViewRoute Handle(ConsoleKeyInfo key);

    /// <summary>
    /// Handles a mouse event in frame coordinates. Only views that care about the mouse implement
    /// this.
    /// </summary>
    /// <param name="mouse">The event, with coordinates as frame cells.</param>
    /// <returns>A route to navigate to, or <see cref="ViewRoute.None"/> to stay.</returns>
    ViewRoute HandleMouse(MouseEvent mouse) => ViewRoute.None;

    /// <summary>
    /// Handles text pasted into the terminal. It arrives as one block however long it is, so a screen
    /// that takes typed input should take this too rather than leaving pastes on the floor.
    /// </summary>
    /// <param name="text">What was pasted, with the terminal's markers already stripped.</param>
    /// <returns>A route to navigate to, or <see cref="ViewRoute.None"/> to stay.</returns>
    ViewRoute HandlePaste(string text) => ViewRoute.None;

    /// <summary>
    /// Keys this screen reacts to, declared as data. They are checked before <see cref="Handle"/>,
    /// listed in the command palette, and checked for conflicts with application commands.
    /// </summary>
    /// <returns>The commands of this screen.</returns>
    IReadOnlyList<ViewCommand> Commands() => [];

    /// <summary>
    /// What the hints box shows. Leave it empty and the box is built from <see cref="Commands"/>,
    /// so a rebound key relabels itself.
    /// </summary>
    /// <returns>Pairs of key and description.</returns>
    (string Key, string Description)[] Hints() => [];
}
