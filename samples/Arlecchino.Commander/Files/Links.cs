using System;

namespace Arlecchino.Commander.Files;

public static class Links
{
    public static Connection Parse(string link)
    {
        if (!link.Contains("://", StringComparison.Ordinal))
        {
            return Saved(link);
        }

        var address = new Uri(link);
        var protocol = address.Scheme == "ftp" ? Protocol.Ftp : Protocol.Sftp;
        var credentials = address.UserInfo.Split(':', 2);

        return new(
            protocol,
            address.Host,
            address.IsDefaultPort ? Connection.PortFor(protocol) : address.Port,
            Uri.UnescapeDataString(credentials[0]),
            credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : "",
            address.AbsolutePath.Length > 1 ? address.AbsolutePath : RemotePaths.Root,
            KeyFrom(address.Query));
    }

    private static Connection Saved(string alias)
    {
        foreach (var host in SshConfig.Hosts())
        {
            if (host.Alias == alias)
            {
                return host.AsConnection("");
            }
        }

        throw new ArgumentException(
            $"{alias} is neither a link nor a Host in {SshConfig.Location}",
            nameof(alias));
    }

    private static string KeyFrom(string query)
    {
        const string marker = "key=";
        var start = query.IndexOf(marker, StringComparison.Ordinal);

        if (start < 0)
        {
            return "";
        }

        var rest = query[(start + marker.Length)..];
        var end = rest.IndexOf('&');

        return Uri.UnescapeDataString(end < 0 ? rest : rest[..end]);
    }
}
