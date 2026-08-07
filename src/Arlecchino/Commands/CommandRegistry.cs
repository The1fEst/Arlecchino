using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Commands;

/// <summary>
/// The application commands registered with <c>AddCommand</c>. Take it in a view to list or run them
/// yourself — the sample draws its menu straight from this.
/// </summary>
public class CommandRegistry
{
    /// <summary>Collects the registered commands.</summary>
    /// <param name="commands">Commands from the container, in registration order.</param>
    public CommandRegistry(IEnumerable<IArlecchinoCommand> commands)
    {
        Commands = commands.ToArray();
    }

    /// <summary>The registered commands, in registration order.</summary>
    public IReadOnlyList<IArlecchinoCommand> Commands { get; }

    /// <summary>Finds the command a key press belongs to.</summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <param name="command">The command, when one claims the key.</param>
    /// <returns><c>true</c> when a command claimed the key.</returns>
    public bool TryFind(KeyPress pressed, [NotNullWhen(true)] out IArlecchinoCommand? command)
    {
        foreach (var candidate in Commands)
        {
            if (!candidate.Binding.Matches(pressed))
            {
                continue;
            }

            command = candidate;
            return true;
        }

        command = null;
        return false;
    }

    /// <summary>Runs the command a key belongs to, if any.</summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <returns>The route the command returned, or <see cref="ViewRoute.None"/>.</returns>
    public ViewRoute Send(KeyPress pressed) =>
        TryFind(pressed, out var command) ? command.Execute() : ViewRoute.None;
}
