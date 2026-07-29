using System.Globalization;
using System;
using Arlecchino.Forms;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample.Views;
using Arlecchino.State;
using Arlecchino.Atoms;

namespace Arlecchino.Sample;

public sealed class SettingsView : IArlecchinoView, IDisposable
{
    private readonly Surface _surface;
    private readonly SettingsStore _settings;
    private readonly AtomHistory _history;
    private readonly ArlecchinoState _state;
    private readonly Form _form;
    private readonly IDisposable _watch;

    public SettingsView(Surface surface, SettingsStore settings, AtomHistory history, ArlecchinoState state, ArlecchinoOptions options)
    {
        _surface = surface;
        _settings = settings;
        _history = history;
        _state = state;

        _form = new(state, options)
        {
            Fields =
            [
                Field.Text(static () => "Profile", settings.Profile,
                    static text => text.Length == 0 ? "must not be empty" : null,
                    static () => "shown in the title bar"),
                Field.Secret(static () => "Passphrase", settings.Passphrase,
                    static () => "never rendered in clear text"),
                Field.Choice(static () => "Theme", ["dark", "light", "high contrast"], settings.Theme,
                    static () => "colour palette of the application"),
                Field.Slider(static () => "Volume", settings.Volume, 0, 100,
                    static () => "←→ inside the modal, or type a number"),
                Field.Toggle(static () => "Fullscreen", settings.Fullscreen,
                    static value => value ? "Yes" : "No"),
                Field.MultiChoice(static () => "Columns", ["Name", "Date Modified", "Size", "Kind"],
                    settings.Columns, static picked => string.Join(", ", picked)),
                Field.Date(static () => "Release", settings.Release,
                    static value => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Field.Color(static () => "Accent", settings.Accent),
                Field.Path(static () => "Folder", settings.Profile, ViewKind.Settings, pickFolder: true,
                    static () => "opens the file picker"),
                Field.Action(static () => "Apply", Apply, () => settings.IsComplete.Value,
                    static () => "enabled once profile and theme are set"),
            ],
        };

        _watch = settings.Summary.Subscribe(() => _state.Output = $"settings: {settings.Summary.Value}");
    }

    public void Draw()
    {
        var content = _surface.Content;
        content.WriteLine(0, "Settings", Theme.Header);
        content.WriteLine(1, _settings.Summary.Value, Theme.Muted);

        _form.Draw(content.Rows(3, content.Height - 3));
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Z when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                _state.Output = _history.Undo() ? "undo" : "nothing to undo";
                return ViewRoute.None;
            case ConsoleKey.Y when key.Modifiers.HasFlag(ConsoleModifiers.Control):
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

    public void Dispose() => _watch.Dispose();

    private static ViewRoute Apply() => ViewKind.Default;
}
