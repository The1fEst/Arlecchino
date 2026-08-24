using System;
using System.Linq;
using Arlecchino.Forms;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Sample.Views;
using Arlecchino.State;
using Arlecchino.Atoms.History;

namespace Arlecchino.Sample;

public sealed class SettingsView : IArlecchinoView, IDisposable
{
    private readonly Surface _surface;
    private readonly SettingsStore _settings;
    private readonly AtomHistory _history;
    private readonly ArlecchinoState _state;
    private readonly Form _form;
    private readonly IDisposable _watch;
    private readonly IDisposable _complete;

    public SettingsView(Surface surface, SettingsStore settings, AtomHistory history, ArlecchinoState state, ArlecchinoOptions options)
    {
        _surface = surface;
        _settings = settings;
        _history = history;
        _state = state;

        var apply = Fields.Action(static () => "Apply", Apply)
            .WithHelp(static () => "enabled once profile and theme are set");

        _form = new(state, options)
        {
            Fields =
            [
                Fields.Text(static () => "Profile", settings.Profile)
                    .WithValidation(static text => !string.IsNullOrWhiteSpace(text))
                    .WithHelp(static () => "shown in the title bar"),
                Fields.Secret(static () => "Passphrase", settings.Passphrase)
                    .WithHelp(static () => "never rendered in clear text"),
                Fields.Choice(static () => "Theme", ["dark", "light", "high contrast"], settings.Theme)
                    .WithHelp(static () => "color palette of the application"),
                Fields.Slider(static () => "Volume", settings.Volume)
                    .WithItems(static () => [.. Enumerable.Range(0, 101)])
                    .WithHelp(static () => "←→ inside the modal, or type a number"),
                Fields.Toggle(static () => "Fullscreen", settings.Fullscreen),
                Fields.MultiChoice(static () => "Columns", ["Name", "Date Modified", "Size", "Kind"], settings.Columns),
                Fields.Date(static () => "Release", settings.Release),
                Fields.Color(static () => "Accent", settings.Accent),
                Fields.Path(static () => "Folder", settings.Profile, ViewKind.Settings, pickFolder: true)
                    .WithHelp(static () => "opens the file picker"),
                apply,
            ],
        };

        _watch = settings.Summary.Subscribe(() => _state.Output = $"settings: {settings.Summary.Value}");
        _complete = settings.IsComplete.Subscribe(() => apply.IsEnabled = settings.IsComplete.Value);
        apply.IsEnabled = settings.IsComplete.Value;
    }

    public void Draw()
    {
        var content = _surface.Content;
        content.WriteLine(0, "Settings", Theme.Header);
        content.WriteLine(1, _settings.Summary.Value, Theme.Secondary);

        _form.Draw(content.Rows(3, content.Height - 3));
    }

    public ViewRoute Handle(KeyPress key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Z when key.Modifiers.HasFlag(KeyModifiers.Control):
                _state.Output = _history.Undo() ? "undo" : "nothing to undo";
                return ViewRoute.None;
            case ConsoleKey.Y when key.Modifiers.HasFlag(KeyModifiers.Control):
                _state.Output = _history.Redo() ? "redo" : "nothing to redo";
                return ViewRoute.None;
            case ConsoleKey.Escape:
                return ViewKind.Default;
            default:
                return _form.Handle(key).Route;
        }
    }

    public ViewRoute HandleMouse(MouseEvent mouse) => _form.HandleMouse(mouse).Route;

    public (string Key, string Description)[] Hints() =>
    [
        .. _form.Hints(),
        ("Ctrl+Z", "undo"),
        ("Ctrl+Y", "redo"),
        ("Esc", "back"),
    ];

    public void Dispose()
    {
        _watch.Dispose();
        _complete.Dispose();
    }

    private static ViewRoute Apply() => ViewKind.Default;
}
