using System;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Widgets;

namespace Arlecchino.Views;

/// <summary>
/// What the application has said lately, newest first. The output row only shows the last message and
/// only for a few seconds, so this is where it is read afterwards. Entries leave on their own once
/// they are older than the configured lifetime — the screen shows what is still held, not a log.
/// </summary>
internal class NotificationsView : IArlecchinoView
{
    /// <summary>The route it answers to.</summary>
    public const string Route = "Notifications";

    private readonly Surface _surface;
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly ArlecchinoStrings _strings;
    private readonly Navigator _navigator;
    private readonly ListBox<Notification> _list;

    public NotificationsView(Surface surface, ArlecchinoState state, ArlecchinoOptions options, Navigator navigator)
    {
        _surface = surface;
        _state = state;
        _navigator = navigator;
        _keymap = options.Keymap;
        _strings = options.Strings;

        _list = new(options.Keymap)
        {
            Render = Describe,
            ItemStyle = static entry => entry.Level switch
            {
                NotificationLevel.Failure => Theme.Error,
                NotificationLevel.Warning => Theme.Warning,
                _ => Theme.Default,
            },
        };
    }

    public void Draw()
    {
        var entries = _state.Notifications.Entries;
        var content = _surface.Content;
        var (header, rest) = content.SplitTop(2);

        header.WriteLine(0, _strings.NotificationsTitle(), Theme.Header);
        header.WriteLine(1, _strings.NotificationsCount(entries.Count), Theme.Muted);

        if (entries.Count == 0)
        {
            rest.WriteLine(0, _strings.NotificationsEmpty(), Theme.Muted);
            return;
        }

        _list.Items = entries;
        _list.Draw(rest);
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key) || _keymap.Notifications.Matches(key))
        {
            return Back();
        }

        if (_keymap.Erase.Matches(key))
        {
            _state.Notifications.Clear();
            return ViewRoute.None;
        }

        return _list.Handle(key).Route;
    }

    public ViewRoute HandleMouse(MouseEvent mouse) => _list.HandleMouse(mouse).Route;

    public (string Key, string Description)[] Hints() =>
    [
        ($"{_keymap.MoveUp}{_keymap.MoveDown}", _strings.FormMove()),
        (_keymap.Erase.ToString(), _strings.NotificationsClear()),
        (_keymap.Cancel.ToString(), _strings.NotificationsClose()),
    ];

    private ViewRoute Back()
    {
        _navigator.Back();
        return ViewRoute.None;
    }

    private string Describe(Notification entry) =>
        $" {entry.Time.ToLocalTime():HH:mm:ss}  {entry.Text}";
}
