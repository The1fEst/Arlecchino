using System;
using System.Collections.Generic;
using System.IO;
using Arlecchino.Commander.Model;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Arlecchino.Commander.Files;

public static class Credentials
{
    private static readonly string[] DefaultKeys = ["id_ed25519", "id_ecdsa", "id_rsa", "id_dsa"];

    public static ConnectionInfo For(Connection connection)
    {
        var methods = new List<AuthenticationMethod>();

        foreach (var file in KeyFiles(connection))
        {
            if (Load(file, connection.Password) is { } key)
            {
                methods.Add(new PrivateKeyAuthenticationMethod(connection.User, key));
            }
        }

        if (methods.Count == 0 || connection.Password.Length > 0)
        {
            methods.Add(new PasswordAuthenticationMethod(connection.User, connection.Password));
        }

        return new(connection.Host, connection.Port, connection.User, [.. methods]);
    }

    public static IReadOnlyList<string> KeyFiles(Connection connection)
    {
        if (connection.KeyFile.Length > 0)
        {
            return File.Exists(connection.KeyFile) ? [connection.KeyFile] : [];
        }

        var found = new List<string>();
        var folder = Path.Combine(Listing.Home(), ".ssh");

        foreach (var name in DefaultKeys)
        {
            var path = Path.Combine(folder, name);

            if (File.Exists(path))
            {
                found.Add(path);
            }
        }

        return found;
    }

    private static PrivateKeyFile? Load(string file, string passphrase)
    {
        try
        {
            return passphrase.Length == 0 ? new(file) : new PrivateKeyFile(file, passphrase);
        }
        catch (Exception error) when (error is IOException or SshException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
