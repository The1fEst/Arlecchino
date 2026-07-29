using System;
using System.Collections.Generic;
using System.IO;
using Arlecchino.Commander.Files;
using Arlecchino.Commander.Model;
using Arlecchino.Commander.Stores;
using Arlecchino.Commands;
using Arlecchino.Focus;
using Arlecchino.Forms;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Widgets;
using Microsoft.Extensions.DependencyInjection;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Commander.Views;

public sealed class ConnectView : IArlecchinoView, IDisposable
{
    private const int HeaderRows = 3;
    private const int FormRows = 9;
    private const int LowestPort = 1;
    private const int HighestPort = 65535;

    private static readonly string[] Schemes = ["sftp", "ftp"];

    private readonly Surface _surface;
    private readonly Remote _session;
    private readonly Panels _panels;
    private readonly ArlecchinoState _state;
    private readonly IServiceProvider _services;
    private readonly Spinner _spinner = new();
    private readonly FocusRing _focus;
    private readonly PaneTree _layout;
    private readonly IDisposable _watchingScheme;
    private readonly IDisposable _watchingSaved;

    public ConnectView(
        Surface surface,
        Remote session,
        Panels panels,
        ArlecchinoState state,
        ArlecchinoOptions options,
        IServiceProvider services)
    {
        _surface = surface;
        _session = session;
        _panels = panels;
        _state = state;
        _services = services;

        var saved = SshConfig.Hosts();

        var form = new Form(state, options)
        {
            Fields =
            [
                Field.Choice(static () => "Saved host", Aliases(saved), session.Saved,
                    static () => "a Host entry from ~/.ssh/config; picking one fills the rest in"),
                Field.Choice(static () => "Protocol", Schemes, session.Scheme,
                    static () => "sftp goes over SSH, ftp is plain"),
                Field.Text(static () => "Host", session.Host, Filled, static () => "name or address of the server"),
                Field.Number(static () => "Port", session.Port, LowestPort, HighestPort,
                    static () => "22 for sftp, 21 for ftp"),
                Field.Text(static () => "User", session.User, Filled),
                Field.Secret(static () => "Password", session.Password,
                    static () => "the passphrase when a key file is given; kept in memory only"),
                Field.PathFrom(static () => "Key file", session.KeyFile, ViewKind.Connect, false, Keys,
                    static () => "an OpenSSH private key for sftp; empty means password login"),
                Field.Text(static () => "Folder", session.Folder, null,
                    static () => "where the panel opens; empty means the home folder"),
                Field.Action(static () => "Connect", Start, () => !session.Connecting.Value && Ready(session)),
            ],
        };

        var status = new StatusBar
        {
            Left = [Said],
            Right = [static () => "Esc back"],
        };

        _layout = Branch(
            Rows,
            HeaderRows,
            Leaf(DrawHeader),
            Branch(Rows, FormRows, Leaf(form), Leaf(status)));

        _focus = _layout.AsFocusRing(options.Keymap);
        _watchingSaved = session.Saved.Subscribe(() => Fill(saved, session));
        _watchingScheme = session.Scheme.Subscribe(() =>
            session.Port.Value = Connection.PortFor(session.Scheme.Value == "ftp" ? Protocol.Ftp : Protocol.Sftp));
    }

    public void Draw() => _layout.Draw(_surface.Content);

    public void Dispose()
    {
        _watchingScheme.Dispose();
        _watchingSaved.Dispose();
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Escape, static () => "back", static () => ViewKind.Commander),
    ];

    private void DrawHeader(SurfaceRegion header)
    {
        var side = _panels.RightIsActive.Value ? "right" : "left";

        header.WriteLine(0, $"Connect the {side} panel", Theme.Header);
        header.WriteLine(1, "The panel keeps browsing the server until it is disconnected", Theme.Muted);

        if (!_session.Connecting.Value)
        {
            return;
        }

        _spinner.Advance();
        _spinner.Draw(header.SplitLeft(header.Width - 1).Right);
    }

    private string Said()
    {
        if (_session.Connecting.Value)
        {
            return $"{_spinner.Current} connecting to {_session.Host.Value}";
        }

        return _session.Failure.Value.Length > 0 ? _session.Failure.Value : "nothing connected yet";
    }

    private ViewRoute Start()
    {
        var wanted = _session.Wanted();

        _session.Connecting.Value = true;
        _session.Failure.Value = "";

        Connector.Start(wanted, (source, folder) => Landed(wanted, source, folder), Failed);

        return ViewRoute.None;
    }

    private void Landed(Connection connection, IFileSource source, string folder)
    {
        _session.Connecting.Value = false;

        if (connection.Protocol == Protocol.Sftp)
        {
            _session.Ssh = connection;
        }

        Side().Connect(source, folder);

        _state.Output = $"Connected to {connection.Label}";
        _services.GetRequiredService<Navigator>().Apply(ViewKind.Commander);
    }

    private void Failed(string message, bool denied)
    {
        _session.Connecting.Value = false;
        _session.Failure.Value = message;

        _state.RequestMessage(denied ? "The server refused those credentials" : "Could not connect", message);
    }

    private PanelState Side() => _panels.RightIsActive.Value ? _panels.Right : _panels.Left;

    private static string? Filled(string text) => text.Trim().Length == 0 ? "This one is needed" : null;

    private static string Keys() => Path.Combine(Listing.Home(), ".ssh");

    private static IReadOnlyList<string> Aliases(IReadOnlyList<SshHost> saved)
    {
        var names = new List<string>(saved.Count + 1) { "" };

        foreach (var host in saved)
        {
            names.Add(host.Alias);
        }

        return names;
    }

    private static void Fill(IReadOnlyList<SshHost> saved, Remote session)
    {
        foreach (var host in saved)
        {
            if (host.Alias != session.Saved.Value)
            {
                continue;
            }

            session.Fill(host);
            return;
        }
    }

    private static bool Ready(Remote session) =>
        session.Host.Value.Trim().Length > 0 && session.User.Value.Trim().Length > 0;
}
