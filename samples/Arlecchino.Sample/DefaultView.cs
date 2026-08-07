using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Sample.Views;
using Arlecchino.State;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Setting;

namespace Arlecchino.Sample;

public class DefaultView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly ArlecchinoState _state;
    private readonly CommandRegistry _commands;
    private readonly ViewCommand[] _own;

    private int _selected;

    public DefaultView(Surface surface, ArlecchinoState state, CommandRegistry commands)
    {
        _surface = surface;
        _state = state;
        _commands = commands;

        _own =
        [
            ViewCommand.For(ConsoleKey.N, static () => "text", AskName),
            ViewCommand.For(ConsoleKey.P, static () => "password", AskPassphrase),
            ViewCommand.For(ConsoleKey.E, static () => "email", AskEmail),
            ViewCommand.For(ConsoleKey.U, static () => "url", AskHomepage),
            ViewCommand.For(ConsoleKey.M, static () => "number", AskPrice),
            ViewCommand.For(ConsoleKey.R, static () => "slider", AskVolume),
            ViewCommand.For(ConsoleKey.T, static () => "toggle", AskFullscreen),
            ViewCommand.For(ConsoleKey.C, static () => "multi-choice", AskColumns),
            ViewCommand.For(ConsoleKey.D, static () => "date", AskReleaseDate),
            ViewCommand.For(ConsoleKey.I, static () => "time", AskStartTime),
            ViewCommand.For(ConsoleKey.K, static () => "color", AskAccent),
            ViewCommand.Navigating(ConsoleKey.S, static () => "settings", static () => ViewKind.Settings),
            ViewCommand.Navigating(ConsoleKey.W, static () => "widgets", static () => ViewKind.Widgets),
            ViewCommand.Navigating(ConsoleKey.L, static () => "panes", static () => ViewKind.Panes),
            ViewCommand.Navigating(ConsoleKey.G, static () => "charts", static () => ViewKind.Charts),
            ViewCommand.Navigating(ConsoleKey.J, static () => "pictures", static () => ViewKind.Pictures),
        ];
    }

    public void Draw()
    {
        _surface.SkipLine();
        _surface.AppendLine("A R L E C C H I N O", Theme.Header, Align.Center);
        _surface.AppendLine("a terminal UI framework", Theme.Muted, Align.Center, new(0, 0, 0, 2));

        for (var i = 0; i < _commands.Commands.Count; i++)
        {
            var command = _commands.Commands[i];
            var style = i == _selected ? Theme.Selected : Theme.Info;
            _surface.AppendLine($"{command.Icon}  {command.Label,-24}{command.Binding}",
                style,
                Align.Center,
                new(0, 0, 0, 1));
        }
    }

    public IReadOnlyList<ViewCommand> Commands() => _own;

    public ViewRoute Handle(KeyPress key)
    {
        if (_commands.TryFind(key, out var command))
        {
            return command.Execute();
        }

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                _selected = Math.Max(0, _selected - 1);
                return ViewRoute.None;
            case ConsoleKey.DownArrow:
                _selected = Math.Min(_commands.Commands.Count - 1, _selected + 1);
                return ViewRoute.None;
            case ConsoleKey.Enter:
                return _commands.Commands[_selected].Execute();
            default:
                return ViewRoute.None;
        }
    }

    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                _selected = Math.Max(0, _selected - 1);
                return ViewRoute.None;
            case MouseAction.ScrolledDown:
                _selected = Math.Min(_commands.Commands.Count - 1, _selected + 1);
                return ViewRoute.None;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left:
                return _commands.Commands[_selected].Execute();
            default:
                return ViewRoute.None;
        }
    }

    private void AskName() =>
        _state.RequestText("Your name",
            "",
            static text => text.Length == 0 ? "must not be empty" : null,
            text => _state.Output = $"hello, {text}");

    private void AskPassphrase() =>
        _state.RequestPassword("Passphrase", value => _state.Output = $"passphrase: {value.Length} characters");

    private void AskEmail() => _state.RequestEmail("Email", "", value => _state.Output = $"email: {value}");

    private void AskHomepage() => _state.RequestUrl("Homepage", "https://", value => _state.Output = $"url: {value}");

    private void AskPrice() => _state.Modal = new NumberModal
    {
        Title = "Price",
        Text = "10",
        Minimum = 0,
        Maximum = 500,
        Step = 5,
        LargeStep = 50,
        Decimals = 2,
        Prefix = "$ ",
        OnSubmit = value => _state.Output = $"price: {value}",
    };

    private void AskVolume() => _state.Modal = new SliderModal
    {
        Title = "Volume",
        Value = 60,
        Step = 5,
        Suffix = " %",
        OnSubmit = value => _state.Output = $"volume: {value}",
    };

    private void AskFullscreen() =>
        _state.RequestToggle("Fullscreen", true, value => _state.Output = $"fullscreen: {value}");

    private void AskColumns() =>
        _state.RequestMultiChoice(
            "Columns",
            ["Name", "Date Modified", "Size", "Kind"],
            ["Name", "Size"],
            picked => _state.Output = $"columns: {string.Join(", ", picked)}");

    private void AskReleaseDate() =>
        _state.RequestDate("Release date",
            DateOnly.FromDateTime(DateTime.Today),
            value => _state.Output = $"date: {value:yyyy-MM-dd}");

    private void AskStartTime() =>
        _state.RequestTime("Start at", new(9, 41), value => _state.Output = $"time: {value:HH\\:mm}");

    private void AskAccent() =>
        _state.RequestColor("Accent color", new(63, 169, 245), value => _state.Output = $"color: {value.Hex}");
}
