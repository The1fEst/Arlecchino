using Arlecchino.Views;

namespace Arlecchino.Navigation;

/// <summary>Routes of the screens that ship with the framework.</summary>
public static class Routes
{
    /// <summary>The file picker. Fill <c>ArlecchinoState.FilePicker</c>, then navigate here.</summary>
    public static readonly ViewRoute FilePicker = new(FilePickerView.Route);

    /// <summary>The notifications screen, listing what the application has said lately.</summary>
    public static readonly ViewRoute Notifications = new(NotificationsView.Route);

    /// <summary>The screen listing every key and command.</summary>
    public static readonly ViewRoute Help = new(HelpView.Route);
}
