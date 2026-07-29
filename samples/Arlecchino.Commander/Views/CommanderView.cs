using System;
using System.Collections.Generic;
using Arlecchino.Commander.Files;
using Arlecchino.Commander.Model;
using Arlecchino.Commander.Stores;
using Arlecchino.Commander.Widgets;
using Arlecchino.Commands;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Commander.Views;

public sealed class CommanderView : IArlecchinoView
{
    private const int FooterRows = 2;
    private const int BarCells = 22;
    private const int SpinnerCells = 2;
    private const string Hints = "Tab panel   Enter open   Space mark   Backspace up";
    private const string StopsHint = "Esc stops";

    private static readonly (string Key, string Label)[] FunctionKeys =
    [
        ("1", "Help"),
        ("2", "Drive"),
        ("3", "View"),
        ("4", "Filter"),
        ("5", "Copy"),
        ("6", "Move"),
        ("7", "Mkdir"),
        ("8", "Delete"),
        ("9", "Menu"),
        ("10", "Quit"),
    ];

    private static readonly string[] PanelItems =
    [
        "Sort by name",
        "Sort by size",
        "Sort by date",
        "Show hidden files",
        "Choose drive",
        "Open a saved host",
        "Connect to a server",
        "Disconnect",
        "Reload",
    ];

    private static readonly string[] FileItems =
    [
        "View",
        "Copy",
        "Move",
        "Rename",
        "Make folder",
        "Delete",
    ];

    private static readonly string[] CommandItems =
    [
        "Swap panels",
        "Both panels here",
        "Filter",
        "Run a command over SSH",
        "Reload both panels",
    ];

    private static readonly string[] OptionItems =
    [
        "Hidden files here",
        "Hidden files there",
        "Notifications",
        "Keys",
    ];

    private static readonly MenuSection[] Sections =
    [
        new("Left", PanelItems),
        new("File", FileItems),
        new("Command", CommandItems),
        new("Options", OptionItems),
        new("Right", PanelItems),
    ];

    private readonly Surface _surface;
    private readonly Panels _panels;
    private readonly Remote _remote;
    private readonly ArlecchinoState _state;
    private readonly IServiceProvider _services;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly FilePanel _left;
    private readonly FilePanel _right;
    private readonly Operations _operations;
    private readonly Spinner _spinner = new();

    private readonly ProgressBar _bar = new()
    {
        Caption = static value => $"{value:0}%",
    };
    private readonly FocusRing _focus;
    private readonly PaneTree _layout;

    private int _seen;

    public CommanderView(
        Surface surface,
        Panels panels,
        Remote remote,
        Operations operations,
        ArlecchinoState state,
        ArlecchinoOptions options,
        IServiceProvider services,
        IHostApplicationLifetime lifetime)
    {
        _surface = surface;
        _panels = panels;
        _remote = remote;
        _operations = operations;
        _state = state;
        _services = services;
        _lifetime = lifetime;

        _left = new(panels.Left, options.Keymap) { OnOpenFile = Open };
        _right = new(panels.Right, options.Keymap) { OnOpenFile = Open };
        _seen = operations.Revision.Value;

        _layout = Branch(
            Rows,
            PaneSize.CellsFromEnd(FooterRows),
            Branch(Columns, Leaf(_left, () => _left.Title), Leaf(_right, () => _right.Title)),
            Branch(Rows, 1, Leaf(DrawStatus), Leaf(DrawFunctionKeys)));

        _focus = _layout.AsFocusRing(options.Keymap);
        _focus.Focus(panels.RightIsActive.Value ? _right : _left);
    }

    /// <summary>
    /// Draws the screen, reloading the panels first when work that was running elsewhere has finished
    /// since the last frame — the operation outlives this screen, so the screen catches up rather than
    /// being told.
    /// </summary>
    public void Draw()
    {
        if (_seen != _operations.Revision.Value)
        {
            _seen = _operations.Revision.Value;

            Active().State.Marks.Clear();
            _left.Reload();
            _right.Reload();
        }

        _layout.Draw(_surface.Content);
    }

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        var route = _focus.Handle(key);

        _panels.RightIsActive.Value = _right.IsFocused;

