using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Arlecchino.Commander.Files;
using Arlecchino.Commander.Stores;
using Arlecchino.Commands;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Widgets;
using Renci.SshNet;
using Renci.SshNet.Common;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Commander.Views;

public sealed class SshView : IArlecchinoView
{
    private const int HeaderRows = 2;

    private readonly Surface _surface;
    private readonly Remote _session;
    private readonly ArlecchinoState _state;
    private readonly Spinner _spinner = new();
    private readonly List<string> _lines = [];
    private readonly PaneTree _layout;
    private readonly FocusRing _focus;

    private string _command = "";
    private bool _running;

    public SshView(Surface surface, Remote session, ArlecchinoState state, ArlecchinoOptions options)
    {
        _surface = surface;
        _session = session;
        _state = state;

        var output = new ScrollPane(options.Keymap)
        {
            ContentHeight = () => _lines.Count,
            Content = region =>
            {
                for (var row = 0; row < _lines.Count; row++)
                {
                    region.WriteLine(row, _lines[row], Style(_lines[row]));
                }
            },
        };

        var status = new StatusBar
        {
            Left = [Said],
            Right = [static () => "Enter run", static () => "Esc back"],
        };

        _layout = Branch(
            Rows,
            HeaderRows,
            Leaf(DrawHeader),
            Branch(
                Rows,
                PaneSize.CellsFromEnd(1),
                Leaf(output, static () => "Output"),
                Leaf(status)));

        _focus = _layout.AsFocusRing(options.Keymap);
    }

    public void Draw() => _layout.Draw(_surface.Content);

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Escape, static () => "back", static () => ViewKind.Commander),
        ViewCommand.For(ConsoleKey.Enter, static () => "run a command", Ask),
        ViewCommand.For(new KeyBinding(ConsoleKey.K, ConsoleModifiers.Control), static () => "clear", _lines.Clear),
    ];

    private void DrawHeader(SurfaceRegion header)
    {
        header.WriteLine(0, _session.Ssh is { } ssh ? $"SSH · {ssh.Label}" : "SSH · nothing connected", Theme.Header);
        header.WriteLine(
            1,
            _session.Ssh is null
                ? "Connect a panel over sftp first; those credentials are reused here"
                : $"Last command: {(_command.Length == 0 ? "none yet" : _command)}",
            Theme.Muted);

        if (!_running)
        {
            return;
        }

        _spinner.Advance();
        _spinner.Draw(header.SplitLeft(header.Width - 1).Right);
    }

    private string Said() => _running
        ? $"{_spinner.Current} running"
        : _lines.Count == 0 ? "nothing run yet" : $"{_lines.Count} lines";

    private void Ask()
    {
        if (_running)
        {
            _state.Output = "Still running";
            return;
        }

        if (_session.Ssh is not { } ssh)
        {
            _state.Output = "Connect a panel over sftp first";
            return;
        }

        _state.RequestText($"Run on {ssh.Host}", _command, Filled, command => Run(ssh, command.Trim()));
    }

    private void Run(Connection ssh, string command)
    {
        _command = command;
        _running = true;

        _lines.Add($"$ {command}");

        Task.Run(() =>
        {
            var report = Execute(ssh, command);

            FrameThread.Post(() =>
            {
                _running = false;
                _lines.AddRange(report);
                _state.Invalidate();
            });
        });
    }

    private static IReadOnlyList<string> Execute(Connection ssh, string command)
    {
        try
        {
            using var client = new SshClient(Credentials.For(ssh));

            client.Connect();

            using var running = client.RunCommand(command);
            var lines = new List<string>();

            lines.AddRange(Split(running.Result));
            lines.AddRange(Split(running.Error));
            lines.Add($"[exit {running.ExitStatus}]");

            return lines;
        }
        catch (Exception error) when (error is SshException or SocketException or ObjectDisposedException)
        {
            return [$"[failed] {error.Message}"];
        }
    }

    private static IReadOnlyList<string> Split(string text) => text.Length == 0
        ? []
        : text.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');

    private static IArlecchinoColor Style(string line)
    {
        if (line.StartsWith("$ ", StringComparison.Ordinal))
        {
            return Theme.Accent;
        }

        if (line.StartsWith("[failed]", StringComparison.Ordinal))
        {
            return Theme.Error;
        }

        return line.StartsWith("[exit ", StringComparison.Ordinal) ? Theme.Muted : Theme.Default;
    }

    private static string? Filled(string text) => text.Trim().Length == 0 ? "A command is needed" : null;
}
