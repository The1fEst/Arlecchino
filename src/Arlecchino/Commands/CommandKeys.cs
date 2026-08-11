using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Commands;

/// <summary>
/// The keys that reach commands, and the half-typed chord in between two of them. Kept in one place
/// because the screen has to draw what a leader has behind it while the router waits for the key that
/// finishes it, and both are asking the same question of the same commands.
/// </summary>
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
    /// The view's own commands first, then the ones available everywhere. A command available
    /// everywhere is only reached with a modifier held, so an unmodified letter always belongs to the
    /// view.
    ///
    /// A command that says it is not available takes nothing. It used to swallow its key anyway, which made
    /// <c>IsEnabled</c> mean two different things: grayed out on the key screen, and a key that silently does
    /// nothing. It also left a view unable to give the same key a second meaning for the times its command is
    /// off. Skipped here, the key carries on to the commands available everywhere and then to the view, exactly
    /// as if nothing had claimed it.
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
    /// Finishes the chord that was started. The key is taken even when it lands on nothing: a person
    /// halfway through a chord meant the chord, and letting a stray second key act on its own would run
    /// whatever it is bound to instead.
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
    /// What the hints box shows while a chord is half typed: every key that finishes it, under the name
    /// of the key alone. This is the whole point of grouping commands behind a leader — the second key
    /// is looked up rather than remembered.
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
