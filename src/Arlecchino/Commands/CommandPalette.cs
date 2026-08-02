using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Modals.Choosing;
using Arlecchino.Navigation;
using Arlecchino.State;

namespace Arlecchino.Commands;

/// <summary>
/// The list of everything a key would do, opened by one key of its own.
///
/// It is the only dialog the framework opens without being asked to, and the only one whose keys are
/// every other key: pressing one runs what it is bound to, whether that is the view's or the
/// application's, and pressing anything unbound says so rather than doing nothing.
/// </summary>
internal sealed class CommandPalette
{
    private readonly ArlecchinoState _state;
    private readonly Navigator _navigator;
    private readonly CommandRegistry _commands;
    private readonly ArlecchinoOptions _options;
    private readonly KeyText _keyText;

    /// <summary>Opens and reads the palette.</summary>
    /// <param name="state">Where the dialog on top lives.</param>
    /// <param name="navigator">Supplies the current view's commands and applies routes.</param>
    /// <param name="commands">Commands available everywhere.</param>
    /// <param name="options">Settings gathered at startup, for the key that opens it and the words.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    public CommandPalette(
        ArlecchinoState state,
        Navigator navigator,
        CommandRegistry commands,
        ArlecchinoOptions options,
        KeyText keyText)
    {
        _state = state;
        _navigator = navigator;
        _commands = commands;
        _options = options;
        _keyText = keyText;
    }

    /// <summary>Whether a key is the one that opens it, which it is not when there is nothing to list.</summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the palette should open.</returns>
    public bool Opens(ConsoleKeyInfo key) =>
        _commands.Commands.Count > 0 && _keyText.Resolve(key) == _options.CommandPaletteKey;

    /// <summary>Opens it.</summary>
    public void Open()
    {
        _state.Output = string.Empty;
        _state.Modal = new CommandModal
        {
            Title = _options.Strings.CommandPaletteTitle(),
            Commands = Entries(),
            OnKey = Handle,
            OnRow = Run,
        };
    }

    /// <summary>
    /// Reads a key while it is open. Whatever the key is the palette closes first: it is a reminder of
    /// what the keys are, and a reminder that stays up after it has been acted on is in the way.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    public void Handle(ConsoleKeyInfo key)
    {
        _state.CloseModal();

        if (_options.Keymap.Cancel.Matches(key) || _options.Keymap.Confirm.Matches(key))
        {
            return;
        }

        foreach (var viewCommand in _navigator.CurrentCommands)
        {
            if (!viewCommand.Binding.Matches(key))
            {
                continue;
            }

            if (viewCommand.IsEnabled())
            {
                _navigator.Apply(viewCommand.Run());
            }

            return;
        }

        if (_commands.TryFind(key, out var command))
        {
            _navigator.Apply(command.Execute());

            return;
        }

        _state.Output = _options.Strings.CommandUnknown(Shown(key));
    }

    /// <summary>Runs whichever row was clicked.</summary>
    /// <param name="row">Which row.</param>
    private void Run(int row)
    {
        var viewCommands = _navigator.CurrentCommands;

        if (row < 0 || row >= viewCommands.Count + _commands.Commands.Count)
        {
            return;
        }

        _state.CloseModal();

        if (row >= viewCommands.Count)
        {
            _navigator.Apply(_commands.Commands[row - viewCommands.Count].Execute());

            return;
        }

        var viewCommand = viewCommands[row];

        if (viewCommand.IsEnabled())
        {
            _navigator.Apply(viewCommand.Run());
        }
    }

    /// <summary>What goes in it: the view's commands first, then the ones available everywhere.</summary>
    /// <returns>The rows.</returns>
    private (string Key, string Label)[] Entries()
    {
        var entries = new List<(string Key, string Label)>();

        foreach (var command in _navigator.CurrentCommands)
        {
            entries.Add((command.Binding.ToString(), command.Label()));
        }

        foreach (var command in _commands.Commands)
        {
            entries.Add((command.Binding.ToString(), command.Label));
        }

        return [.. entries];
    }

    /// <summary>How to name a key that turned out to be bound to nothing.</summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns>The character it typed, or the binding it would have been.</returns>
    private static string Shown(ConsoleKeyInfo key) => char.IsControl(key.KeyChar) || key.KeyChar == '\0'
        ? new KeyBinding(key.Key, key.Modifiers).ToString()
        : key.KeyChar.ToString();
}
