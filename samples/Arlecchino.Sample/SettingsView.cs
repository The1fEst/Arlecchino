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

        var apply = Fields.Action(static () => Loc(LocString.Apply), Apply)
            .WithHelp(static () => Loc(LocString.ApplyHelp));

        _form = new(state, options)
        {
            Fields =
            [
                Fields.Text(static () => Loc(LocString.Profile), settings.Profile)
                    .WithValidation(static text => !string.IsNullOrWhiteSpace(text))
                    .WithHelp(static () => Loc(LocString.ProfileHelp)),
                Fields.Secret(static () => Loc(LocString.Passphrase), settings.Passphrase)
                    .WithHelp(static () => Loc(LocString.PassphraseHelp)),
                Fields.Choice(static () => Loc(LocString.Theme), ["dark", "light", "high contrast"], settings.Theme)
                    .WithHelp(static () => Loc(LocString.ThemeHelp)),
                Fields.Choice(static () => Loc(LocString.LanguageLabel), ["auto", .. Languages], settings.Language)
                    .WithHelp(static () => Loc(LocString.LanguageHelp)),
                Fields.Slider(static () => Loc(LocString.Volume), settings.Volume)
                    .WithItems(static () => [.. Enumerable.Range(0, 101)])
                    .WithHelp(static () => Loc(LocString.VolumeHelp)),
                Fields.Toggle(static () => Loc(LocString.Fullscreen), settings.Fullscreen),
                Fields.MultiChoice(static () => Loc(LocString.Columns), ["Name", "Date Modified", "Size", "Kind"], settings.Columns),
                Fields.Date(static () => Loc(LocString.Release), settings.Release),
                Fields.Color(static () => Loc(LocString.Accent), settings.Accent),
                Fields.Path(static () => Loc(LocString.Folder), settings.Profile, ViewKind.Settings, pickFolder: true)
                    .WithHelp(static () => Loc(LocString.FolderHelp)),
                apply,
            ],
        };

        _watch = settings.Summary.Subscribe(() => _state.Output = Loc(LocString.SettingsOutput, settings.Summary.Value));
        _complete = settings.IsComplete.Subscribe(() => apply.IsEnabled = settings.IsComplete.Value);
        apply.IsEnabled = settings.IsComplete.Value;
    }

    public void Draw()
    {
        var content = _surface.Content;
        content.WriteLine(0, Loc(LocString.SettingsTitle), Theme.Header);
        content.WriteLine(1, _settings.Summary.Value, Theme.Secondary);
        content.WriteLine(2, FormatSystemLanguage(), Theme.Secondary);

        _form.Draw(content.Rows(4, content.Height - 4));
    }

    public ViewRoute Handle(KeyPress key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Z when key.Modifiers.HasFlag(KeyModifiers.Control):
                _state.Output = Loc(_history.Undo() ? LocString.Undo : LocString.NothingToUndo);
                return ViewRoute.None;
            case ConsoleKey.Y when key.Modifiers.HasFlag(KeyModifiers.Control):
                _state.Output = Loc(_history.Redo() ? LocString.Redo : LocString.NothingToRedo);
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
        ("Ctrl+Z", Loc(LocString.Undo)),
        ("Ctrl+Y", Loc(LocString.Redo)),
        ("Esc", Loc(LocString.Back)),
    ];

    public void Dispose()
    {
        _watch.Dispose();
        _complete.Dispose();
    }

    private static ViewRoute Apply() => ViewKind.Default;

    private static string FormatSystemLanguage()
    {
        var languages = SystemLanguages.GetPreferred();

        return Loc(LocString.SystemLanguageInfo,
            languages.Count == 0 ? Loc(LocString.NothingReported) : string.Join(", ", languages),
            Localization.Language);
    }
}
