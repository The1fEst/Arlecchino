using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Arlecchino.Commander.Model;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Arlecchino.Commander.Files;

public sealed class SftpSource : IFileSource
{
    private const int Sessions = 8;

    private readonly SftpPool _pool;
    private readonly Connection _connection;
    private readonly Lock _shellGate = new();

    private SshClient? _shell;
    private bool _shellRefused;
    private RemoteShellKind _kind = RemoteShellKind.Unknown;

    private SftpSource(Connection connection, SftpPool pool)
    {
        _connection = connection;
        _pool = pool;
    }

    public string Label => _connection.Label;

    public bool IsRemote => true;

    public int Concurrency => Sessions;

    public string Home
    {
        get
        {
            using var lease = _pool.Take();
            var client = lease.Client;

            return Guarded(() => client.WorkingDirectory);
        }
    }

    public static SftpSource Connect(Connection connection) => new(connection, new(connection, Sessions));

    public string Combine(string folder, string name) => RemotePaths.Combine(folder, name);

    public string? Parent(string folder) => RemotePaths.Parent(folder);

    public string NameOf(string path) => RemotePaths.NameOf(path);

    public bool FolderExists(string folder)
    {
        using var lease = _pool.Take();
        var client = lease.Client;

        try
        {
            return client.Exists(folder) && client.GetAttributes(folder).IsDirectory;
        }
        catch (Exception error) when (error is SshException or ObjectDisposedException)
        {
            return false;
        }
    }

    public string Free(string folder) => "";

    public IReadOnlyList<FileEntry> List(string folder, bool showHidden)
    {
        using var lease = _pool.Take();
        var client = lease.Client;

        return Guarded(() =>
        {
            var entries = new List<FileEntry>();

            if (RemotePaths.Parent(folder) is { } parent)
            {
                entries.Add(new("..", parent, true, true, 0, default, false, false));
            }

            foreach (var found in client.ListDirectory(folder))
            {
                if (found.Name is "." or "..")
                {
                    continue;
                }

                var hidden = RemotePaths.IsHidden(found.Name);

                if (hidden && !showHidden)
                {
                    continue;
                }

                entries.Add(new(
                    found.Name,
                    found.FullName,
                    found.IsDirectory,
                    false,
                    found.IsDirectory ? 0 : found.Length,
                    found.LastWriteTime,
                    hidden,
                    false));
            }

            return (IReadOnlyList<FileEntry>)entries;
        });
    }

    public Stream OpenRead(string path)
    {
        var lease = _pool.Take();
        var client = lease.Client;

        return Leased(lease, () => client.OpenRead(path));
    }

    public Stream Create(string path)
    {
        var lease = _pool.Take();
        var client = lease.Client;

        return Leased(lease, () => client.Create(path));
    }

    public void CreateFolder(string path)
    {
        using var lease = _pool.Take();
        var client = lease.Client;

        Guarded(() =>
        {
            if (!client.Exists(path))
            {
                client.CreateDirectory(path);
            }
        });
    }

    public void Delete(FileEntry entry)
    {
        using var lease = _pool.Take();
        var client = lease.Client;

        Guarded(() =>
        {
            if (entry.IsFolder)
            {
                client.DeleteDirectory(entry.Path);
                return;
            }

            client.DeleteFile(entry.Path);
        });
    }

    /// <summary>
    /// Removes the tree with one <c>rm -rf</c> over SSH rather than a request per file. A server on
    /// the other side of the world answers a delete in a fraction of a second, which a thousand times
    /// over is a coffee break; the same tree goes in one round trip this way.
    /// </summary>
    /// <param name="entry">The folder to remove.</param>
    /// <returns><c>false</c> when the server has no shell to run it, leaving the tree to be walked.</returns>
    public bool TryDeleteTree(FileEntry entry)
    {
        if (!entry.IsFolder)
        {
            return false;
        }

        lock (_shellGate)
        {
            if (Shell() is not { } shell || RemoteShells.Sweep(Kind(shell), entry.Path) is not { } command)
            {
                return false;
            }

            try
            {
                using var running = shell.RunCommand(command);

                return running.ExitStatus == 0;
            }
            catch (Exception error) when (IsShellFailure(error))
            {
                _shellRefused = true;

                return false;
            }
        }
    }

    /// <summary>
    /// What answers on the other side. A Unix server is a Unix server, but an OpenSSH server on
    /// Windows hands the command to whatever was set as its default shell — <c>cmd.exe</c> on a stock
    /// install, PowerShell on many others — and the three of them share no way of removing a folder.
    /// Asked once per connection and remembered.
    /// </summary>
    private RemoteShellKind Kind(SshClient shell)
    {
        if (_kind != RemoteShellKind.Unknown)
        {
            return _kind;
        }

        _kind = RemoteShells.Ask(question =>
        {
            try
            {
                using var running = shell.RunCommand(question);

                return (running.Result + running.Error, running.ExitStatus ?? -1);
            }
            catch (Exception error) when (IsShellFailure(error))
            {
                return ("", -1);
            }
        });

        return _kind;
    }

    /// <summary>
    /// The shell session, opened once and kept. Opening one costs a round trip, which is the whole
    /// saving when a hundred small folders are removed one after another.
    /// </summary>
    /// <returns>The session, or <c>null</c> when the server would not give one.</returns>
    private SshClient? Shell()
    {
        if (_shellRefused)
        {
            return null;
        }

        if (_shell is { IsConnected: true })
        {
            return _shell;
        }

        try
        {
            _shell?.Dispose();
            _shell = new(Credentials.For(_connection));
            _shell.Connect();

            return _shell;
        }
        catch (Exception error) when (IsShellFailure(error))
        {
            _shell = null;
            _shellRefused = true;

            return null;
        }
    }

    private static bool IsShellFailure(Exception error) =>
        error is SshException or SocketException or IOException or ObjectDisposedException
            or UnauthorizedAccessException;

    public void Move(string from, string to)
    {
        using var lease = _pool.Take();
        var client = lease.Client;

        Guarded(() => client.RenameFile(from, to));
    }

    public void Dispose()
    {
        lock (_shellGate)
        {
            _shell?.Dispose();
            _shell = null;
        }

        _pool.Dispose();
    }

    private static Stream Leased(SftpPool.Lease lease, Func<Stream> open)
    {
        try
        {
            return new LeasedStream(Guarded(open), lease);
        }
        catch
        {
            lease.Dispose();
            throw;
        }
    }

    private static T Guarded<T>(Func<T> work)
    {
        try
        {
            return work();
        }
        catch (Exception error) when (error is SshException or ObjectDisposedException or SocketException)
        {
            throw RemotePaths.AsIoException(error);
        }
    }

    private static void Guarded(Action work) => Guarded<object?>(() =>
    {
        work();
        return null;
    });
}
