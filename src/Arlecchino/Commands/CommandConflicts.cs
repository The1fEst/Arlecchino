using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Arlecchino.Diagnostics;
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
    private readonly HashSet<string> _reportedRoutes = [];

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
        if (commands.Count == 0 || !_reportedRoutes.Add(route.Name))
        {
            return;
        }

        var bindings = new Dictionary<KeyBinding, ViewCommand>();

        foreach (var command in commands)
        {
            if (bindings.TryGetValue(command.Binding, out var earlier))
            {
                Log.KeyBoundTwice(_logger, route, command.Binding, earlier.Label(), command.Label());
                continue;
            }

            bindings[command.Binding] = command;

            foreach (var global in _global.Commands)
            {
                if (global.Binding != command.Binding)
                {
                    continue;
                }

                Log.KeyShadowsCommand(_logger, route, command.Binding, command.Label(), global.Label);
            }
        }
    }
}
