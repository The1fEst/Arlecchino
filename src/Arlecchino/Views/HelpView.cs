using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Widgets;

namespace Arlecchino.Views;

/// <summary>
/// Every key at once: what the framework answers to everywhere, and the commands the application
/// registered. The hints box has room for a handful of keys and the palette lists commands only, so
/// this is the screen to send someone to when they ask what a key does.
/// </summary>
internal class HelpView : IArlecchinoView
{
    /// <summary>The route it answers to.</summary>
    public const string Route = "Help";

    private const int KeyColumn = 18;

    private readonly Surface _surface;
    private readonly Navigator _navigator;
    private readonly CommandRegistry _commands;
    private readonly ArlecchinoKeymap _keymap;
    private readonly ArlecchinoStrings _strings;
    private readonly ListBox<Row> _rows;

    public HelpView(Surface surface, Navigator navigator, CommandRegistry commands, ArlecchinoOptions options)
    {
        _surface = surface;
        _navigator = navigator;
        _commands = commands;
        _keymap = options.Keymap;
        _strings = options.Strings;

        _rows = new(options.Keymap)
        {
            Render = static row => row.Text,
            ItemStyle = static row => row.IsHeading ? Theme.TableHeader : Theme.Default,
        };
    }

    private sealed record Row(string Text, bool IsHeading);

    public void Draw()
    {
        var content = _surface.Content;
        var (header, rest) = content.SplitTop(2);

        header.WriteLine(0, _strings.HelpTitle(), Theme.Header);
        header.WriteLine(1, $"{_keymap.Cancel} {_strings.HelpClose()}", Theme.Muted);

        _rows.Items = Build();
        _rows.Draw(rest);
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key) || _keymap.Help.Matches(key))
        {
            _navigator.Back();
            return ViewRoute.None;
        }

        return _rows.Handle(key).Route;
    }

    public ViewRoute HandleMouse(MouseEvent mouse) => _rows.HandleMouse(mouse).Route;

    public (string Key, string Description)[] Hints() =>
    [
        ($"{_keymap.MoveUp}{_keymap.MoveDown}", _strings.FormMove()),
        (_keymap.Cancel.ToString(), _strings.HelpClose()),
    ];

    private List<Row> Build()
    {
        var rows = new List<Row> { new(_strings.HelpFrameworkSection(), true) };

        foreach (var (binding, action) in _strings.HelpKeys(_keymap))
        {
            rows.Add(new(Line(binding.ToString(), action), false));
        }

        rows.Add(new("", false));
        rows.Add(new(_strings.HelpCommandsSection(), true));

        if (_commands.Commands.Count == 0)
        {
            rows.Add(new(Line("", _strings.HelpNoCommands()), false));
            return rows;
        }

        foreach (var command in _commands.Commands)
        {
            rows.Add(new(Line(command.Binding.ToString(), $"{command.Icon} {command.Label}".TrimStart()), false));
        }

        return rows;
    }

    private static string Line(string key, string action) =>
        $" {TextWidth.PadRight(key, KeyColumn)}{action}";
}
