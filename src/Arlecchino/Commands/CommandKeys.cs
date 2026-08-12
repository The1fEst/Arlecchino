using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Commands;

/// <summary>The keys that reach commands, and the half-typed chord in between two of them.</summary>
public sealed class CommandKeys
{
    private readonly Navigator _navigator;
    private readonly CommandRegistry _commands;

    private KeyPress? _leader;

    /// <summary>Creates the lookup.</summary>
    /// <param name="navigator">Supplies the commands of the screen being shown.</param>
    /// <param name="commands">Commands available everywhere.</param>
    internal CommandKeys(Navigator navigator, CommandRegistry commands)
    {
        _navigator = navigator;
        _commands = commands;
    }

    /// <summary>Whether a chord has been started, so the next key belongs to it and nothing else.</summary>
    public bool IsWaiting => _leader is not null;

    /// <summary>
    /// Runs what the key is bound to, the view's own commands first and the ones available everywhere
    /// after them. A command that says it is unavailable is skipped rather than swallowing its key.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when something was bound to it and willing to run.</returns>
    internal bool Ran(KeyPress key)
    {
        foreach (var command in _navigator.CurrentCommands)
        {
            if (!command.Binding.Matches(key) || !command.IsEnabled())
            {
                continue;
            }

            _navigator.Apply(command.Run());

            return true;
        }

        if (key.Modifiers == default || !_commands.TryFind(key, out var application))
        {
            return Opened(key);
        }

        _navigator.Apply(application.Execute());

        return true;
    }

    /// <summary>
    /// Finishes the chord that was started. The key is taken even when it lands on nothing, since
    /// halfway through a chord it was meant for the chord.
    /// </summary>
    /// <param name="key">The key that arrived after the leader.</param>
    internal void Finish(KeyPress key)
    {
        if (_leader is not { } leader)
        {
            return;
        }

        _leader = null;

        foreach (var command in _navigator.CurrentCommands)
        {
            if (!command.Binding.Opens(leader) || !command.Binding.Closes(key) || !command.IsEnabled())
            {
                continue;
            }

            _navigator.Apply(command.Run());

            return;
        }

        foreach (var command in _commands.Commands)
        {
            if (!command.Binding.Opens(leader) || !command.Binding.Closes(key))
            {
                continue;
            }

            _navigator.Apply(command.Execute());

            return;
        }
    }

    /// <summary>
    /// Every key that would finish the chord being typed, under the name of that key alone. This is what
    /// a leader is grouped for: the second key is looked up rather than remembered.
    /// </summary>
    /// <returns>The keys behind the leader, or nothing when no chord is waiting.</returns>
    public (string Key, string Description)[] Hints()
    {
        if (_leader is not { } leader)
        {
            return [];
        }

        var hints = new List<(string Key, string Description)>();

        foreach (var command in _navigator.CurrentCommands)
        {
            if (command.Binding.Opens(leader) && command.IsEnabled())
            {
                hints.Add((Finishing(command.Binding), command.Label()));
            }
        }

        foreach (var command in _commands.Commands)
        {
            if (command.Binding.Opens(leader))
            {
                hints.Add((Finishing(command.Binding), command.Label));
            }
        }

        return [.. hints];
    }

    private bool Opened(KeyPress key)
    {
        foreach (var command in _navigator.CurrentCommands)
        {
            if (!command.Binding.Opens(key) || !command.IsEnabled())
            {
                continue;
            }

            _leader = key;

            return true;
        }

        foreach (var command in _commands.Commands)
        {
            if (!command.Binding.Opens(key))
            {
                continue;
            }

            _leader = key;

            return true;
        }

        return false;
    }

    private static string Finishing(KeyBinding binding) => binding.Second?.ToString() ?? "";
}
