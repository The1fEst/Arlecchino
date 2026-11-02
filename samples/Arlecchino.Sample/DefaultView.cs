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

public sealed class DefaultView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly ArlecchinoState _state;
    private readonly CommandRegistry _commands;
    private readonly ViewCommand[] _own;

    private int _selectedIndex;

    public DefaultView(Surface surface, ArlecchinoState state, CommandRegistry commands)
    {
        _surface = surface;
        _state = state;
        _commands = commands;

        _own =
        [
            Bind.To(new(ConsoleKey.N), LocString.TextCommand, AskName),
            Bind.To(new(ConsoleKey.P), LocString.PasswordCommand, AskPassphrase),
            Bind.To(new(ConsoleKey.E), LocString.EmailCommand, AskEmail),
            Bind.To(new(ConsoleKey.U), LocString.UrlCommand, AskHomepage),
            Bind.To(new(ConsoleKey.M), LocString.NumberCommand, AskPrice),
            Bind.To(new(ConsoleKey.R), LocString.SliderCommand, AskVolume),
            Bind.To(new(ConsoleKey.T), LocString.ToggleCommand, AskFullscreen),
            Bind.To(new(ConsoleKey.C), LocString.MultiChoiceCommand, AskColumns),
            Bind.To(new(ConsoleKey.D), LocString.DateCommand, AskReleaseDate),
            Bind.To(new(ConsoleKey.I), LocString.TimeCommand, AskStartTime),
            Bind.To(new(ConsoleKey.K), LocString.ColorCommand, AskAccent),
            Bind.Going(new(ConsoleKey.S), LocString.SettingsCommand, static () => ViewKind.Settings),
            Bind.Going(new(ConsoleKey.W), LocString.WidgetsCommand, static () => ViewKind.Widgets),
            Bind.Going(new(ConsoleKey.L), LocString.PanesCommand, static () => ViewKind.Panes),
            Bind.Going(new(ConsoleKey.G), LocString.ChartsCommand, static () => ViewKind.Charts),
            Bind.Going(new(ConsoleKey.J), LocString.PicturesCommand, static () => ViewKind.Pictures),
        ];
    }

    public void Draw()
    {
        _surface.SkipLine();
        _surface.AppendLine("A R L E C C H I N O", Theme.Header, Align.Center);
        _surface.AppendLine(Loc(LocString.Tagline), Theme.Secondary, Align.Center, new(0, 0, 0, 2));

        for (var i = 0; i < _commands.Commands.Count; i++)
        {
            var command = _commands.Commands[i];
            var style = i == _selectedIndex ? Theme.Selection : Theme.Info;
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
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                return ViewRoute.None;
            case ConsoleKey.DownArrow:
                _selectedIndex = Math.Min(_commands.Commands.Count - 1, _selectedIndex + 1);
                return ViewRoute.None;
            case ConsoleKey.Enter:
                return _commands.Commands[_selectedIndex].Execute();
            default:
                return ViewRoute.None;
        }
    }

    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                return ViewRoute.None;
            case MouseAction.ScrolledDown:
                _selectedIndex = Math.Min(_commands.Commands.Count - 1, _selectedIndex + 1);
                return ViewRoute.None;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left:
                return _commands.Commands[_selectedIndex].Execute();
            default:
                return ViewRoute.None;
        }
    }

    private void AskName() =>
        _state.RequestText(Loc(LocString.NameTitle),
            "",
            static text => text.Length == 0 ? Loc(LocString.MustNotBeEmpty) : null,
            text => _state.Output = Loc(LocString.Greeting, text));

    private void AskPassphrase() =>
        _state.RequestPassword(Loc(LocString.Passphrase),
            value => _state.Output = Loc(LocString.PassphraseLength, value.Length));

    private void AskEmail() =>
        _state.RequestEmail(Loc(LocString.Email), "", value => _state.Output = Loc(LocString.EmailOutput, value));

    private void AskHomepage() =>
        _state.RequestUrl(Loc(LocString.Homepage),
            "https://",
            value => _state.Output = Loc(LocString.UrlOutput, value));

    private void AskPrice() => _state.Modal = new NumberModal
    {
        Title = Loc(LocString.Price),
        Text = "10",
        Minimum = 0,
        Maximum = 500,
        Step = 5,
        LargeStep = 50,
        Decimals = 2,
        Prefix = "$ ",
        OnSubmit = value => _state.Output = Loc(LocString.PriceOutput, value),
    };

    private void AskVolume() => _state.Modal = new SliderModal
    {
        Title = Loc(LocString.Volume),
        Value = 60,
        Step = 5,
        Suffix = " %",
        OnSubmit = value => _state.Output = Loc(LocString.VolumeOutput, value),
    };

    private void AskFullscreen() =>
        _state.RequestToggle(Loc(LocString.Fullscreen),
            true,
            value => _state.Output = Loc(LocString.FullscreenOutput,
                Loc(value ? LocString.AnswerYes : LocString.AnswerNo)));

    private void AskColumns() =>
        _state.RequestMultiChoice(
            Loc(LocString.Columns),
            ["Name", "Date Modified", "Size", "Kind"],
            ["Name", "Size"],
            picked => _state.Output = Loc(LocString.ColumnsOutput, string.Join(", ", picked)));

    private void AskReleaseDate() =>
        _state.RequestDate(Loc(LocString.ReleaseDate),
            DateOnly.FromDateTime(DateTime.Today),
            value => _state.Output = Loc(LocString.DateOutput, value));

    private void AskStartTime() =>
        _state.RequestTime(Loc(LocString.StartAt), new(9, 41), value => _state.Output = Loc(LocString.TimeOutput, value));

    private void AskAccent() =>
        _state.RequestColor(Loc(LocString.AccentColor),
            new(63, 169, 245),
            value => _state.Output = Loc(LocString.ColorOutput, value.Hex));
}
