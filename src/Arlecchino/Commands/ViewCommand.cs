using System;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Commands;

/// <summary>
/// A key a screen reacts to, declared as data rather than hidden in a switch. That is what lets the
/// palette list it, the hints box label it, and the conflict check see it.
/// </summary>
public sealed class ViewCommand
{
    /// <summary>Key that runs the command. Checked before the screen's own key handling.</summary>
    public required KeyBinding Binding { get; init; }

    /// <summary>Name shown in the palette and the hints box; a delegate, so it can be translated.</summary>
    public required Func<string> Label { get; init; }

    /// <summary>What the command does.</summary>
    public required Func<ViewRoute> Run { get; init; }

    /// <summary>
    /// Whether the command can run now. A disabled command still swallows its key rather than
    /// letting it fall through — the key is spoken for either way.
    /// </summary>
    public Func<bool> IsEnabled { get; init; } = static () => true;

    /// <summary>A command that does something and stays on the screen.</summary>
    /// <param name="binding">Key that runs it.</param>
    /// <param name="label">Name shown to the user.</param>
    /// <param name="run">What to do.</param>
    /// <returns>The command.</returns>
    public static ViewCommand For(KeyBinding binding, Func<string> label, Action run) => new()
    {
        Binding = binding,
        Label = label,
        Run = () =>
        {
            run();
            return ViewRoute.None;
        },
    };

    /// <summary>A command on a plain key that does something and stays on the screen.</summary>
    /// <param name="key">Key that runs it.</param>
    /// <param name="label">Name shown to the user.</param>
    /// <param name="run">What to do.</param>
    /// <returns>The command.</returns>
    public static ViewCommand For(ConsoleKey key, Func<string> label, Action run) =>
        For(new KeyBinding(key), label, run);

    /// <summary>A command on a plain key that navigates somewhere.</summary>
    /// <param name="key">Key that runs it.</param>
    /// <param name="label">Name shown to the user.</param>
    /// <param name="run">What to do; the route it returns is applied.</param>
    /// <returns>The command.</returns>
    public static ViewCommand Navigating(ConsoleKey key, Func<string> label, Func<ViewRoute> run) => new()
    {
        Binding = new(key),
        Label = label,
        Run = run,
    };
}