        return route;
    }

    public ViewRoute HandleMouse(MouseEvent mouse)
    {
        var route = _focus.HandleMouse(mouse);

        _panels.RightIsActive.Value = _right.IsFocused;

        return route;
    }

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.For(ConsoleKey.F2, static () => "drive", ChooseDrive),
        ViewCommand.Navigating(ConsoleKey.F3, static () => "view", View),
        ViewCommand.For(ConsoleKey.F4, static () => "filter", Filter),
        ViewCommand.For(ConsoleKey.F5, static () => "copy", Copy),
        ViewCommand.For(ConsoleKey.F6, static () => "move", Move),
        ViewCommand.For(new KeyBinding(ConsoleKey.F6, ConsoleModifiers.Shift), static () => "rename", Rename),
        ViewCommand.For(ConsoleKey.F7, static () => "make folder", MakeFolder),
        ViewCommand.For(ConsoleKey.F8, static () => "delete", Delete),
        ViewCommand.For(ConsoleKey.F9, static () => "menu", OpenMenu),
        ViewCommand.For(new KeyBinding(ConsoleKey.R, ConsoleModifiers.Control), static () => "reload", Reload),
        ViewCommand.For(new KeyBinding(ConsoleKey.H, ConsoleModifiers.Control), static () => "hidden files",
            ToggleHidden),
        ViewCommand.For(new KeyBinding(ConsoleKey.O, ConsoleModifiers.Control), static () => "swap panels", Swap),
        ViewCommand.For(new KeyBinding(ConsoleKey.K, ConsoleModifiers.Control), static () => "open a saved host",
            () => OpenSaved(Active())),
        ViewCommand.For(ConsoleKey.F10, static () => "quit", _lifetime.StopApplication),
        new()
        {
            Binding = new(ConsoleKey.Escape),
            Label = static () => "stop what is running",
            IsEnabled = () => _operations.IsBusy,
            Run = () =>
            {
                _operations.Cancel();

                return ViewRoute.None;
            },
        },
    ];

    private Navigator Navigation => _services.GetRequiredService<Navigator>();

    private void Copy()
    {
        var from = Active();
        var to = Passive();
        var sources = from.Targets();

        if (Nothing(sources))
        {
            return;
        }

        _state.RequestText(
            $"Copy {Counted(sources)} to {to.Source.Label}",
            to.Folder,
            target => Folder(to, target),
            target => _operations.Copy(from.Source, sources, to.Source, target));
    }

    private void Move()
    {
        var from = Active();
        var to = Passive();
        var sources = from.Targets();

        if (Nothing(sources))
        {
            return;
        }

        _state.RequestText(
            $"Move {Counted(sources)} to {to.Source.Label}",
            to.Folder,
            Filled,
            target => _operations.Move(from.Source, sources, to.Source, target));
    }

    private void Rename()
    {
        var panel = Active();

        if (panel.Current is not { IsParent: false } current)
        {
            return;
        }

        _state.RequestText($"Rename {current.Name} to", current.Name, Filled, name =>
        {
            panel.State.Cursor = panel.Source.NameOf(name);
            _operations.Rename(panel.Source, current, panel.Source.Combine(panel.Folder, name));
        });
    }

    private void MakeFolder()
    {
        var panel = Active();

        _state.RequestText("Create folder", "", Filled, name =>
        {
            if (FileTasks.CreateFolder(panel.Source, panel.Folder, name) is not { } created)
            {
                _state.Output = $"Could not create {name}";
                return;
            }

            panel.State.Cursor = panel.Source.NameOf(created);
            panel.Reload();
        });
    }

    private void Delete()
    {
        var panel = Active();
        var sources = panel.Targets();

        if (Nothing(sources))
        {
            return;
        }

        _state.RequestConfirmation(
            $"Delete {Counted(sources)}?",
            () => _operations.Delete(panel.Source, sources));
    }

    private bool Nothing(IReadOnlyList<FileEntry> sources)
    {
        if (sources.Count > 0)
        {
            return false;
        }

        _state.Output = "Nothing selected";

        return true;
    }

    private static string Counted(IReadOnlyList<FileEntry> sources) =>
        sources.Count == 1 ? sources[0].Name : $"{sources.Count} items";

    private static string? Filled(string text) => text.Trim().Length == 0 ? "A name is needed" : null;

    private static string? Folder(FilePanel panel, string target) =>
        panel.Source.IsRemote || panel.Source.FolderExists(target) ? Filled(target) : "That folder does not exist";

    private FilePanel Active() => _right.IsFocused ? _right : _left;

    private FilePanel Passive() => _right.IsFocused ? _left : _right;

    private ViewRoute Open(FileEntry entry)
    {
        var panel = Active();

        panel.State.Cursor = entry.Name;
        _panels.Viewing.Value = entry.Path;
        _panels.ViewingSource = panel.Source;
        _panels.ViewingSize = entry.Size;

        return ViewKind.Viewer;
    }

    private ViewRoute View()
    {
        var panel = Active();

        if (panel.Current is { } current)
        {
            return panel.Activate(current);
        }

        _state.Output = "Nothing to open";

        return ViewRoute.None;
    }

    private void ChooseDrive() => ChooseDrive(Active());

    private void ChooseDrive(FilePanel panel) =>
        _state.RequestChoice("Drive", Listing.Drives(), panel.GoTo, panel.Folder);

    private void Filter() => Filter(Active());

    private void Filter(FilePanel panel) =>
        _state.RequestText("Show only names containing", panel.State.Filter, null, text =>
        {
            panel.State.Filter = text.Trim();
            panel.Reload();
        });

    private void OpenMenu()
    {
        var titles = new List<string>(Sections.Length);

        foreach (var section in Sections)
        {
            titles.Add(section.Title);
        }

        _state.RequestChoice("Menu", titles, OpenSection);
    }

    private void OpenSection(string title)
    {
        for (var index = 0; index < Sections.Length; index++)
        {
            if (Sections[index].Title != title)
            {
                continue;
            }

            var section = Sections[index];
            var chosen = index;

            _state.RequestChoice(section.Title, section.Items, item => Run(chosen, item));
            return;
        }
    }

    private void Run(int section, string item)
    {
        switch (section)
        {
            case 0:
                RunForPanel(_left, item);
                break;
            case 1:
                RunForFile(item);
                break;
            case 2:
                RunForBoth(item);
                break;
            case 3:
                RunForOptions(item);
                break;
            default:
                RunForPanel(_right, item);
                break;
        }
    }

    private void RunForPanel(FilePanel panel, string item)
    {
        switch (item)
        {
            case "Sort by name":
                panel.SortBy(Sorting.Name);
                break;
            case "Sort by size":
                panel.SortBy(Sorting.Size);
                break;
            case "Sort by date":
                panel.SortBy(Sorting.Modified);
                break;
            case "Show hidden files":
                ToggleHidden(panel);
                break;
            case "Choose drive":
                ChooseDrive(panel);
                break;
            case "Open a saved host":
                OpenSaved(panel);
                break;
            case "Connect to a server":
                Connect(panel);
                break;
            case "Disconnect":
                Disconnect(panel);
                break;
            default:
                panel.Reload();
                break;
        }
    }

    private void RunForFile(string item)
    {
        switch (item)
        {
            case "View":
                Navigation.Apply(View());
                break;
            case "Copy":
                Copy();
                break;
            case "Move":
                Move();
                break;
            case "Rename":
                Rename();
                break;
            case "Make folder":
                MakeFolder();
                break;
            default:
                Delete();
                break;
        }
    }

    private void RunForBoth(string item)
    {
        switch (item)
        {
            case "Swap panels":
                Swap();
                break;
            case "Both panels here":
                Passive().GoTo(Active().Folder);
                break;
            case "Filter":
                Filter(Active());
                break;
            case "Run a command over SSH":
                Navigation.Apply(ViewKind.Ssh);
                break;
            default:
                Reload();
                break;
        }
    }

    private void RunForOptions(string item)
    {
        switch (item)
        {
            case "Hidden files here":
                ToggleHidden(_left);
                break;
            case "Hidden files there":
                ToggleHidden(_right);
                break;
            case "Notifications":
                Navigation.Apply(Routes.Notifications);
                break;
            default:
                Navigation.Apply(Routes.Help);
                break;
        }
    }

    private void OpenSaved(FilePanel panel)
    {
        var saved = SshConfig.Hosts();

        if (saved.Count == 0)
        {
            _state.Output = $"No hosts in {SshConfig.Location}";
            return;
        }

        var listed = new List<string>(saved.Count);

        foreach (var host in saved)
        {
            listed.Add(host.Describe());
        }

        _state.RequestChoice("Saved hosts", listed, chosen =>
        {
            for (var index = 0; index < listed.Count; index++)
            {
                if (listed[index] != chosen)
                {
                    continue;
                }

                Dial(panel, saved[index], saved[index].AsConnection(""));
                return;
            }
        });
    }

    private void Dial(FilePanel panel, SshHost host, Connection wanted)
    {
        _state.Output = $"Connecting to {host.Alias}…";

        Connector.Start(
            wanted,
            (source, folder) =>
            {
                _remote.Ssh = wanted;
                panel.Connect(source, folder);
                _state.Output = $"{host.Alias} · {folder}";
            },
            (message, denied) => AskPassword(panel, host, wanted, message, denied));
    }

    private void AskPassword(FilePanel panel, SshHost host, Connection wanted, string message, bool denied)
    {
        if (!denied || wanted.Password.Length > 0)
        {
            _state.RequestMessage($"Could not open {host.Alias}", message);
            return;
        }

        _state.Output = message;
        _state.RequestPassword(
            $"Password for {host.User}@{host.HostName}",
            password => Dial(panel, host, wanted with { Password = password }));
    }

    private void Connect(FilePanel panel)
    {
        _panels.RightIsActive.Value = ReferenceEquals(panel, _right);
        Navigation.Apply(ViewKind.Connect);
    }

    private void Disconnect(FilePanel panel)
    {
        if (!panel.Source.IsRemote)
        {
            _state.Output = "That panel is already local";
            return;
        }

        var label = panel.Source.Label;

        panel.Connect(new LocalSource(), Environment.CurrentDirectory);

        _state.Output = $"Disconnected from {label}";
    }

    private void ToggleHidden() => ToggleHidden(Active());

    private void ToggleHidden(FilePanel panel)
    {
        panel.State.ShowHidden = !panel.State.ShowHidden;
        panel.Reload();

        _state.Output = panel.State.ShowHidden ? "Hidden files shown" : "Hidden files skipped";
    }

    private void Swap()
    {
        var here = _left.Folder;

        _left.GoTo(_right.Folder);
        _right.GoTo(here);
    }

    private void Reload()
    {
        _left.Reload();
        _right.Reload();

        _state.Output = "Reloaded";
    }

    private void DrawStatus(SurfaceRegion status)
    {
        if (_operations.IsBusy)
        {
            DrawProgress(status);
            return;
        }

        var said = _state.Output;

        if (said.Length > 0)
        {
            status.WriteLine(0, said, Theme.Warning);
            return;
        }

        var panel = Active();
        var room = Math.Max(0, status.Width - Hints.Length - 3);

        var free = panel.Source.Free(panel.Folder);
        var where = free.Length == 0 ? panel.Folder : $"{panel.Folder}  ·  {free}";

        status.Write(0, 0, TextWidth.Truncate(where, room), Theme.Muted);
        status.WriteLine(0, Hints, Theme.Muted, Align.Right);
    }

    /// <summary>
    /// The row while something is running: a spinner for the counting pass, which has no denominator
    /// yet, then a bar with the percentage beside it and whatever is being worked on now.
    /// </summary>
    private void DrawProgress(SurfaceRegion status)
    {
        _spinner.Advance();
        status.Write(0, 0, _spinner.Current, Theme.Accent);

        var text = _operations.Progress();
        var column = SpinnerCells;

        if (_operations.IsMeasured)
        {
            _bar.Value = (decimal)(_operations.Share * 100);
            _bar.Draw(status.Rows(0, 1).Inset(new Margin(column, 0, Math.Max(0, status.Width - column - BarCells), 0)));

            column += BarCells + 1;
        }

        var room = Math.Max(0, status.Width - column - StopsHint.Length - 2);

        status.Write(0, column, TextWidth.Truncate(text, room), Theme.Accent);
        status.WriteLine(0, StopsHint, Theme.Muted, Align.Right);
    }

    private static void DrawFunctionKeys(SurfaceRegion keys)
    {
        var cell = keys.Width / FunctionKeys.Length;

        if (cell < 4)
        {
            return;
        }

        for (var index = 0; index < FunctionKeys.Length; index++)
        {
            var (key, label) = FunctionKeys[index];
            var column = index * cell;

            keys.Write(0, column, key, Theme.Muted);
            keys.Write(0, column + key.Length, Fit(label, cell - key.Length), Theme.Selected);
        }
    }

    private static string Fit(string label, int width) =>
        TextWidth.PadRight(TextWidth.Truncate(label, width), width);
}
