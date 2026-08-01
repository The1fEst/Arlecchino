using System;
using System.Collections.Generic;
using System.Globalization;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Modals;
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
internal sealed class NotificationsView : IArlecchinoView
{
    /// <summary>The route it answers to.</summary>
    public const string Route = "Notifications";

    private const int BarCells = 12;
    private const char BarFilled = '█';
    private const char BarEmpty = '░';

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
            OnActivate = Open,
            ItemStyle = static entry => entry.Loudness switch
            {
                NotificationLevel.Failure => Theme.Error,
                NotificationLevel.Warning => Theme.Warning,
                _ => Theme.Default,
            },
        };
    }

    public void Draw()
    {
        var entries = Listed();
        var content = _surface.Content;
        var (header, rest) = content.SplitTop(2);

        header.WriteLine(0, _strings.NotificationsTitle(), Theme.Header);
        header.WriteLine(1, _strings.NotificationsCount(entries.Count), Theme.Muted);

        if (entries.Count == 0)
        {
            rest.WriteLine(0, _strings.NotificationsEmpty(), Theme.Muted);
            return;
        }

        _list.Draw(rest);
    }

    /// <summary>
    /// Hands the list what is held right now. Keys are answered whether or not a frame has been drawn
    /// since the entry arrived, so this runs before drawing and before reading input rather than only
    /// on the way to the screen.
    /// </summary>
    /// <returns>What the list is showing.</returns>
    private IReadOnlyList<Notification> Listed()
    {
        var entries = _state.Notifications.Entries;

        _list.Items = entries;

        return entries;
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key) || _keymap.Notifications.Matches(key))
        {
            return Back();
        }

        Listed();

        if (!_keymap.Erase.Matches(key))
        {
            return _list.Handle(key).Route;
        }

        _state.Notifications.Clear();
        return ViewRoute.None;
    }

    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        Listed();

        return _list.HandleMouse(mouse).Route;
    }

    public (string Key, string Description)[] Hints() =>
    [
        ($"{_keymap.MoveUp}{_keymap.MoveDown}", _strings.FormMove()),
        (_keymap.Confirm.ToString(), _strings.NotificationsOpen()),
        (_keymap.Erase.ToString(), _strings.NotificationsClear()),
        (_keymap.Cancel.ToString(), _strings.NotificationsClose()),
    ];

    private ViewRoute Back()
    {
        _navigator.Back();
        return ViewRoute.None;
    }

    /// <summary>
    /// Opens the entry that was confirmed, because one row is not enough for a report — the dialog
    /// shows the whole of it and offers whatever the entry said could be done about it.
    /// </summary>
    private ViewRoute Open(Notification entry)
    {
        _state.Modal = new NotificationModal
        {
            Title = _strings.NotificationsTitle(),
            Entry = entry,
        };

        return ViewRoute.None;
    }

    /// <summary>
    /// One row: the time it arrived, a small bar when the entry says how far along it is, and what it
    /// has to say. The bar is drawn in the row rather than beside it, because a list draws each row in
    /// one style and the row is what the list gives.
    /// </summary>
    /// <param name="entry">The entry to describe.</param>
    /// <returns>The row.</returns>
    private static string Describe(Notification entry)
    {
        var stamp = entry.Time.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        if (entry.Filled() is not { } share)
        {
            return $" {stamp}  {entry.Line}";
        }

        var filled = (int)Math.Round(share * BarCells);
        var bar = new string(BarFilled, filled) + new string(BarEmpty, Math.Max(0, BarCells - filled));

        return $" {stamp}  {bar} {share * 100:0}%  {entry.Line}";
    }
}
