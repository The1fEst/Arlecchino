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
internal sealed class HelpView : IArlecchinoView
{
    /// <summary>The route it answers to.</summary>
    public const string Route = "Help";

    private const int KeyColumn = 18;
    private const int ColumnFloor = 34;
    private const int Gutter = 2;

    private readonly Surface _surface;
    private readonly Navigator _navigator;
    private readonly CommandRegistry _commands;
    private readonly ArlecchinoKeymap _keymap;
    private readonly ArlecchinoStrings _strings;
    private readonly ScrollPane _pane;
    private readonly List<Row> _everywhere = [];
    private readonly List<Row> _screen = [];
    private readonly List<Row> _registered = [];

    private int _column;
    private bool _doubled;

    public HelpView(Surface surface, Navigator navigator, CommandRegistry commands, ArlecchinoOptions options)
    {
        _surface = surface;
        _navigator = navigator;
        _commands = commands;
        _keymap = options.Keymap;
        _strings = options.Strings;

        _pane = new(options.Keymap)
        {
            IsFocused = true,
            ContentHeight = Height,
            Content = Paint,
        };
    }

    private sealed record Row(string Text, bool IsHeading);

    /// <summary>
    /// Draws the screen. The keys that work everywhere and the keys of the screen this was opened
    /// from stand side by side when there is width for it, with the application's commands in a band
    /// underneath — two lists that are read against each other, not one list to be scrolled through.
    /// </summary>
    public void Draw()
    {
        var content = _surface.Content;
        var (header, rest) = content.SplitTop(2);

        header.WriteLine(0, _strings.HelpTitle(), Theme.Header);
        header.WriteLine(1, $"{_keymap.Cancel} {_strings.HelpClose()}", Theme.Muted);

        Build();

        _doubled = _screen.Count > 0 && rest.Width >= (ColumnFloor * 2) + Gutter;
        _column = _doubled ? (rest.Width - Gutter) / 2 : rest.Width;

        _pane.Draw(rest);
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        if (!_keymap.Cancel.Matches(key) && !_keymap.Help.Matches(key))
        {
            return _pane.Handle(key).Route;
        }

        _navigator.Back();
        return ViewRoute.None;
    }

    public ViewRoute HandleMouse(MouseEvent mouse) => _pane.HandleMouse(mouse).Route;

    public (string Key, string Description)[] Hints() =>
    [
        ($"{_keymap.MoveUp}{_keymap.MoveDown}", _strings.FormMove()),
        (_keymap.Cancel.ToString(), _strings.HelpClose()),
    ];

    private int Height() => Above() + 1 + _registered.Count;

    private int Above() => _doubled
        ? Math.Max(_everywhere.Count, _screen.Count)
        : _everywhere.Count + (_screen.Count == 0 ? 0 : _screen.Count + 1);

    private void Paint(SurfaceRegion region)
    {
        var above = Above();

        if (_doubled)
        {
            Write(region, _everywhere, 0, 0);
            Write(region, _screen, 0, _column + Gutter);
        }
        else
        {
            Write(region, _everywhere, 0, 0);
            Write(region, _screen, _everywhere.Count + 1, 0);
        }

        region.WriteLine(above, new('─', region.Width), Theme.Muted);
        Write(region, _registered, above + 1, 0);
    }

    private void Write(SurfaceRegion region, List<Row> rows, int top, int column)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            region.Write(
                top + index,
                column,
                TextWidth.Truncate(rows[index].Text, _column),
                rows[index].IsHeading ? Theme.TableHeader : Theme.Default);
        }
    }

    private void Build()
    {
        _everywhere.Clear();
        _screen.Clear();
        _registered.Clear();

        _everywhere.Add(new(_strings.HelpFrameworkSection(), true));

        foreach (var (binding, action) in _strings.HelpKeys(_keymap))
        {
            _everywhere.Add(new(Line(binding.ToString(), action), false));
        }

        if (_navigator.PreviousCommands.Count > 0)
        {
            _screen.Add(new(_strings.HelpScreenSection(_navigator.PreviousRoute.Name), true));

            foreach (var command in _navigator.PreviousCommands)
            {
                _screen.Add(new(Line(command.Binding.ToString(), command.Label()), false));
            }
        }

        _registered.Add(new(_strings.HelpCommandsSection(), true));

        if (_commands.Commands.Count == 0)
        {
            _registered.Add(new(Line("", _strings.HelpNoCommands()), false));
            return;
        }

        foreach (var command in _commands.Commands)
        {
            _registered.Add(new(Line(command.Binding.ToString(), $"{command.Icon} {command.Label}".TrimStart()), false));
        }
    }

    private static string Line(string key, string action) =>
        $" {TextWidth.PadRight(key, KeyColumn)}{action}";
}
