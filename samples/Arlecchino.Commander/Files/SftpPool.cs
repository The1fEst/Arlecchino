using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Arlecchino.Commander.Files;

/// <summary>
/// A handful of SFTP sessions to the same server, handed out one at a time. One session answers one
/// request at a time, so a delete of a hundred files over a link with any latency spends the whole
/// minute waiting; several sessions let the next request leave before the last one has answered.
///
/// Sessions are opened as they are first needed and then kept, because opening one costs a round trip
/// of its own.
/// </summary>
public sealed class SftpPool : IDisposable
{
    private readonly ConcurrentBag<SftpClient> _free = [];
    private readonly ConcurrentBag<SftpClient> _all = [];
    private readonly SemaphoreSlim _slots;
    private readonly Connection _connection;

    /// <summary>Opens the first session, so a server that refuses is reported before anything else.</summary>
    /// <param name="connection">Where to connect and with what.</param>
    /// <param name="size">How many sessions at most.</param>
    public SftpPool(Connection connection, int size)
    {
        _connection = connection;
        _slots = new(size, size);

        var first = Open();

        _all.Add(first);
        _free.Add(first);
    }

    /// <summary>Takes a session, opening one when none is free and the pool is not full yet.</summary>
    /// <returns>The session, given back when the lease is disposed.</returns>
    public Lease Take()
    {
        _slots.Wait();

        if (_free.TryTake(out var free))
        {
            return new(this, free);
        }

        var opened = Open();

        _all.Add(opened);

        return new(this, opened);
    }

    /// <summary>Closes every session.</summary>
    public void Dispose()
    {
        foreach (var client in _all)
        {
            client.Dispose();
        }

        _slots.Dispose();
    }

    private void Give(SftpClient client)
    {
        _free.Add(client);
        _slots.Release();
    }

    private SftpClient Open()
    {
        var client = new SftpClient(Credentials.For(_connection));

        try
        {
            client.Connect();

            return client;
        }
        catch (SshAuthenticationException error)
        {
            client.Dispose();

            throw new UnauthorizedAccessException(error.Message, error);
        }
        catch (Exception error) when (error is SshException or SocketException or IOException)
        {
            client.Dispose();

            throw RemotePaths.AsIoException(error);
        }
    }

    /// <summary>One session, borrowed for as long as the lease is held.</summary>
    public readonly struct Lease : IDisposable
    {
        private readonly SftpPool _pool;

        internal Lease(SftpPool pool, SftpClient client)
        {
            _pool = pool;
            Client = client;
        }

        /// <summary>The session to work on.</summary>
        public SftpClient Client { get; }

        /// <summary>Gives the session back.</summary>
        public void Dispose() => _pool.Give(Client);
    }
}
