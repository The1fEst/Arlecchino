using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Commands;

/// <summary>
/// Reports keys that two commands both claim. Checked once per route as it is first shown, because
/// a screen command silently shadowing an application command is a bug that is otherwise invisible.
/// </summary>
internal sealed class CommandConflicts
{
    private readonly CommandRegistry _global;
    private readonly ILogger<CommandConflicts> _logger;
    private readonly HashSet<string> _reported = [];

    /// <summary>Creates the check.</summary>
    /// <param name="global">Application commands to compare against.</param>
    /// <param name="logger">Where warnings go.</param>
    public CommandConflicts(CommandRegistry global, ILogger<CommandConflicts> logger)
    {
        _global = global;
        _logger = logger;
    }

    /// <summary>
    /// Warns about keys claimed twice by a screen, and about screen commands that hide an
    /// application command. Each route is reported once.
    /// </summary>
    /// <param name="route">The route being shown.</param>
    /// <param name="commands">Commands the screen declared.</param>
    public void Report(ViewRoute route, IReadOnlyList<ViewCommand> commands)
    {
        if (commands.Count == 0 || !_reported.Add(route.Name))
        {
            return;
        }

        var seen = new Dictionary<KeyBinding, ViewCommand>();

        foreach (var command in commands)
        {
            if (seen.TryGetValue(command.Binding, out var earlier))
            {
                _logger.LogWarning(
                    "{Route} binds {Binding} to both '{Kept}' and '{Shadowed}'; only the first one can run.",
                    route, command.Binding, earlier.Label(), command.Label());
                continue;
            }

            seen[command.Binding] = command;

            foreach (var global in _global.Commands)
            {
                if (global.Binding != command.Binding)
                {
                    continue;
                }

                _logger.LogWarning(
                    "{Route} binds {Binding} to '{View}', shadowing the application command '{Global}'.",
                    route, command.Binding, command.Label(), global.Label);
            }
        }
    }
}
